using System;
using Model;

namespace Data.Audit.TypeHandlers;

/// <summary>
/// Interface for converting audit values to the appropriate AuditRecordField properties.
/// </summary>
public interface IAuditValueConverter
{
    /// <summary>
    /// Sets the appropriate value properties on the audit field based on the property type.
    /// </summary>
    /// <param name="auditField">The audit field to set values on.</param>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    /// <param name="propertyType">The type of the property.</param>
    /// <param name="fieldType">The optional field type override.</param>
    void SetAuditFieldValues(AuditRecordField auditField, object? oldValue, object? newValue, Type propertyType, string? fieldType);
}