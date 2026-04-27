using Data.Audit;
using Data.Audit.Context;
using Model;

namespace Data.Audit.Services;

/// <summary>
/// Implementation of IAuditRecordFactory.
/// </summary>
public class AuditRecordFactory : IAuditRecordFactory
{
    /// <inheritdoc />
    public AuditRecord CreateAuditRecord(AuditContext context)
    {
        return new AuditRecord
        {
            Action = (byte)context.Action,
            EntityTable = context.EntityType.Name,
            EntityTableKey = context.Entity.Id,
            AuditDate = context.Entity.ModifiedDate,
            UserName = context.Entity.ModifiedUser ?? context.Configuration.DefaultSystemUser,
        };
    }

    /// <inheritdoc />
    public AuditRecord CreateCollectionItemAuditRecord(
        IAuditable parentEntity, AuditCollectionItemContext itemContext)
    {
        return new AuditRecord
        {
            Action = (byte)itemContext.Action,
            EntityTable = parentEntity.GetType().Name,
            EntityTableKey = parentEntity.Id,
            AssociationTable = itemContext.ItemType.Name,
            AssociationTableKey = itemContext.Item.Id,
            AuditDate = parentEntity.ModifiedDate,
            UserName = parentEntity.ModifiedUser ?? itemContext.Configuration.DefaultSystemUser,
        };
    }
}