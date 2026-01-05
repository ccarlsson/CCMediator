using CCMediator.Abstractions;
using CCMediator.Configuration;
using CCMediator.Implementation;
using Moq;
using Xunit;

namespace CCMediator.Tests;

public class PublishModeTests
{
    [Fact]
    public async Task Publish_SequentialMode_Should_Invoke_Handlers_In_Order()
    {
        var events = new List<string>();

        var sp = new Mock<IServiceProvider>();
        var options = new SimpleMediatorOptions { NotificationPublishMode = NotificationPublishMode.Sequential };
        var mediator = new Mediator(sp.Object, options);

        var notification = new TestNotification();

        var handler1 = new Mock<INotificationHandler<TestNotification>>();
        handler1
            .Setup(h => h.Handle(notification, It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                events.Add("h1");
                return Task.CompletedTask;
            });

        var handler2 = new Mock<INotificationHandler<TestNotification>>();
        handler2
            .Setup(h => h.Handle(notification, It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                events.Add("h2");
                return Task.CompletedTask;
            });

        sp.Setup(x => x.GetService(typeof(IEnumerable<INotificationHandler<TestNotification>>)))
            .Returns(new[] { handler1.Object, handler2.Object });

        await mediator.Publish(notification);

        Assert.Equal(new[] { "h1", "h2" }, events);
    }

    [Fact]
    public async Task Publish_ParallelMode_Should_Invoke_All_Handlers()
    {
        var sp = new Mock<IServiceProvider>();
        var options = new SimpleMediatorOptions { NotificationPublishMode = NotificationPublishMode.Parallel };
        var mediator = new Mediator(sp.Object, options);

        var notification = new TestNotification();

        var handler1 = new Mock<INotificationHandler<TestNotification>>();
        var handler2 = new Mock<INotificationHandler<TestNotification>>();

        handler1.Setup(h => h.Handle(notification, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        handler2.Setup(h => h.Handle(notification, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        sp.Setup(x => x.GetService(typeof(IEnumerable<INotificationHandler<TestNotification>>)))
            .Returns(new[] { handler1.Object, handler2.Object });

        await mediator.Publish(notification);

        handler1.Verify(h => h.Handle(notification, It.IsAny<CancellationToken>()), Times.Once);
        handler2.Verify(h => h.Handle(notification, It.IsAny<CancellationToken>()), Times.Once);
    }
}
