using System;

namespace Data.Audit;

/// <summary>
/// Attribute to mark fields/properties that should be tracked for auditing purposes.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class AuditFieldAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the display name for the field in audit records.
    /// If not specified, the property name will be used.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets whether this field should be included in audit tracking.
    /// Default is true.
    /// </summary>
    public bool IsAuditable { get; set; } = true;

    /// <summary>
    /// Gets or sets the field type for audit record storage.
    /// If not specified, it will be inferred from the property type.
    /// </summary>
    public string? FieldType { get; set; }

    /// <summary>
    /// Gets or sets whether sensitive data should be masked in audit logs.
    /// Default is false.
    /// </summary>
    public bool IsSensitive { get; set; } = false;

    /// <summary>
    /// Gets or sets the order in which fields should be processed during auditing.
    /// Default is 0.
    /// </summary>
    public int Order { get; set; } = 0;

    /// <summary>
    /// Initializes a new instance of the AuditFieldAttribute class.
    /// </summary>
    public AuditFieldAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the AuditFieldAttribute class with a display name.
    /// </summary>
    /// <param name="displayName">The display name for the field in audit records.</param>
    public AuditFieldAttribute(string displayName)
    {
        DisplayName = displayName;
    }
}