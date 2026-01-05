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
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCCMediator();
        var provider = services.BuildServiceProvider();

        // Assert
        var mediator = provider.GetService<IMediator>();
        Assert.NotNull(mediator);
        Assert.IsType<Mediator>(mediator);
    }

    [Fact]
    public void AddSimpleMediator_Should_Not_Register_Handlers_By_Default()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCCMediator();
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.Null(provider.GetService<IRequestHandler<TestRequest, string>>());
        Assert.Empty(provider.GetServices<INotificationHandler<TestNotification>>());
    }

    [Fact]
    public void AddSimpleMediatorWithScanning_Should_Register_RequestHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCCMediatorWithScanning(Assembly.GetExecutingAssembly());
        var provider = services.BuildServiceProvider();

        // Assert
        var handler = provider.GetService<IRequestHandler<TestRequest, string>>();
        Assert.NotNull(handler);
        Assert.IsType<TestRequestHandler>(handler);
    }

    [Fact]
    public void AddSimpleMediatorWithScanning_Should_Register_NotificationHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCCMediatorWithScanning(Assembly.GetExecutingAssembly());
        var provider = services.BuildServiceProvider();

        // Assert
        var handlers = provider.GetServices<INotificationHandler<TestNotification>>().ToList();
        Assert.NotEmpty(handlers);
        Assert.Single(handlers);
        Assert.IsType<TestNotificationHandler>(handlers.First());
    }
}

