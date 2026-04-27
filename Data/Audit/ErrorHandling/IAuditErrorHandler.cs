using System;
using Data.Audit.Context;

namespace Data.Audit.ErrorHandling;

/// <summary>
/// Interface for handling audit-related errors.
/// </summary>
public interface IAuditErrorHandler
{
    /// <summary>
    /// Handles an error that occurred during audit processing.
    /// </summary>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="context">The audit context.</param>
    void HandleAuditError(Exception exception, AuditContext context);

    /// <summary>
    /// Handles an error that occurred during field processing.
    /// </summary>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="propertyName">The name of the property being processed.</param>
    /// <param name="context">The audit context.</param>
    void HandleFieldProcessingError(Exception exception, string propertyName, AuditContext context);
}