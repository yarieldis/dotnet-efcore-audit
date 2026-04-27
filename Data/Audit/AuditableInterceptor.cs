using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

using Unctad.eRegulations.Library.Model;
using Unctad.eRegulations.Library.Data.Audit.Caching;
using Unctad.eRegulations.Library.Data.Audit.Configuration;
using Unctad.eRegulations.Library.Data.Audit.Context;
using Unctad.eRegulations.Library.Data.Audit.ErrorHandling;
using Unctad.eRegulations.Library.Data.Audit.Services;

namespace Data.Audit;

/// <summary>
/// Entity Framework interceptor for auditing changes to entities that implement IAuditable.
/// Processes IAuditableCollection items in a first pass (before parent scalar fields),
/// sourcing AuditDate/UserName from the parent IAuditable entity.
/// </summary>
public sealed class AuditableInterceptor(
    IAuditRecordFactory auditRecordFactory,
    IAuditFieldProcessor fieldProcessor,
    IAuditErrorHandler errorHandler,
    IAuditPropertyCache propertyCache,
    AuditConfiguration configuration,
    ILogger<AuditableInterceptor>? logger = null) : SaveChangesInterceptor
{
    private readonly ILogger<AuditableInterceptor>? _logger = logger;
    private readonly IAuditRecordFactory _auditRecordFactory = auditRecordFactory ?? throw new ArgumentNullException(nameof(auditRecordFactory));
    private readonly IAuditFieldProcessor _fieldProcessor = fieldProcessor ?? throw new ArgumentNullException(nameof(fieldProcessor));
    private readonly IAuditErrorHandler _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
    private readonly IAuditPropertyCache _propertyCache = propertyCache ?? throw new ArgumentNullException(nameof(propertyCache));
    private readonly AuditConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            try
            {
                CreateAuditableRecords(eventData.Context);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred while creating audit records during SaveChanges");
                throw;
            }
        }
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            try
            {
                CreateAuditableRecords(eventData.Context);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred while creating audit records during SaveChangesAsync");
                throw;
            }
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void CreateAuditableRecords(DbContext context)
    {
        // PASS 1: collection items — must run before parent records are created
        ProcessCollectionItems(context);

        // PASS 2: parent IAuditable entities
        var entities = context.ChangeTracker.Entries<IAuditable>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (EntityEntry<IAuditable> entry in entities)
        {
            try
            {
                var auditAction = entry.State switch
                {
                    EntityState.Added => AuditAction.Insert,
                    EntityState.Modified => AuditAction.Update,
                    EntityState.Deleted => AuditAction.Delete,
                    _ => throw new InvalidOperationException(
                        $"Unsupported entity state '{entry.State}' for auditing.")
                };

                var auditContext = new AuditContext(context, entry, auditAction, _configuration);
                var auditRecord = _auditRecordFactory.CreateAuditRecord(auditContext);

                context.Set<AuditRecord>().Add(auditRecord);

                if (entry.State == EntityState.Modified)
                {
                    foreach (var auditField in _fieldProcessor.ProcessFields(auditContext, auditRecord))
                        context.Set<AuditRecordField>().Add(auditField);
                }

                if (_configuration.EnableDetailedLogging)
                    _logger?.LogDebug(
                        "Created audit record for {EntityType} ID {EntityId} Action {Action}",
                        entry.Entity.GetType().Name, entry.Entity.Id, auditAction);
            }
            catch (Exception ex)
            {
                var auditContext = new AuditContext(context, entry, AuditAction.Init, _configuration);
                _errorHandler.HandleAuditError(ex, auditContext);
                throw;
            }
        }
    }

    private void ProcessCollectionItems(DbContext context)
    {
        var collectionEntries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is IAuditableCollection
                     && (e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
            .ToList();

        foreach (var entry in collectionEntries)
        {
            try
            {
                var item = (IAuditableCollection)entry.Entity;
                var itemType = item.GetType();

                // Find the IAuditable parent that has [AuditCollectionField] for this item type
                var parentEntity = entry.References
                    .Where(r => r.CurrentValue is IAuditable)
                    .Select(r => (IAuditable)r.CurrentValue!)
                    .FirstOrDefault(parent =>
                        _propertyCache.GetAuditableCollectionProperties(parent.GetType())
                            .Any(p => GetCollectionElementType(p.PropertyType) == itemType));

                if (parentEntity is null)
                    continue; // not opted in or parent not loaded

                var itemAction = entry.State switch
                {
                    EntityState.Added => AuditAction.Insert,
                    EntityState.Modified => AuditAction.Update,
                    EntityState.Deleted => AuditAction.Delete,
                    _ => throw new InvalidOperationException(
                        $"Unsupported state '{entry.State}' for collection item auditing.")
                };

                var itemContext = new AuditCollectionItemContext(
                    context, entry, item, itemAction, _configuration);

                var itemRecord = _auditRecordFactory.CreateCollectionItemAuditRecord(
                    parentEntity, itemContext);

                context.Set<AuditRecord>().Add(itemRecord);

                if (entry.State == EntityState.Modified)
                {
                    foreach (var field in _fieldProcessor.ProcessCollectionItemFields(itemContext, itemRecord))
                        context.Set<AuditRecordField>().Add(field);
                }

                if (_configuration.EnableDetailedLogging)
                    _logger?.LogDebug(
                        "Created collection audit record for {Parent}.{Item} ID {Id} Action {Action}",
                        parentEntity.GetType().Name, itemType.Name, item.Id, itemAction);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex,
                    "Error creating collection audit record for {ItemType}",
                    entry.Entity.GetType().Name);

                if (!_configuration.ContinueOnFieldProcessingError)
                    throw;
            }
        }
    }

    private static Type? GetCollectionElementType(Type propertyType)
    {
        if (!propertyType.IsGenericType) return null;
        var elementType = propertyType.GetGenericArguments().FirstOrDefault();
        return elementType is not null
               && typeof(IAuditableCollection).IsAssignableFrom(elementType)
            ? elementType
            : null;
    }
}
