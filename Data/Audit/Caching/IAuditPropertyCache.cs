using System;
using System.Reflection;

namespace Data.Audit.Caching;

/// <summary>
/// Interface for caching audit-related property information.
/// </summary>
public interface IAuditPropertyCache
{
    /// <summary>
    /// Gets the auditable properties for the specified entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>An array of PropertyInfo objects that have the AuditFieldAttribute.</returns>
    PropertyInfo[] GetAuditableProperties(Type entityType);

    /// <summary>
    /// Gets the AuditFieldAttribute for the specified property.
    /// </summary>
    /// <param name="property">The property to get the attribute for.</param>
    /// <returns>The AuditFieldAttribute if present, otherwise null.</returns>
    AuditFieldAttribute? GetAuditFieldAttribute(PropertyInfo property);
}