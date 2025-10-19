using Model;

namespace Data.Audit.Services;

/// <summary>
/// Implementation of IAuditRecordFactory.
/// </summary>
public class AuditRecordFactory : IAuditRecordFactory
{
    /// <inheritdoc />
    public AuditRecord CreateAuditRecord(Context.AuditContext context)
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
}