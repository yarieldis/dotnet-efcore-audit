using System;
using Microsoft.Extensions.Logging;
using Unctad.eRegulations.Library.Data.Audit.Context;

namespace Data.Audit.ErrorHandling;

/// <summary>
/// Default implementation of IAuditErrorHandler.
/// </summary>
public class DefaultAuditErrorHandler(ILogger<DefaultAuditErrorHandler>? logger = null) : IAuditErrorHandler
{
    /// <inheritdoc />
    public void HandleAuditError(Exception exception, AuditContext context)
    {
        logger?.LogError(exception,
            "Error creating audit record for entity {EntityType} with ID {EntityId}",
            context.EntityType.Name,
            context.Entity.Id);
    }

    /// <inheritdoc />
    public void HandleFieldProcessingError(Exception exception, string propertyName, AuditContext context)
    {
        logger?.LogWarning(exception,
            "Error processing audit field {PropertyName} for entity {EntityType}",
            propertyName,
            context.EntityType.Name);
    }
}