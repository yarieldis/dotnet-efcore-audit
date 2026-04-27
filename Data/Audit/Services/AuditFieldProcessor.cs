using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.Logging;

using Data.Audit.Caching;
using Data.Audit.Context;
using Data.Audit.TypeHandlers;
using Model;

namespace Data.Audit.Services;

/// <summary>
/// Implementation of IAuditFieldProcessor.
/// </summary>
public class AuditFieldProcessor(
    IAuditPropertyCache propertyCache,
    IAuditValueConverter valueConverter,
    ILogger<AuditFieldProcessor>? logger = null) : IAuditFieldProcessor
{
    private readonly IAuditPropertyCache _propertyCache = propertyCache ?? throw new ArgumentNullException(nameof(propertyCache));
    private readonly IAuditValueConverter _valueConverter = valueConverter ?? throw new ArgumentNullException(nameof(valueConverter));

    /// <inheritdoc />
    public IEnumerable<AuditRecordField> ProcessFields(AuditContext context, AuditRecord auditRecord)
    {
        if (!context.Configuration.EnableFieldLevelAuditing)
        {
            yield break;
        }

        var auditableProperties = _propertyCache.GetAuditableProperties(context.EntityType);

        foreach (var property in auditableProperties.OrderBy(p => _propertyCache.GetAuditFieldAttribute(p)?.Order ?? 0))
        {
            AuditRecordField? auditField = null;
            
            try
            {
                var attribute = _propertyCache.GetAuditFieldAttribute(property);
                
                // Skip if explicitly marked as not auditable
                if (attribute?.IsAuditable == false)
                    continue;

                var oldValue = context.EntityEntry.OriginalValues[property.Name];
                var newValue = context.EntityEntry.CurrentValues[property.Name];

                if (!AreValuesEqual(oldValue, newValue))
                {
                    auditField = CreateAuditField(context, auditRecord, property, attribute, oldValue, newValue);
                    
                    if (context.Configuration.EnableDetailedLogging)
                    {
                        logger?.LogDebug("Added audit field for property {PropertyName} on {EntityType}",
                            property.Name, context.EntityType.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Error processing audit field {PropertyName} for entity {EntityType}",
                    property.Name, context.EntityType.Name);
                
                if (!context.Configuration.ContinueOnFieldProcessingError)
                {
                    throw;
                }
            }

            if (auditField != null)
            {
                yield return auditField;
            }
        }
    }

    private AuditRecordField CreateAuditField(
        AuditContext context,
        AuditRecord auditRecord,
        PropertyInfo property,
        AuditFieldAttribute? attribute,
        object? oldValue,
        object? newValue)
    {
        var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        var displayName = attribute?.DisplayName ?? property.Name;
        var fieldType = attribute?.FieldType;
        var isSensitive = attribute?.IsSensitive ?? false;

        var auditField = new AuditRecordField
        {
            AuditRecordId = auditRecord.Id,
            AuditRecord = auditRecord,
            FieldName = displayName,
        };

        // Handle sensitive data masking
        if (isSensitive && context.Configuration.EnableSensitiveDataMasking)
        {
            oldValue = oldValue != null ? context.Configuration.DefaultMaskValue : null;
            newValue = newValue != null ? context.Configuration.DefaultMaskValue : null;
        }

        _valueConverter.SetAuditFieldValues(auditField, oldValue, newValue, propertyType, fieldType);
        return auditField;
    }

    /// <inheritdoc />
    public IEnumerable<AuditRecordField> ProcessCollectionItemFields(
        AuditCollectionItemContext context, AuditRecord auditRecord)
    {
        if (!context.Configuration.EnableFieldLevelAuditing)
            yield break;

        var auditableProperties = _propertyCache.GetAuditableProperties(context.ItemType);

        foreach (var property in auditableProperties.OrderBy(p =>
            _propertyCache.GetAuditFieldAttribute(p)?.Order ?? 0))
        {
            AuditRecordField? auditField = null;

            try
            {
                var attribute = _propertyCache.GetAuditFieldAttribute(property);

                if (attribute?.IsAuditable == false)
                    continue;

                var oldValue = context.ItemEntry.OriginalValues[property.Name];
                var newValue = context.ItemEntry.CurrentValues[property.Name];

                if (!AreValuesEqual(oldValue, newValue))
                {
                    auditField = CreateCollectionItemAuditField(
                        context, auditRecord, property, attribute, oldValue, newValue);

                    if (context.Configuration.EnableDetailedLogging)
                        logger?.LogDebug(
                            "Added collection audit field for {PropertyName} on {ItemType}",
                            property.Name, context.ItemType.Name);
                }
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex,
                    "Error processing collection audit field {PropertyName} for {ItemType}",
                    property.Name, context.ItemType.Name);

                if (!context.Configuration.ContinueOnFieldProcessingError)
                    throw;
            }

            if (auditField != null)
                yield return auditField;
        }
    }

    private AuditRecordField CreateCollectionItemAuditField(
        AuditCollectionItemContext context,
        AuditRecord auditRecord,
        PropertyInfo property,
        AuditFieldAttribute? attribute,
        object? oldValue,
        object? newValue)
    {
        var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        var displayName = attribute?.DisplayName ?? property.Name;
        var fieldType = attribute?.FieldType;
        var isSensitive = attribute?.IsSensitive ?? false;

        var auditField = new AuditRecordField
        {
            AuditRecordId = auditRecord.Id,
            AuditRecord = auditRecord,
            FieldName = displayName,
        };

        if (isSensitive && context.Configuration.EnableSensitiveDataMasking)
        {
            oldValue = oldValue != null ? context.Configuration.DefaultMaskValue : null;
            newValue = newValue != null ? context.Configuration.DefaultMaskValue : null;
        }

        _valueConverter.SetAuditFieldValues(auditField, oldValue, newValue, propertyType, fieldType);
        return auditField;
    }

    private static bool AreValuesEqual(object? oldValue, object? newValue)
    {
        return oldValue switch
        {
            null when newValue is null => true,
            null => false,
            _ when newValue is null => false,
            _ => oldValue.Equals(newValue)
        };
    }
}