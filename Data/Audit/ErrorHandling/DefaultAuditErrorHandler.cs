using Microsoft.Extensions.Logging;
using System;

namespace Data.Audit.ErrorHandling;

/// <summary>
/// Default implementation of IAuditErrorHandler.
/// </summary>
public class DefaultAuditErrorHandler : IAuditErrorHandler
{
    private readonly ILogger<DefaultAuditErrorHandler>? _logger;

    public DefaultAuditErrorHandler(ILogger<DefaultAuditErrorHandler>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public void HandleAuditError(System.Exception exception, Context.AuditContext context)
    {
        _logger?.LogError(exception, 
            "Error creating audit record for entity {EntityType} with ID {EntityId}",
            context.EntityType.Name, 
            context.Entity.Id);
    }

    /// <inheritdoc />
    public void HandleFieldProcessingError(System.Exception exception, string propertyName, Context.AuditContext context)
    {
        _logger?.LogWarning(exception, 
            "Error processing audit field {PropertyName} for entity {EntityType}",
            propertyName, 
            context.EntityType.Name);
    }
}