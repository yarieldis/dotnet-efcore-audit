using System;
using Model;

namespace Data.Audit.TypeHandlers;

/// <summary>
/// Interface for handling specific types during audit value conversion.
/// </summary>
public interface IAuditTypeHandler
{
    /// <summary>
    /// Determines if this handler can handle the specified type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if this handler can handle the type, otherwise false.</returns>
    bool CanHandle(Type type);

    /// <summary>
    /// Sets the values on the audit field for the handled type.
    /// </summary>
    /// <param name="field">The audit field to set values on.</param>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    /// <param name="fieldType">The optional field type override.</param>
    void SetValues(AuditRecordField field, object? oldValue, object? newValue, string? fieldType);
}