# CLAUDE.md - dotnet-efcore-audit

This file provides guidance to Claude Code when working with the **dotnet-efcore-audit** repository.

## Repository Overview

**Purpose**: A .NET class library that provides automatic entity change auditing for Entity Framework Core via `SaveChanges` interceptors. It tracks Insert, Update, Delete, and other actions on entities implementing `IAuditable`, recording field-level changes with type-aware value conversion.

**Status**: Active development

**Technology Stack**:
- .NET 8.0
- Entity Framework Core 8.0.6 (SqlServer provider)
- C# with nullable reference types enabled

## Project Structure

```
dotnet-efcore-audit/
├── dotnet-efcore-audit.csproj      # Project file (class library, net8.0)
├── dotnet-efcore-audit.sln         # Solution file
├── README.md                       # Architecture documentation
└── Data/
    └── Audit/
        ├── IAuditable.cs               # Core interface entities must implement
        ├── AuditAction.cs              # Enum: Init, Insert, Update, Delete, Translate, MovedToRecycleBin
        ├── AuditFieldAttribute.cs      # Attribute for field-level audit configuration
        ├── AuditableInterceptor.cs     # Main EF Core SaveChanges interceptor (DI-based)
        ├── Caching/
        │   ├── IAuditPropertyCache.cs  # Interface for reflection cache
        │   └── AuditPropertyCache.cs   # Caches auditable properties and attributes
        ├── Configuration/
        │   └── AuditConfiguration.cs   # Centralized audit settings (masking, logging, etc.)
        ├── Context/
        │   └── AuditContext.cs         # Encapsulates audit processing context
        ├── ErrorHandling/
        │   ├── IAuditErrorHandler.cs   # Error handling interface
        │   └── DefaultAuditErrorHandler.cs
        ├── Extensions/
        │   └── ServiceCollectionExtensions.cs  # DI registration (AddAuditServices)
        ├── Legacy/
        │   └── LegacyAuditableInterceptor.cs   # Non-DI backward-compatible interceptor
        ├── Services/
        │   ├── IAuditRecordFactory.cs  # Creates audit records
        │   ├── AuditRecordFactory.cs
        │   ├── IAuditFieldProcessor.cs # Processes field-level changes
        │   └── AuditFieldProcessor.cs
        └── TypeHandlers/
            ├── IAuditTypeHandler.cs    # Strategy interface for type conversion
            ├── IAuditValueConverter.cs  # Value converter interface
            ├── AuditValueConverter.cs   # Orchestrates type handlers
            └── AuditTypeHandlers.cs     # Built-in handlers (string, int, decimal, DateTime, bool, enum, Guid, fallback)
```

## Quick Start

### Build
```bash
dotnet build

dotnet build --configuration Release
```

### Tests
```bash
# No test project yet — tests should be added
```

## Architecture Patterns

### Interceptor Pattern
The core of the library is `AuditableInterceptor`, an EF Core `SaveChangesInterceptor` that hooks into `SavingChanges`/`SavingChangesAsync` to capture entity changes before they are persisted.

### Strategy Pattern (Type Handlers)
`IAuditTypeHandler` implementations handle type-specific value conversion. Each handler declares which types it supports via `CanHandle(Type)`. Built-in handlers cover: string, int/short/long, decimal/double/float, DateTime/DateTimeOffset/DateOnly, bool, enum, Guid, plus a fallback.

### Dependency Injection
All services are registered via `ServiceCollectionExtensions.AddAuditServices()`. Core registrations:
- `AuditConfiguration` — Singleton
- `IAuditPropertyCache` / `IAuditValueConverter` — Singleton
- `IAuditRecordFactory` / `IAuditFieldProcessor` / `IAuditErrorHandler` — Scoped
- `AuditableInterceptor` — Scoped

### Legacy Support
`LegacyAuditableInterceptor` provides backward compatibility for codebases not using DI.

## Key Interfaces

