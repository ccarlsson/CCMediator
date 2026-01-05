using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Xunit;
using CCMediator;

namespace CCMediator.Tests;

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
        var services = new ServiceCollection();

        services.AddCCMediatorWithScanning(Assembly.GetExecutingAssembly(), typeof(IMediator).Assembly);
        var provider = services.BuildServiceProvider();

        var mediator = provider.GetService<IMediator>();
        Assert.NotNull(mediator);

        services = new ServiceCollection();
        services.AddCCMediatorWithScanning(Assembly.GetExecutingAssembly());
        services.AddTransient<INotificationHandler<DummyNotification>, DummyNotificationHandler>();
        provider = services.BuildServiceProvider();

        var handlers = provider.GetServices<INotificationHandler<DummyNotification>>();
        Assert.NotNull(handlers);
        Assert.NotEmpty(handlers);
    }
}
