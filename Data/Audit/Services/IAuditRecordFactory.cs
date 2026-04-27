using Data.Audit;
using Data.Audit.Context;
using Model;

namespace Data.Audit.Services;

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
    AuditRecord CreateAuditRecord(AuditContext context);

    /// <summary>
    /// Creates an audit record for a collection item, associating it with the parent entity.
    /// EntityTable/EntityTableKey reference the parent; AssociationTable/AssociationTableKey
    /// reference the collection item. AuditDate/UserName are sourced from the parent entity.
    /// </summary>
    /// <param name="parentEntity">The IAuditable parent entity.</param>
    /// <param name="itemContext">The collection item context.</param>
    /// <returns>A new AuditRecord instance.</returns>
    AuditRecord CreateCollectionItemAuditRecord(IAuditable parentEntity, AuditCollectionItemContext itemContext);
}