- **`IAuditable`** — Entities must implement this (`Id`, `ModifiedDate`, `ModifiedUser`)
- **`IAuditRecordFactory`** — Creates audit records from entity changes
- **`IAuditFieldProcessor`** — Processes individual field changes
- **`IAuditValueConverter`** — Converts values using type handler strategy
- **`IAuditPropertyCache`** — Caches reflection-based property lookups
- **`IAuditErrorHandler`** — Centralized error handling
- **`IAuditTypeHandler`** — Per-type value conversion strategy

## Configuration

### AuditConfiguration Properties
| Property | Default | Description |
|---|---|---|
| `EnableSensitiveDataMasking` | `true` | Mask sensitive fields in audit logs |
| `EnableFieldLevelAuditing` | `true` | Track individual field changes |
| `ContinueOnFieldProcessingError` | `true` | Continue if a field fails processing |
| `DefaultMaskValue` | `"***MASKED***"` | Replacement text for masked values |
| `DefaultSystemUser` | `"System"` | Fallback user when none specified |
| `EnableDetailedLogging` | `false` | Verbose logging for debugging |

## Common Development Workflows

### Adding a New Type Handler
1. Create a class implementing `IAuditTypeHandler`
2. Implement `CanHandle(Type)` and `SetValues(AuditRecordField, object?, object?, string?)`
3. Register via `services.AddCustomAuditTypeHandlers(new YourHandler())`

### Making an Entity Auditable
1. Implement `IAuditable` on the entity class
2. Optionally decorate properties with `[AuditField]` to control display name, sensitivity, ordering, and field type

### Integrating the Library
```csharp
// Register services
services.AddAuditServices(config =>
{
    config.EnableSensitiveDataMasking = true;
    config.EnableDetailedLogging = true;
});

// Add interceptor to DbContext
services.AddDbContext<YourDbContext>((sp, options) =>
{
    options.UseSqlServer(connectionString);
    options.AddInterceptors(sp.GetRequiredService<AuditableInterceptor>());
});
```

## Important Notes

- The namespace root is `Data.Audit` with sub-namespaces for each subfolder
- The project uses file-scoped namespaces
- All public interfaces follow the `I` prefix convention
- The library does **not** define `AuditRecord` or `AuditRecordField` entities — those are expected to exist in the consuming project's DbContext

## Best Practices

### Code Style
- Use nullable reference types (`<Nullable>enable</Nullable>`)
- XML documentation comments on all public members
- Constructor injection for all dependencies
- `sealed` on attribute classes
- `TryAdd*` for DI registrations to avoid overwriting consumer-defined services

### Services Design
- Interfaces for all services (testability)
- Singletons for stateless/cached services, Scoped for context-dependent services
- Strategy pattern for extensibility (type handlers)

### Performance
- Cache reflection results via `IAuditPropertyCache`
- Use `TryAdd` to avoid redundant registrations
- Async support throughout the interceptor pipeline

## Dependencies

| Package | Version | Purpose |
|---|---|---|
| Microsoft.EntityFrameworkCore.SqlServer | 8.0.6 | EF Core SQL Server provider |
| Microsoft.EntityFrameworkCore.Design | 8.0.6 | Design-time EF tooling |
| Microsoft.EntityFrameworkCore.Tools | 8.0.4 | EF CLI tools |

## Git Workflow

## Version Control Guidelines

- **NEVER** commit changes without user approval. Ask systematically for approval before committing.
- Commit messages should be clear and follow convention:
  - ai-tooling: AI agents, automation commands, workflows, or other AI-enabled developer tooling
  - feat: New feature
  - fix: Bug fix
  - docs: Documentation
  - style: Formatting
  - refactor: Code restructuring
  - test: Adding tests
  - chore: Maintenance tasks
- **NEVER** mention AI/Claude authorship in commit messages (no "Generated with Claude Code", "AI-assisted", etc.)

## Troubleshooting

### Build Errors
- Ensure .NET 8.0 SDK is installed
- Run `dotnet restore` if NuGet packages are missing
- Check that EF Core package versions are compatible (all should be 8.0.x)

### Common Issues
- If `AuditRecord` / `AuditRecordField` types are not found, they must be defined in the consuming project
- If audit records aren't being created, verify the entity implements `IAuditable` and the interceptor is registered
