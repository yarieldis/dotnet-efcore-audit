using System;

namespace Data.Audit;

/// <summary>
/// Marks a collection navigation property on an IAuditable parent entity whose items
/// (implementing IAuditableCollection) should be captured as association audit records
/// under the parent. Place this attribute in the parent's [MetadataType] buddy metadata class.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class AuditCollectionFieldAttribute : Attribute
{
}
