using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace SimpleMediator;

// Extension methods for DI registration
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSimpleMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        // Register the mediator
        services.AddScoped<IMediator, Mediator>();

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