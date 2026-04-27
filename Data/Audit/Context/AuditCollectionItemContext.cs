using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Data.Audit.Configuration;

namespace Data.Audit.Context;

/// <summary>
/// Provides context for auditing field-level changes on an IAuditableCollection item.
/// AuditDate/UserName are sourced from the parent IAuditable entity, not the item itself.
/// </summary>
public class AuditCollectionItemContext(
    DbContext dbContext,
    EntityEntry itemEntry,
    IAuditableCollection item,
    AuditAction action,
    AuditConfiguration configuration)
{
    public DbContext DbContext { get; } = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    /// <summary>Non-generic entity entry for reading OriginalValues/CurrentValues.</summary>
    public EntityEntry ItemEntry { get; } = itemEntry ?? throw new ArgumentNullException(nameof(itemEntry));

    public IAuditableCollection Item { get; } = item ?? throw new ArgumentNullException(nameof(item));

    public AuditAction Action { get; } = action;

    public AuditConfiguration Configuration { get; } = configuration ?? throw new ArgumentNullException(nameof(configuration));

    public Type ItemType => Item.GetType();
}
