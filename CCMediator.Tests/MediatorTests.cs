using Moq;
using Xunit;
using CCMediator;
using CCMediator.Implementation;
using CCMediator.Internal;

namespace CCMediator.Tests;

public class MediatorTests
{
    private readonly Mock<IHandlerResolver> _resolverMock;
    private readonly Mediator _mediator;

    public MediatorTests()
    {
        _resolverMock = new Mock<IHandlerResolver>();
        _mediator = new Mediator(_resolverMock.Object, new CCMediatorOptions());
    }

    [Fact]
    public async Task Send_Should_Invoke_RequestHandler()
    {
        // Arrange
        var request = new TestRequest { Message = "Hello" };
        var handlerMock = new Mock<IRequestHandler<TestRequest, string>>();
        handlerMock
            .Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Handled: Hello");

        _resolverMock
            .Setup(r => r.GetSingleRequestHandler(typeof(TestRequest), typeof(string)))
            .Returns(handlerMock.Object);

        _resolverMock
            .Setup(r => r.GetPipelineBehaviors(typeof(TestRequest), typeof(string)))
            .Returns(Enumerable.Empty<object>());

        // Act
        var response = await _mediator.Send(request);

        // Assert
        Assert.Equal("Handled: Hello", response);
        handlerMock.Verify(h => h.Handle(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Publish_Should_Invoke_All_NotificationHandlers()
    {
        // Arrange
        var notification = new TestNotification();
        var handlerMock1 = new Mock<INotificationHandler<TestNotification>>();
        var handlerMock2 = new Mock<INotificationHandler<TestNotification>>();

        handlerMock1
            .Setup(h => h.Handle(notification, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        handlerMock2
            .Setup(h => h.Handle(notification, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _resolverMock
            .Setup(r => r.GetNotificationHandlers(typeof(TestNotification)))
            .Returns(new object[] { handlerMock1.Object, handlerMock2.Object });

        // Act
        await _mediator.Publish(notification);

        // Assert
        handlerMock1.Verify(h => h.Handle(notification, It.IsAny<CancellationToken>()), Times.Once);
        handlerMock2.Verify(h => h.Handle(notification, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Send_Should_Throw_When_Handler_Not_Registered()
    {
        // Arrange
        var request = new TestRequest { Message = "Hello" };

        _resolverMock
            .Setup(r => r.GetSingleRequestHandler(typeof(TestRequest), typeof(string)))
            .Throws(new HandlerNotFoundException(typeof(TestRequest), typeof(string)));

        // Act & Assert
        await Assert.ThrowsAsync<HandlerNotFoundException>(() => _mediator.Send(request));
    }
}

