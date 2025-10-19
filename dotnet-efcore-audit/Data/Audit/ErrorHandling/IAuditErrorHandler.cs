using Microsoft.Extensions.Logging;
using System;

namespace Unctad.eRegulations.Library.Data.Audit.ErrorHandling;

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
    void HandleAuditError(System.Exception exception, Context.AuditContext context);

    /// <summary>
    /// Handles an error that occurred during field processing.
    /// </summary>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="propertyName">The name of the property being processed.</param>
    /// <param name="context">The audit context.</param>
    void HandleFieldProcessingError(System.Exception exception, string propertyName, Context.AuditContext context);
}