using System;
using System.Collections.Generic;
using Unctad.eRegulations.Library.Model;

namespace Unctad.eRegulations.Library.Data.Audit.Services;

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
    IEnumerable<AuditRecordField> ProcessFields(Context.AuditContext context, AuditRecord auditRecord);
}