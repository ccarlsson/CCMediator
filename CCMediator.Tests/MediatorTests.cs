using Moq;
using Xunit;
using CCMediator;
using CCMediator.Implementation;

namespace CCMediator.Tests;

public class MediatorTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mediator _mediator;

    public MediatorTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _mediator = new Mediator(_serviceProviderMock.Object, new CCMediatorOptions());
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

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IRequestHandler<TestRequest, string>>)))
            .Returns(new[] { handlerMock.Object });

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

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<INotificationHandler<TestNotification>>)))
            .Returns(new List<INotificationHandler<TestNotification>> { handlerMock1.Object, handlerMock2.Object });

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

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IRequestHandler<TestRequest, string>>)))
            .Returns(Array.Empty<IRequestHandler<TestRequest, string>>());

        // Act & Assert
        await Assert.ThrowsAsync<HandlerNotFoundException>(() => _mediator.Send(request));
    }
}

