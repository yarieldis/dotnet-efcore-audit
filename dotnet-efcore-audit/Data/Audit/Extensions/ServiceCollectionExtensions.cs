using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;
using Unctad.eRegulations.Library.Data.Audit.Caching;
using Unctad.eRegulations.Library.Data.Audit.Configuration;
using Unctad.eRegulations.Library.Data.Audit.ErrorHandling;
using Unctad.eRegulations.Library.Data.Audit.Services;
using Unctad.eRegulations.Library.Data.Audit.TypeHandlers;

namespace Unctad.eRegulations.Library.Data.Audit.Extensions;

/// <summary>
/// Extension methods for configuring audit services in the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds audit services to the service collection with default configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAuditServices(this IServiceCollection services)
    {
        return services.AddAuditServices(new AuditConfiguration());
    }

    /// <summary>
    /// Adds audit services to the service collection with custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The audit configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAuditServices(this IServiceCollection services, AuditConfiguration configuration)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

        // Register configuration
        services.TryAddSingleton(configuration);

        // Register core services
        services.TryAddSingleton<IAuditPropertyCache, AuditPropertyCache>();
        services.TryAddSingleton<IAuditValueConverter, AuditValueConverter>();
        services.TryAddScoped<IAuditRecordFactory, AuditRecordFactory>();
        services.TryAddScoped<IAuditFieldProcessor, AuditFieldProcessor>();
        services.TryAddScoped<IAuditErrorHandler, DefaultAuditErrorHandler>();

        // Register the interceptor
        services.TryAddScoped<AuditableInterceptor>();

        return services;
    }

    /// <summary>
    /// Adds audit services with a custom configuration action.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureAction">Action to configure audit settings.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAuditServices(this IServiceCollection services, Action<AuditConfiguration> configureAction)
    {
        if (configureAction == null) throw new ArgumentNullException(nameof(configureAction));

        var configuration = new AuditConfiguration();
        configureAction(configuration);

        return services.AddAuditServices(configuration);
    }

    /// <summary>
    /// Adds custom type handlers for audit value conversion.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="typeHandlers">The custom type handlers.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCustomAuditTypeHandlers(this IServiceCollection services, params IAuditTypeHandler[] typeHandlers)
    {
        if (typeHandlers?.Length > 0)
        {
            services.TryAddSingleton<IAuditValueConverter>(provider =>
                new AuditValueConverter(typeHandlers.AsEnumerable()));
        }

        return services;
    }
}