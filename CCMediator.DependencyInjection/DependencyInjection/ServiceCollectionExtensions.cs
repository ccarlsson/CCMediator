using CCMediator.Implementation;
using CCMediator.Internal;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CCMediator;

/// <summary>
/// Extension methods for registering CCMediator services with Microsoft.Extensions.DependencyInjection.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCCMediator(this IServiceCollection services)
        => services.AddCCMediator(static _ => { });

    public static IServiceCollection AddCCMediator(this IServiceCollection services, Action<CCMediatorOptions> configure)
    {
        var options = new CCMediatorOptions();
        configure(options);

        services.AddSingleton(options);

        services.AddScoped<IHandlerResolver, ServiceProviderHandlerResolver>();
        services.AddScoped<IMediator, Mediator>();

        return services;
    }

    public static IServiceCollection AddCCMediatorWithScanning(this IServiceCollection services, params Assembly[] assemblies)
        => services.AddCCMediatorWithScanning(static _ => { }, assemblies);

    public static IServiceCollection AddCCMediatorWithScanning(this IServiceCollection services, Action<CCMediatorOptions> configure, params Assembly[] assemblies)
    {
        services.AddCCMediator(configure);

        RegisterRequestHandlers(services, assemblies);
        RegisterNotificationHandlers(services, assemblies);

        return services;
    }

    private static void RegisterRequestHandlers(IServiceCollection services, Assembly[] assemblies)
    {
        var handlerTypes = from assembly in assemblies
            from type in assembly.GetTypes()
            where type is { IsAbstract: false, IsInterface: false }
            from interfaceType in type.GetInterfaces()
            where interfaceType.IsGenericType &&
                  interfaceType.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)
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
            where type is { IsAbstract: false, IsInterface: false }
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
