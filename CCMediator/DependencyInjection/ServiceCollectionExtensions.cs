using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using CCMediator.Implementation;

namespace CCMediator;

/// <summary>
/// Extension methods for registering CCMediator services with Microsoft.Extensions.DependencyInjection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the CCMediator core services.
    /// </summary>
    /// <remarks>
    /// This overload does not scan assemblies; handlers and pipeline behaviors must be registered explicitly.
    /// </remarks>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection instance.</returns>
    public static IServiceCollection AddCCMediator(this IServiceCollection services)
        => services.AddCCMediator(static _ => { });

    /// <summary>
    /// Registers the CCMediator core services.
    /// </summary>
    /// <remarks>
    /// This overload does not scan assemblies; handlers and pipeline behaviors must be registered explicitly.
    /// </remarks>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Callback used to configure <see cref="CCMediatorOptions"/>.</param>
    /// <returns>The same service collection instance.</returns>
    public static IServiceCollection AddCCMediator(this IServiceCollection services, Action<CCMediatorOptions> configure)
    {
        var options = new CCMediatorOptions();
        configure(options);

        services.AddSingleton(options);

        // Register the mediator
        services.AddScoped<IMediator, Mediator>();

        return services;
    }

    /// <summary>
    /// Registers the CCMediator core services and scans the specified assemblies
    /// to register all <see cref="IRequestHandler{TRequest, TResponse}"/> and <see cref="INotificationHandler{TNotification}"/> implementations.
    /// </summary>
    /// <remarks>
    /// This method performs reflection-based scanning (e.g. <see cref="Assembly.GetTypes"/>) and is an explicit opt-in.
    /// For maximum startup performance and predictability, prefer <see cref="AddCCMediator(IServiceCollection)"/> with explicit registrations.
    /// </remarks>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">Assemblies to scan for handler implementations.</param>
    /// <returns>The same service collection instance.</returns>
    public static IServiceCollection AddCCMediatorWithScanning(this IServiceCollection services, params Assembly[] assemblies)
        => services.AddCCMediatorWithScanning(static _ => { }, assemblies);

    /// <summary>
    /// Registers the CCMediator core services and scans the specified assemblies
    /// to register all <see cref="IRequestHandler{TRequest, TResponse}"/> and <see cref="INotificationHandler{TNotification}"/> implementations.
    /// </summary>
    /// <remarks>
    /// This method performs reflection-based scanning (e.g. <see cref="Assembly.GetTypes"/>) and is an explicit opt-in.
    /// For maximum startup performance and predictability, prefer <see cref="AddCCMediator(IServiceCollection, Action{SimpleMediatorOptions})"/> with explicit registrations.
    /// </remarks>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Callback used to configure <see cref="CCMediatorOptions"/>.</param>
    /// <param name="assemblies">Assemblies to scan for handler implementations.</param>
    /// <returns>The same service collection instance.</returns>
    public static IServiceCollection AddCCMediatorWithScanning(this IServiceCollection services, Action<CCMediatorOptions> configure, params Assembly[] assemblies)
    {
        services.AddCCMediator(configure);

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