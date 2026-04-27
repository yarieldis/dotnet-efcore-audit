# Audit System Architecture

This document describes the audit system that provides a modular, testable, and configurable approach to auditing entity changes in Entity Framework Core.

## Overview

The audit system has been implemented as a modular architecture with clear separation of concerns, dependency injection support, and enhanced configurability.

## What's New

**Collection Auditing Support**: The audit system now supports tracking changes to child collection items. Child entities implementing `IAuditableCollection` can be audited alongside their parent `IAuditable` entities. Use the `AuditCollectionFieldAttribute` to mark collection navigation properties for auditing. Audit metadata (date/user) is automatically sourced from the parent entity.

## Architecture Components

### 1. Core Interfaces and Services

#### `IAuditRecordFactory`
- **Purpose**: Creates audit records for entity changes
- **Implementation**: `AuditRecordFactory`
- **Responsibility**: Transform audit context into `AuditRecord` entities

#### `IAuditFieldProcessor`
- **Purpose**: Processes field-level changes for auditing
- **Implementation**: `AuditFieldProcessor`
- **Responsibility**: Extract and process individual field changes

#### `IAuditValueConverter`
- **Purpose**: Converts field values to appropriate audit record field types
- **Implementation**: `AuditValueConverter`
- **Responsibility**: Handle type-specific value conversion using strategy pattern

#### `IAuditPropertyCache`
- **Purpose**: Caches reflection-based property information
- **Implementation**: `AuditPropertyCache`
- **Responsibility**: Improve performance by caching auditable properties and attributes

#### `IAuditErrorHandler`
- **Purpose**: Handles errors during audit processing
- **Implementation**: `DefaultAuditErrorHandler`
- **Responsibility**: Centralized error handling and logging

### 2. Configuration

#### `AuditConfiguration`
Centralized configuration for audit behavior:

```csharp
public class AuditConfiguration
{
    public bool EnableSensitiveDataMasking { get; set; } = true;
    public bool EnableFieldLevelAuditing { get; set; } = true;
    public bool ContinueOnFieldProcessingError { get; set; } = true;
    public string DefaultMaskValue { get; set; } = "***MASKED***";
    public string DefaultSystemUser { get; set; } = "System";
    public bool EnableDetailedLogging { get; set; } = false;
}
```

### 3. Type Handling Strategy

#### `IAuditTypeHandler`
Interface for handling specific data types during audit value conversion:

- `StringTypeHandler` - Handles string values
- `IntegerTypeHandler` - Handles int, short, long values
- `DecimalTypeHandler` - Handles decimal, double, float values
- `DateTimeTypeHandler` - Handles DateTime, DateTimeOffset, DateOnly values
- `BooleanTypeHandler` - Handles boolean values
- `EnumTypeHandler` - Handles enum values
- `GuidTypeHandler` - Handles Guid values
- `FallbackTypeHandler` - Handles any other type as text

### 4. Context and Error Handling

#### `AuditContext`
Encapsulates all information needed for audit processing:
- DbContext
- EntityEntry
- AuditAction
- Configuration

#### `AuditCollectionItemContext`
Provides context for auditing field-level changes on IAuditableCollection items:
- ItemEntry and collection item reference
- Audit metadata sourced from parent entity
- Action type (Insert, Update, Delete)
- Configuration

#### Error Handling
- Centralized error handling through `IAuditErrorHandler`
- Configurable error continuation behavior
- Structured logging for troubleshooting

### 5. Collection Auditing Support

#### `IAuditableCollection`
Marker interface for child collection items owned by an IAuditable parent:
- Items do not require ModifiedDate/ModifiedUser properties
- Audit metadata is inherited from the parent entity
- Tracked as association records under the parent

#### `AuditCollectionFieldAttribute`
Marks collection navigation properties that should be audited:
- Applied to properties in the parent's [MetadataType] buddy class
- Items implementing IAuditableCollection are captured as association audit records
- Supports audit tracking for Add/Remove/Modify operations on collection items

## Usage

### Basic Setup with Dependency Injection

```csharp
// In Program.cs or Startup.cs
services.AddAuditServices();

// Or with custom configuration
services.AddAuditServices(config =>
{
    config.EnableSensitiveDataMasking = true;
    config.EnableDetailedLogging = true;
    config.DefaultMaskValue = "[REDACTED]";
});

// Register with DbContext
services.AddDbContext<YourDbContext>((serviceProvider, options) =>
{
    options.UseSqlServer(connectionString);
    options.AddInterceptors(serviceProvider.GetRequiredService<AuditableInterceptor>());
});
```

### Legacy Usage (Backward Compatibility)

For existing code that doesn't use dependency injection:

```csharp
// Use the legacy interceptor
var interceptor = new LegacyAuditableInterceptor(logger);

// Add to DbContext options
options.AddInterceptors(interceptor);
```

### Custom Type Handlers

You can add custom type handlers for specific data types:

```csharp
public class CustomTypeHandler : IAuditTypeHandler
{
    public bool CanHandle(Type type) => type == typeof(MyCustomType);
    
    public void SetValues(AuditRecordField field, object? oldValue, object? newValue, string? fieldType)
    {
        // Custom conversion logic
        field.FieldType = "CustomType";
        field.OldValue_asText = ConvertToCustomFormat(oldValue);
        field.NewValue_asText = ConvertToCustomFormat(newValue);
    }
}

// Register custom handlers
services.AddCustomAuditTypeHandlers(new CustomTypeHandler());
```

### Collection Auditing

Audit child collection items alongside their parent entities:

