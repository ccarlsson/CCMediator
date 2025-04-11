using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace SimpleMediator.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddSimpleMediator_Should_Register_IMediator()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act

        services.AddSimpleMediator(Assembly.GetExecutingAssembly());
        var provider = services.BuildServiceProvider();

        // Assert
        var mediator = provider.GetService<IMediator>();
        Assert.NotNull(mediator);
        Assert.IsType<Mediator>(mediator);
    }

    [Fact]
    public void AddSimpleMediator_Should_Register_RequestHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSimpleMediator(Assembly.GetExecutingAssembly());
        var provider = services.BuildServiceProvider();

        // Assert
        var handler = provider.GetService<IRequestHandler<TestRequest, string>>();
        Assert.NotNull(handler);
        Assert.IsType<TestRequestHandler>(handler);
    }

    [Fact]
    public void AddSimpleMediator_Should_Register_NotificationHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSimpleMediator(Assembly.GetExecutingAssembly());
        var provider = services.BuildServiceProvider();

        // Assert
        var handlers = provider.GetServices<INotificationHandler<TestNotification>>().ToList();
        Assert.NotEmpty(handlers);
        Assert.Single(handlers);
        Assert.IsType<TestNotificationHandler>(handlers.First());
    }
}

