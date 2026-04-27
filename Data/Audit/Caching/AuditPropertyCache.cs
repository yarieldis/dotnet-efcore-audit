using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Data.Audit.Caching;

/// <summary>
/// Implementation of IAuditPropertyCache that provides caching for audit property information.
/// Supports [AuditField] declared directly on properties or via a [MetadataType] buddy class.
/// </summary>
public class AuditPropertyCache : IAuditPropertyCache
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertiesCache = new();
    private static readonly ConcurrentDictionary<PropertyInfo, AuditFieldAttribute?> _attributeCache = new();
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _collectionPropertiesCache = new();

    /// <inheritdoc />
    public PropertyInfo[] GetAuditableProperties(Type entityType)
    {
        return _propertiesCache.GetOrAdd(entityType, type =>
        {
            var buddyType = type.GetCustomAttribute<MetadataTypeAttribute>()?.MetadataClassType;

            return [.. type.GetProperties().Where(p =>
                p.GetCustomAttributes(typeof(AuditFieldAttribute), false).Length != 0
                || buddyType?.GetProperty(p.Name)
                    ?.GetCustomAttributes(typeof(AuditFieldAttribute), false).Length > 0)];
        });
    }

    /// <inheritdoc />
    public AuditFieldAttribute? GetAuditFieldAttribute(PropertyInfo property)
    {
        return _attributeCache.GetOrAdd(property, prop =>
        {
            var direct = prop.GetCustomAttributes(typeof(AuditFieldAttribute), false)
                .FirstOrDefault() as AuditFieldAttribute;
            if (direct != null)
                return direct;

            var buddyType = prop.DeclaringType?.GetCustomAttribute<MetadataTypeAttribute>()?.MetadataClassType;
            return buddyType?.GetProperty(prop.Name)
                ?.GetCustomAttributes(typeof(AuditFieldAttribute), false)
                .FirstOrDefault() as AuditFieldAttribute;
        });
    }

    /// <inheritdoc />
    public PropertyInfo[] GetAuditableCollectionProperties(Type entityType)
    {
        return _collectionPropertiesCache.GetOrAdd(entityType, type =>
        {
            var buddyType = type.GetCustomAttribute<MetadataTypeAttribute>()?.MetadataClassType;

            return [.. type.GetProperties().Where(p =>
                p.GetCustomAttributes(typeof(AuditCollectionFieldAttribute), false).Length != 0
                || buddyType?.GetProperty(p.Name)
                    ?.GetCustomAttributes(typeof(AuditCollectionFieldAttribute), false).Length > 0)];
        });
    }
}