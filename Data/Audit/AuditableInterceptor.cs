using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data.Audit.Configuration;
using Data.Audit.Context;
using Data.Audit.ErrorHandling;
using Data.Audit.Services;
using Model;

namespace Data.Audit;

/// <summary>
/// Entity Framework interceptor for auditing changes to entities that implement IAuditable.
/// </summary>
public sealed class AuditableInterceptor(
    IAuditRecordFactory auditRecordFactory,
    IAuditFieldProcessor fieldProcessor,
    IAuditErrorHandler errorHandler,
    AuditConfiguration configuration,
    ILogger<AuditableInterceptor>? logger = null) : SaveChangesInterceptor
{
    private readonly ILogger<AuditableInterceptor>? _logger = logger;
    private readonly IAuditRecordFactory _auditRecordFactory = auditRecordFactory ?? throw new ArgumentNullException(nameof(auditRecordFactory));
    private readonly IAuditFieldProcessor _fieldProcessor = fieldProcessor ?? throw new ArgumentNullException(nameof(fieldProcessor));
    private readonly IAuditErrorHandler _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
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
                    _ => throw new InvalidOperationException($"Unsupported entity state '{entry.State}' for auditing.")
                };

                var auditContext = new AuditContext(context, entry, auditAction, _configuration);
                var auditRecord = _auditRecordFactory.CreateAuditRecord(auditContext);

                context.Set<AuditRecord>().Add(auditRecord);

                // Only process field-level auditing for modifications
                if (entry.State == EntityState.Modified)
                {
                    var auditFields = _fieldProcessor.ProcessFields(auditContext, auditRecord);
                    foreach (var auditField in auditFields)
                    {
                        context.Set<AuditRecordField>().Add(auditField);
                    }
                }

                if (_configuration.EnableDetailedLogging)
                {
                    _logger?.LogDebug("Created audit record for {EntityType} with ID {EntityId}, Action: {Action}",
                        entry.Entity.GetType().Name, entry.Entity.Id, auditAction);
                }
            }
            catch (Exception ex)
            {
                var auditContext = new AuditContext(context, entry, AuditAction.Init, _configuration);
                _errorHandler.HandleAuditError(ex, auditContext);
                throw;
            }
        }
    }
}
