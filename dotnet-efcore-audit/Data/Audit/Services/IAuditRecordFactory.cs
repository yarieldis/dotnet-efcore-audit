using Unctad.eRegulations.Library.Model;

namespace Unctad.eRegulations.Library.Data.Audit.Services;

/// <summary>
/// Interface for creating audit records.
/// </summary>
public interface IAuditRecordFactory
{
    /// <summary>
    /// Creates an audit record for the specified context.
    /// </summary>
    /// <param name="context">The audit context.</param>
    /// <returns>A new AuditRecord instance.</returns>
    AuditRecord CreateAuditRecord(Context.AuditContext context);
}