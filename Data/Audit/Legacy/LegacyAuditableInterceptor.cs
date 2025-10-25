using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Data.Audit.Caching;
using Data.Audit.Configuration;
using Data.Audit.Context;
using Data.Audit.ErrorHandling;
using Data.Audit.Services;
using Data.Audit.TypeHandlers;
using Model;

namespace Data.Audit.Legacy;

/// <summary>
/// Legacy version of AuditableInterceptor that maintains backward compatibility.
/// This class creates its own dependencies internally and doesn't require DI.
/// For new projects, use the main AuditableInterceptor with proper DI setup.
/// </summary>
[Obsolete("Use the main AuditableInterceptor with dependency injection for better testability and configurability.")]
public sealed class LegacyAuditableInterceptor : SaveChangesInterceptor
{
    private readonly ILogger<LegacyAuditableInterceptor>? _logger;
    private readonly IAuditRecordFactory _auditRecordFactory;
    private readonly IAuditFieldProcessor _fieldProcessor;
    private readonly IAuditErrorHandler _errorHandler;
    private readonly AuditConfiguration _configuration;

    // Cache for reflection operations to improve performance
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _auditablePropertiesCache = new();

    public LegacyAuditableInterceptor(ILogger<LegacyAuditableInterceptor>? logger = null)
    {
        _logger = logger;
        _configuration = new AuditConfiguration();
        
        // Create dependencies manually for backward compatibility
        var propertyCache = new AuditPropertyCache();
        var valueConverter = new AuditValueConverter();
        _auditRecordFactory = new AuditRecordFactory();
        _fieldProcessor = new AuditFieldProcessor(propertyCache, valueConverter);
        _errorHandler = new DefaultAuditErrorHandler();
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            try
            {
                CreateAuditableRecord(eventData.Context);
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
                CreateAuditableRecord(eventData.Context);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred while creating audit records during SaveChangesAsync");
                throw;
            }
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void CreateAuditableRecord(DbContext context)
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

                _logger?.LogDebug("Created audit record for {EntityType} with ID {EntityId}, Action: {Action}",
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
}