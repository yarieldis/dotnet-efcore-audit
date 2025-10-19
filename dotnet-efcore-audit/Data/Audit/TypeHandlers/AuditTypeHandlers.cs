using System;
using Model;

namespace Data.Audit.TypeHandlers;

/// <summary>
/// Type handler for string values.
/// </summary>
public class StringTypeHandler : IAuditTypeHandler
{
    /// <inheritdoc />
    public bool CanHandle(Type type) => type == typeof(string);

    /// <inheritdoc />
    public void SetValues(AuditRecordField field, object? oldValue, object? newValue, string? fieldType)
    {
        field.FieldType = fieldType ?? "Text";
        field.OldValue_asText = oldValue?.ToString();
        field.NewValue_asText = newValue?.ToString();
    }
}

/// <summary>
/// Type handler for integer values (int, short, long).
/// </summary>
public class IntegerTypeHandler : IAuditTypeHandler
{
    /// <inheritdoc />
    public bool CanHandle(Type type) => 
        type == typeof(int) || type == typeof(short) || type == typeof(long);

    /// <inheritdoc />
    public void SetValues(AuditRecordField field, object? oldValue, object? newValue, string? fieldType)
    {
        field.FieldType = fieldType ?? "Integer";
        field.OldValue_asInteger = oldValue != null ? Convert.ToInt32(oldValue) : null;
        field.NewValue_asInteger = newValue != null ? Convert.ToInt32(newValue) : null;
    }
}

/// <summary>
/// Type handler for decimal values (decimal, double, float).
/// </summary>
public class DecimalTypeHandler : IAuditTypeHandler
{
    /// <inheritdoc />
    public bool CanHandle(Type type) => 
        type == typeof(decimal) || type == typeof(double) || type == typeof(float);

    /// <inheritdoc />
    public void SetValues(AuditRecordField field, object? oldValue, object? newValue, string? fieldType)
    {
        field.FieldType = fieldType ?? "Decimal";
        field.OldValue_asDecimal = oldValue != null ? Convert.ToDecimal(oldValue) : null;
        field.NewValue_asDecimal = newValue != null ? Convert.ToDecimal(newValue) : null;
    }
}

/// <summary>
/// Type handler for DateTime values (DateTime, DateTimeOffset, DateOnly).
/// </summary>
public class DateTimeTypeHandler : IAuditTypeHandler
{
    /// <inheritdoc />
    public bool CanHandle(Type type) => 
        type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(DateOnly);

    /// <inheritdoc />
    public void SetValues(AuditRecordField field, object? oldValue, object? newValue, string? fieldType)
    {
        field.FieldType = fieldType ?? "DateTime";
        field.OldValue_asDateTime = oldValue != null ? Convert.ToDateTime(oldValue) : null;
        field.NewValue_asDateTime = newValue != null ? Convert.ToDateTime(newValue) : null;
    }
}

/// <summary>
/// Type handler for boolean values.
/// </summary>
public class BooleanTypeHandler : IAuditTypeHandler
{
    /// <inheritdoc />
    public bool CanHandle(Type type) => type == typeof(bool);

    /// <inheritdoc />
    public void SetValues(AuditRecordField field, object? oldValue, object? newValue, string? fieldType)
    {
        field.FieldType = fieldType ?? "Boolean";
        field.OldValue_asBoolean = (bool?)oldValue;
        field.NewValue_asBoolean = (bool?)newValue;
    }
}

/// <summary>
/// Type handler for enum values.
/// </summary>
public class EnumTypeHandler : IAuditTypeHandler
{
    /// <inheritdoc />
    public bool CanHandle(Type type) => type.IsEnum;

    /// <inheritdoc />
    public void SetValues(AuditRecordField field, object? oldValue, object? newValue, string? fieldType)
    {
        field.FieldType = fieldType ?? "Enum";
        field.OldValue_asText = oldValue?.ToString();
        field.NewValue_asText = newValue?.ToString();
    }
}

/// <summary>
/// Type handler for Guid values.
/// </summary>
public class GuidTypeHandler : IAuditTypeHandler
{
    /// <inheritdoc />
    public bool CanHandle(Type type) => type == typeof(Guid);

    /// <inheritdoc />
    public void SetValues(AuditRecordField field, object? oldValue, object? newValue, string? fieldType)
    {
        field.FieldType = fieldType ?? "Guid";
        field.OldValue_asText = oldValue?.ToString();
        field.NewValue_asText = newValue?.ToString();
    }
}

/// <summary>
/// Fallback type handler for complex types.
/// </summary>
public class FallbackTypeHandler : IAuditTypeHandler
{
    /// <inheritdoc />
    public bool CanHandle(Type type) => true; // Can handle any type as fallback

    /// <inheritdoc />
    public void SetValues(AuditRecordField field, object? oldValue, object? newValue, string? fieldType)
    {
        field.FieldType = fieldType ?? oldValue?.GetType().Name ?? newValue?.GetType().Name ?? "Unknown";
        field.OldValue_asText = oldValue?.ToString();
        field.NewValue_asText = newValue?.ToString();
    }
}