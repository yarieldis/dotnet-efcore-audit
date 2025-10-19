using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using Unctad.eRegulations.Library.Model;

namespace Unctad.eRegulations.Library.Data.Audit.Context;

/// <summary>
/// Provides context information for audit operations.
/// </summary>
public class AuditContext(
    Microsoft.EntityFrameworkCore.DbContext dbContext,
    EntityEntry<IAuditable> entityEntry,
    AuditAction action,
    Configuration.AuditConfiguration configuration)
{

    /// <summary>
    /// Gets the Entity Framework DbContext.
    /// </summary>
    public Microsoft.EntityFrameworkCore.DbContext DbContext { get; } = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    /// <summary>
    /// Gets the entity entry being audited.
    /// </summary>
    public EntityEntry<IAuditable> EntityEntry { get; } = entityEntry ?? throw new ArgumentNullException(nameof(entityEntry));

    /// <summary>
    /// Gets the audit action being performed.
    /// </summary>
    public AuditAction Action { get; } = action;

    /// <summary>
    /// Gets the audit configuration.
    /// </summary>
    public Configuration.AuditConfiguration Configuration { get; } = configuration ?? throw new ArgumentNullException(nameof(configuration));

    /// <summary>
    /// Gets the entity being audited.
    /// </summary>
    public IAuditable Entity => EntityEntry.Entity;

    /// <summary>
    /// Gets the type of the entity being audited.
    /// </summary>
    public Type EntityType => EntityEntry.Entity.GetType();
}