using System;
using System.Collections.Generic;
using System.Linq;
using Model;

namespace Data.Audit.TypeHandlers;

/// <summary>
/// Implementation of IAuditValueConverter that uses a collection of type handlers.
/// </summary>
public class AuditValueConverter : IAuditValueConverter
{
    private readonly IReadOnlyList<IAuditTypeHandler> _typeHandlers;

    public AuditValueConverter()
    {
        _typeHandlers = GetDefaultTypeHandlers();
    }

    public AuditValueConverter(IEnumerable<IAuditTypeHandler> typeHandlers)
    {
        if (typeHandlers == null)
            throw new ArgumentNullException(nameof(typeHandlers));

        var handlersList = typeHandlers.ToList();
        if (handlersList.Count == 0)
        {
            // If empty collection provided, use default handlers
            _typeHandlers = GetDefaultTypeHandlers();
        }
        else
        {
            _typeHandlers = handlersList;
        }
    }

    private static IReadOnlyList<IAuditTypeHandler> GetDefaultTypeHandlers()
    {
        return
        [
            new StringTypeHandler(),
            new IntegerTypeHandler(),
            new DecimalTypeHandler(),
            new DateTimeTypeHandler(),
            new BooleanTypeHandler(),
            new EnumTypeHandler(),
            new GuidTypeHandler(),
            new FallbackTypeHandler() // Must be last as it handles any type
        ];
    }

    /// <inheritdoc />
    public void SetAuditFieldValues(AuditRecordField auditField, object? oldValue, object? newValue, Type propertyType, string? fieldType)
    {
        var handler = _typeHandlers.FirstOrDefault(h => h.CanHandle(propertyType));

        if (handler == null)
        {
            throw new InvalidOperationException($"No type handler found for type {propertyType.Name}");
        }

        handler.SetValues(auditField, oldValue, newValue, fieldType);
    }
}