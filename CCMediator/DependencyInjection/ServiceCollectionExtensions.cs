using Microsoft.Extensions.DependencyInjection;
using CCMediator.Abstractions;
using CCMediator.Configuration;
using CCMediator.Implementation;
using System.Reflection;

namespace CCMediator.DependencyInjection;

/// <summary>
/// Extension methods for registering SimpleMediator services with Microsoft.Extensions.DependencyInjection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the SimpleMediator core services.
    /// </summary>
    /// <remarks>
    /// This overload does not scan assemblies; handlers and pipeline behaviors must be registered explicitly.
    /// </remarks>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection instance.</returns>
    public static IServiceCollection AddSimpleMediator(this IServiceCollection services)
        => services.AddSimpleMediator(static _ => { });

    /// <summary>
    /// Registers the SimpleMediator core services.
    /// </summary>
    /// <remarks>
    /// This overload does not scan assemblies; handlers and pipeline behaviors must be registered explicitly.
    /// </remarks>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Callback used to configure <see cref="SimpleMediatorOptions"/>.</param>
    /// <returns>The same service collection instance.</returns>
    public static IServiceCollection AddSimpleMediator(this IServiceCollection services, Action<SimpleMediatorOptions> configure)
    {
        var options = new SimpleMediatorOptions();
        configure(options);

        services.AddSingleton(options);

        // Register the mediator
        services.AddScoped<IMediator, Mediator>();

        return services;
    }

    /// <summary>
    /// Registers the SimpleMediator core services and scans the specified assemblies
    /// to register all <see cref="IRequestHandler{TRequest, TResponse}"/> and <see cref="INotificationHandler{TNotification}"/> implementations.
    /// </summary>
    /// <remarks>
    /// This method performs reflection-based scanning (e.g. <see cref="Assembly.GetTypes"/>) and is an explicit opt-in.
    /// For maximum startup performance and predictability, prefer <see cref="AddSimpleMediator(IServiceCollection)"/> with explicit registrations.
    /// </remarks>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">Assemblies to scan for handler implementations.</param>
    /// <returns>The same service collection instance.</returns>
    public static IServiceCollection AddSimpleMediatorWithScanning(this IServiceCollection services, params Assembly[] assemblies)
        => services.AddSimpleMediatorWithScanning(static _ => { }, assemblies);

    /// <summary>
    /// Registers the SimpleMediator core services and scans the specified assemblies
    /// to register all <see cref="IRequestHandler{TRequest, TResponse}"/> and <see cref="INotificationHandler{TNotification}"/> implementations.
    /// </summary>
    /// <remarks>
    /// This method performs reflection-based scanning (e.g. <see cref="Assembly.GetTypes"/>) and is an explicit opt-in.
    /// For maximum startup performance and predictability, prefer <see cref="AddSimpleMediator(IServiceCollection, Action{SimpleMediatorOptions})"/> with explicit registrations.
    /// </remarks>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Callback used to configure <see cref="SimpleMediatorOptions"/>.</param>
    /// <param name="assemblies">Assemblies to scan for handler implementations.</param>
    /// <returns>The same service collection instance.</returns>
    public static IServiceCollection AddSimpleMediatorWithScanning(this IServiceCollection services, Action<SimpleMediatorOptions> configure, params Assembly[] assemblies)
    {
        services.AddSimpleMediator(configure);

        // Register all request handlers
        RegisterRequestHandlers(services, assemblies);

        // Register all notification handlers
        RegisterNotificationHandlers(services, assemblies);

        return services;
    }

    private static void RegisterRequestHandlers(IServiceCollection services, Assembly[] assemblies)
    {
        var handlerTypes = from assembly in assemblies
                           from type in assembly.GetTypes()
                           from interfaceType in type.GetInterfaces()
                           where interfaceType.IsGenericType &&
                                 (interfaceType.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                           select new { HandlerType = type, InterfaceType = interfaceType };

        foreach (var handler in handlerTypes)
        {
            services.AddTransient(handler.InterfaceType, handler.HandlerType);
        }
    }

    private static void RegisterNotificationHandlers(IServiceCollection services, Assembly[] assemblies)
    {
        var handlerTypes = from assembly in assemblies
                           from type in assembly.GetTypes()
                           from interfaceType in type.GetInterfaces()
                           where interfaceType.IsGenericType &&
                                 interfaceType.GetGenericTypeDefinition() == typeof(INotificationHandler<>)
                           select new { HandlerType = type, InterfaceType = interfaceType };

        foreach (var handler in handlerTypes)
        {
            services.AddTransient(handler.InterfaceType, handler.HandlerType);
        }
    }
}