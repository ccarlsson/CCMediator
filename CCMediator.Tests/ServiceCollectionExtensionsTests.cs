using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;
using Xunit;
using CCMediator;
using CCMediator.Implementation;

namespace CCMediator.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddSimpleMediator_Should_Register_IMediator()
    {
        var services = new ServiceCollection();

        services.AddCCMediator();
        var provider = services.BuildServiceProvider();

        var mediator = provider.GetService<IMediator>();
        Assert.NotNull(mediator);
        Assert.IsType<Mediator>(mediator);
    }

    [Fact]
    public void AddSimpleMediator_Should_Not_Register_Handlers_By_Default()
    {
        var services = new ServiceCollection();

        services.AddCCMediator();
        var provider = services.BuildServiceProvider();

        Assert.Null(provider.GetService<IRequestHandler<TestRequest, string>>());
        Assert.Empty(provider.GetServices<INotificationHandler<TestNotification>>());
    }

    [Fact]
    public void AddSimpleMediatorWithScanning_Should_Register_RequestHandlers()
    {
        var services = new ServiceCollection();

        services.AddCCMediatorWithScanning(Assembly.GetExecutingAssembly());
        var provider = services.BuildServiceProvider();

        var handler = provider.GetService<IRequestHandler<TestRequest, string>>();
        Assert.NotNull(handler);
        Assert.IsType<TestRequestHandler>(handler);
    }

    [Fact]
    public void AddSimpleMediatorWithScanning_Should_Register_NotificationHandlers()
    {
        var services = new ServiceCollection();

        services.AddCCMediatorWithScanning(Assembly.GetExecutingAssembly());
        var provider = services.BuildServiceProvider();

        var handlers = provider.GetServices<INotificationHandler<TestNotification>>().ToList();
        Assert.NotEmpty(handlers);
        Assert.Single(handlers);
        Assert.IsType<TestNotificationHandler>(handlers.First());
    }
}

