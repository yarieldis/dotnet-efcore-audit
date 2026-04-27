using System.Collections.Generic;
using Data.Audit.Context;
using Model;

namespace Data.Audit.Services;

/// <summary>
/// Interface for processing audit fields.
/// </summary>
public interface IAuditFieldProcessor
{
    /// <summary>
    /// Processes the auditable fields for the specified context and audit record.
    /// </summary>
    /// <param name="context">The audit context.</param>
    /// <param name="auditRecord">The audit record.</param>
    /// <returns>A collection of AuditRecordField instances.</returns>
    IEnumerable<AuditRecordField> ProcessFields(AuditContext context, AuditRecord auditRecord);

    /// <summary>
    /// Processes the auditable fields for a collection item under its parent's audit record.
    /// Only called for Modified state items; Added/Deleted items produce no field rows.
    /// </summary>
    /// <param name="context">The collection item audit context.</param>
    /// <param name="auditRecord">The audit record created for the collection item.</param>
    /// <returns>A collection of AuditRecordField instances.</returns>
    IEnumerable<AuditRecordField> ProcessCollectionItemFields(
        AuditCollectionItemContext context, AuditRecord auditRecord);
}