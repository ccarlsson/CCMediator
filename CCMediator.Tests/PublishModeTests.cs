using CCMediator;
using CCMediator.Implementation;
using CCMediator.Internal;
using Moq;
using Xunit;

namespace CCMediator.Tests;

public class PublishModeTests
{
    [Fact]
    public async Task Publish_SequentialMode_Should_Invoke_Handlers_In_Order()
    {
        var events = new List<string>();

        var resolver = new Mock<IHandlerResolver>();
        var options = new CCMediatorOptions { NotificationPublishMode = NotificationPublishMode.Sequential };
        var mediator = new Mediator(resolver.Object, options);

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

        resolver.Setup(r => r.GetNotificationHandlers(typeof(TestNotification)))
            .Returns(new object[] { handler1.Object, handler2.Object });

        await mediator.Publish(notification);

        Assert.Equal(new[] { "h1", "h2" }, events);
    }

    [Fact]
    public async Task Publish_ParallelMode_Should_Invoke_All_Handlers()
    {
        var resolver = new Mock<IHandlerResolver>();
        var options = new CCMediatorOptions { NotificationPublishMode = NotificationPublishMode.Parallel };
        var mediator = new Mediator(resolver.Object, options);

        var notification = new TestNotification();

        var handler1 = new Mock<INotificationHandler<TestNotification>>();
        var handler2 = new Mock<INotificationHandler<TestNotification>>();

        handler1.Setup(h => h.Handle(notification, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        handler2.Setup(h => h.Handle(notification, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        resolver.Setup(r => r.GetNotificationHandlers(typeof(TestNotification)))
            .Returns(new object[] { handler1.Object, handler2.Object });

        await mediator.Publish(notification);

        handler1.Verify(h => h.Handle(notification, It.IsAny<CancellationToken>()), Times.Once);
        handler2.Verify(h => h.Handle(notification, It.IsAny<CancellationToken>()), Times.Once);
    }
}
