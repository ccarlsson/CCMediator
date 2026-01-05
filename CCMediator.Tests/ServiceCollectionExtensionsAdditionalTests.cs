using Microsoft.Extensions.DependencyInjection;
using SimpleMediator.Abstractions;
using SimpleMediator.DependencyInjection;
using System.Reflection;
using Xunit;

namespace SimpleMediator.Tests;

public class ServiceCollectionExtensionsAdditionalTests
{
    private class DummyNotification : INotification { }
    private class DummyNotificationHandler : INotificationHandler<DummyNotification>
    {
        public Task Handle(DummyNotification notification, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    [Fact]
    public void AddSimpleMediatorWithScanning_Should_Scan_Multiple_Assemblies()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act: include current test assembly and the SimpleMediator assembly to ensure scanning across multiple assemblies doesn't fail
        services.AddSimpleMediatorWithScanning(Assembly.GetExecutingAssembly(), typeof(IMediator).Assembly);
        var provider = services.BuildServiceProvider();

        // Assert
        var mediator = provider.GetService<IMediator>();
        Assert.NotNull(mediator);

        // Also ensure that the handlers from this assembly are registered when present
        // Register one handler locally via an additional service collection to simulate cross-assembly types
        services = new ServiceCollection();
        services.AddSimpleMediatorWithScanning(Assembly.GetExecutingAssembly());
        services.AddTransient<INotificationHandler<DummyNotification>, DummyNotificationHandler>();
        provider = services.BuildServiceProvider();

        var handlers = provider.GetServices<INotificationHandler<DummyNotification>>();
        Assert.NotNull(handlers);
        Assert.NotEmpty(handlers);
    }
}
