using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace SimpleMediator;

// Extension methods for DI registration
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the SimpleMediator core services.
    /// 
    /// This overload does not scan assemblies; handlers/pipeline behaviors must be registered explicitly.
    /// </summary>
    public static IServiceCollection AddSimpleMediator(this IServiceCollection services)
        => services.AddSimpleMediator(static _ => { });

    /// <summary>
    /// Registers the SimpleMediator core services.
    /// 
    /// This overload does not scan assemblies; handlers/pipeline behaviors must be registered explicitly.
    /// </summary>
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
    /// to register all <c>IRequestHandler&lt;,&gt;</c> and <c>INotificationHandler&lt;&gt;</c> implementations.
    /// 
    /// This overload performs reflection-based scanning and is an explicit opt-in.
    /// </summary>
    public static IServiceCollection AddSimpleMediatorWithScanning(this IServiceCollection services, params Assembly[] assemblies)
        => services.AddSimpleMediatorWithScanning(static _ => { }, assemblies);

    /// <summary>
    /// Registers the SimpleMediator core services and scans the specified assemblies
    /// to register all <c>IRequestHandler&lt;,&gt;</c> and <c>INotificationHandler&lt;&gt;</c> implementations.
    /// 
    /// This overload performs reflection-based scanning and is an explicit opt-in.
    /// </summary>
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