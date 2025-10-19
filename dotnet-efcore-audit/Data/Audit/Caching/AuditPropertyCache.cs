using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Unctad.eRegulations.Library.Data.Audit.Caching;

/// <summary>
/// Implementation of IAuditPropertyCache that provides caching for audit property information.
/// </summary>
public class AuditPropertyCache : IAuditPropertyCache
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertiesCache = new();
    private static readonly ConcurrentDictionary<PropertyInfo, AuditFieldAttribute?> _attributeCache = new();

    /// <inheritdoc />
    public PropertyInfo[] GetAuditableProperties(Type entityType)
    {
        return _propertiesCache.GetOrAdd(entityType, type =>
            type.GetProperties()
                .Where(p => p.GetCustomAttributes(typeof(AuditFieldAttribute), false).Any())
                .ToArray());
    }

    /// <inheritdoc />
    public AuditFieldAttribute? GetAuditFieldAttribute(PropertyInfo property)
    {
        return _attributeCache.GetOrAdd(property, prop =>
            prop.GetCustomAttributes(typeof(AuditFieldAttribute), false)
                .FirstOrDefault() as AuditFieldAttribute);
    }
}