```csharp
// Define a collection item that implements IAuditableCollection
public class OrderItem : IAuditableCollection
{
    public int Id { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
}

// Define parent entity
public class Order : IAuditable
{
    public int Id { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string ModifiedUser { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}

// Create metadata type with AuditCollectionFieldAttribute
[MetadataType(typeof(OrderMetadata))]
public partial class Order { }

public class OrderMetadata
{
    [AuditCollectionField]
    public IEnumerable<OrderItem> Items { get; set; }
}
```

When an Order is modified and its Items collection changes:
- Added items create audit records with AuditAction = Insert
- Deleted items create audit records with AuditAction = Delete  
- Modified items create field-level change records
- Audit metadata (date/user) is sourced from the parent Order

## Benefits of the New Architecture

### 1. **Separation of Concerns**
- Each component has a single, well-defined responsibility
- Easier to understand, test, and maintain
- Clear interfaces define contracts between components

### 2. **Testability**
- All dependencies are injected through constructors
- Easy to mock components for unit testing
- Each component can be tested in isolation

### 3. **Configurability**
- Centralized configuration through `AuditConfiguration`
- Runtime configuration changes possible
- Environment-specific settings support

### 4. **Extensibility**
- Easy to add new type handlers without modifying existing code
- Strategy pattern allows for custom value conversion logic
- Plugin-like architecture for new audit features

### 5. **Performance**
- Improved caching through dedicated cache service
- Type handlers can be optimized independently
- Reflection operations are cached for better performance

### 6. **Error Handling**
- Centralized error handling strategy
- Configurable error continuation behavior
- Better logging and monitoring capabilities

### 7. **Dependency Injection Integration**
- Full support for .NET dependency injection
- Easy integration with existing DI containers
- Proper service lifetime management

## Migration Guide

### From Original AuditableInterceptor

1. **Install new dependencies** in your DI container:
   ```csharp
   services.AddAuditServices();
   ```

2. **Update DbContext registration**:
   ```csharp
   services.AddDbContext<YourDbContext>((serviceProvider, options) =>
   {
       options.AddInterceptors(serviceProvider.GetRequiredService<AuditableInterceptor>());
   });
   ```

3. **Optional: Customize configuration**:
   ```csharp
   services.AddAuditServices(config =>
   {
       config.EnableSensitiveDataMasking = false;
       config.EnableDetailedLogging = true;
   });
   ```

### Minimal Migration (Legacy Support)

If you can't update to use dependency injection immediately:

```csharp
// Replace old interceptor with legacy version
var interceptor = new LegacyAuditableInterceptor(logger);
options.AddInterceptors(interceptor);
```

## Testing

The modular architecture makes testing much easier:

```csharp
[Test]
public void Should_Create_Audit_Record_For_Entity_Change()
{
    // Arrange
    var mockPropertyCache = new Mock<IAuditPropertyCache>();
    var mockValueConverter = new Mock<IAuditValueConverter>();
    var processor = new AuditFieldProcessor(mockPropertyCache.Object, mockValueConverter.Object);
    
    // Act & Assert
    // Test specific functionality in isolation
}
```

## Performance Considerations

- **Caching**: Property and attribute information is cached to avoid repeated reflection
- **Lazy Loading**: Type handlers are resolved only when needed
- **Minimal Allocation**: Reuse of objects where possible
- **Async Support**: Full async/await support throughout the pipeline

## Logging

The system provides structured logging at various levels:
- **Debug**: Detailed operation information
- **Information**: Normal operation milestones
- **Warning**: Non-critical errors that don't stop processing
- **Error**: Critical errors that require attention

Enable detailed logging through configuration:
```csharp
config.EnableDetailedLogging = true;
```

## Future Enhancements

The modular architecture enables future enhancements such as:
- Middleware pipeline for audit processing
- Custom validation rules for audit data
- Asynchronous audit processing
- Audit data encryption/decryption
- Integration with external audit systems
- Performance metrics and monitoring

## Project Structure

```
Data/Audit/
├── IAuditable.cs                      # Core interface for auditable entities
├── IAuditableCollection.cs            # Interface for collection items
├── AuditAction.cs                     # Enum: Insert, Update, Delete, etc.
├── AuditFieldAttribute.cs             # Attribute for field-level audit config
├── AuditCollectionFieldAttribute.cs   # Attribute for collection auditing
├── AuditableInterceptor.cs            # Main EF Core interceptor
├── Caching/
│   ├── IAuditPropertyCache.cs         # Reflection cache interface
│   └── AuditPropertyCache.cs
├── Configuration/
│   └── AuditConfiguration.cs          # Centralized audit settings
├── Context/
│   ├── AuditContext.cs                # Audit processing context
│   └── AuditCollectionItemContext.cs  # Collection item context
├── ErrorHandling/
│   ├── IAuditErrorHandler.cs          # Error handling interface
│   └── DefaultAuditErrorHandler.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs # DI registration
├── Legacy/
│   └── LegacyAuditableInterceptor.cs  # Non-DI backward-compatible
├── Services/
│   ├── IAuditRecordFactory.cs         # Audit record creation
│   ├── AuditRecordFactory.cs
│   ├── IAuditFieldProcessor.cs        # Field processing
│   └── AuditFieldProcessor.cs
└── TypeHandlers/
    ├── IAuditTypeHandler.cs           # Type conversion strategy
    ├── IAuditValueConverter.cs        # Value converter
    ├── AuditValueConverter.cs
    └── AuditTypeHandlers.cs           # Built-in type handlers
```