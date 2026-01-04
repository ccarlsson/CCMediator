using Moq;
using Xunit;

namespace SimpleMediator.Tests;

public class MediatorAdditionalTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mediator _mediator;

    public MediatorAdditionalTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _mediator = new Mediator(_serviceProviderMock.Object);
    }

    [Fact]
    public async Task Send_Should_Pass_CancellationToken_To_Handler()
    {
        // Arrange
        var request = new TestRequest { Message = "Hello" };
        var handlerMock = new Mock<IRequestHandler<TestRequest, string>>();
        var token = new CancellationTokenSource().Token;

        handlerMock
            .Setup(h => h.Handle(request, It.Is<CancellationToken>(ct => ct.Equals(token))))
            .ReturnsAsync("Handled: Hello");

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IRequestHandler<TestRequest, string>>)))
            .Returns(new[] { handlerMock.Object });

        // Act
        var response = await _mediator.Send(request, token);

        // Assert
        Assert.Equal("Handled: Hello", response);
        handlerMock.Verify(h => h.Handle(request, It.Is<CancellationToken>(ct => ct.Equals(token))), Times.Once);
    }

    [Fact]
    public async Task Send_Should_Propagate_Handler_Exception()
    {
        // Arrange
        var request = new TestRequest { Message = "Boom" };
        var handlerMock = new Mock<IRequestHandler<TestRequest, string>>();
        handlerMock
            .Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("handler failed"));

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IRequestHandler<TestRequest, string>>)))
            .Returns(new[] { handlerMock.Object });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _mediator.Send(request));
        Assert.Equal("handler failed", ex.Message);
    }

    [Fact]
    public async Task Send_Should_Wrap_Di_Resolution_Errors_In_MediatorException()
    {
        // Arrange
        var request = new TestRequest { Message = "Hello" };

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IRequestHandler<TestRequest, string>>)))
            .Throws(new InvalidOperationException("di failed"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<MediatorException>(() => _mediator.Send(request));
        Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Equal("di failed", ex.InnerException!.Message);
    }

    [Fact]
    public async Task Publish_Should_Be_NoOp_When_No_Handlers()
    {
        // Arrange
        var notification = new TestNotification();

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<INotificationHandler<TestNotification>>)))
            .Returns(Enumerable.Empty<INotificationHandler<TestNotification>>());

        // Act & Assert (should not throw)
        await _mediator.Publish(notification);
    }

    [Fact]
    public async Task Publish_Should_Propagate_CancellationToken_To_All_Handlers()
    {
        // Arrange
        var notification = new TestNotification();
        var token = new CancellationTokenSource().Token;

        var handlerMock1 = new Mock<INotificationHandler<TestNotification>>();
        var handlerMock2 = new Mock<INotificationHandler<TestNotification>>();

        handlerMock1
            .Setup(h => h.Handle(notification, It.Is<CancellationToken>(ct => ct.Equals(token))))
            .Returns(Task.CompletedTask);
        handlerMock2
            .Setup(h => h.Handle(notification, It.Is<CancellationToken>(ct => ct.Equals(token))))
            .Returns(Task.CompletedTask);

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<INotificationHandler<TestNotification>>)))
            .Returns(new List<INotificationHandler<TestNotification>> { handlerMock1.Object, handlerMock2.Object });

        // Act
        await _mediator.Publish(notification, token);

        // Assert
        handlerMock1.Verify(h => h.Handle(notification, It.Is<CancellationToken>(ct => ct.Equals(token))), Times.Once);
        handlerMock2.Verify(h => h.Handle(notification, It.Is<CancellationToken>(ct => ct.Equals(token))), Times.Once);
    }

    [Fact]
    public async Task Publish_Should_Throw_When_Any_Handler_Throws()
    {
        // Arrange
        var notification = new TestNotification();

        var handlerMock1 = new Mock<INotificationHandler<TestNotification>>();
        var handlerMock2 = new Mock<INotificationHandler<TestNotification>>();

        handlerMock1
            .Setup(h => h.Handle(notification, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("bad handler"));
        handlerMock2
            .Setup(h => h.Handle(notification, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<INotificationHandler<TestNotification>>)))
            .Returns(new List<INotificationHandler<TestNotification>> { handlerMock1.Object, handlerMock2.Object });

        // Act & Assert
        await Assert.ThrowsAsync<AggregateException>(() => _mediator.Publish(notification));
    }

    [Fact]
    public async Task Publish_Should_Wrap_Di_Resolution_Errors_In_MediatorException()
    {
        // Arrange
        var notification = new TestNotification();

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<INotificationHandler<TestNotification>>)))
            .Throws(new InvalidOperationException("di failed"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<MediatorException>(() => _mediator.Publish(notification));
        Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Equal("di failed", ex.InnerException!.Message);
    }

    [Fact]
    public async Task Send_Should_Work_Repeatedly_With_Same_RequestType()
    {
        // Arrange
        var request1 = new TestRequest { Message = "One" };
        var request2 = new TestRequest { Message = "Two" };
        var handlerMock = new Mock<IRequestHandler<TestRequest, string>>();
        handlerMock
            .SetupSequence(h => h.Handle(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Handled: One")
            .ReturnsAsync("Handled: Two");

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IRequestHandler<TestRequest, string>>)))
            .Returns(new[] { handlerMock.Object });

        // Act
        var r1 = await _mediator.Send(request1);
        var r2 = await _mediator.Send(request2);

        // Assert
        Assert.Equal("Handled: One", r1);
        Assert.Equal("Handled: Two", r2);
        handlerMock.Verify(h => h.Handle(request1, It.IsAny<CancellationToken>()), Times.Once);
        handlerMock.Verify(h => h.Handle(request2, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Publish_Sequential_StopOnFirstException_Should_Not_Invoke_Subsequent_Handlers()
    {
        var notification = new TestNotification();

        var handlerMock1 = new Mock<INotificationHandler<TestNotification>>();
        var handlerMock2 = new Mock<INotificationHandler<TestNotification>>();

        handlerMock1
            .Setup(h => h.Handle(notification, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("bad handler"));
        handlerMock2
            .Setup(h => h.Handle(notification, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<INotificationHandler<TestNotification>>)))
            .Returns(new List<INotificationHandler<TestNotification>> { handlerMock1.Object, handlerMock2.Object });

        var mediator = new Mediator(_serviceProviderMock.Object, new SimpleMediatorOptions
        {
            NotificationPublishMode = NotificationPublishMode.Sequential,
            SequentialPublishErrorHandling = NotificationPublishErrorHandling.StopOnFirstException
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Publish(notification));
        handlerMock2.Verify(h => h.Handle(notification, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Publish_Sequential_ContinueAndAggregateExceptions_Should_Invoke_All_Handlers_And_Aggregate()
    {
        var notification = new TestNotification();

        var handlerMock1 = new Mock<INotificationHandler<TestNotification>>();
        var handlerMock2 = new Mock<INotificationHandler<TestNotification>>();
        var handlerMock3 = new Mock<INotificationHandler<TestNotification>>();

        handlerMock1
            .Setup(h => h.Handle(notification, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("bad handler 1"));
        handlerMock2
            .Setup(h => h.Handle(notification, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        handlerMock3
            .Setup(h => h.Handle(notification, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("bad handler 2"));

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<INotificationHandler<TestNotification>>)))
            .Returns(new List<INotificationHandler<TestNotification>> { handlerMock1.Object, handlerMock2.Object, handlerMock3.Object });

        var mediator = new Mediator(_serviceProviderMock.Object, new SimpleMediatorOptions
        {
            NotificationPublishMode = NotificationPublishMode.Sequential,
            SequentialPublishErrorHandling = NotificationPublishErrorHandling.ContinueAndAggregateExceptions
        });

        var ex = await Assert.ThrowsAsync<AggregateException>(() => mediator.Publish(notification));
        Assert.Equal(2, ex.InnerExceptions.Count);
        handlerMock2.Verify(h => h.Handle(notification, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Publish_Parallel_AggregateExceptionsInParallel_False_Should_Throw_First_Observed_Exception()
    {
        var notification = new TestNotification();

        var gate = new TaskCompletionSource();

        var handler1 = new Mock<INotificationHandler<TestNotification>>();
        handler1
            .Setup(h => h.Handle(notification, It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Yield();
                throw new InvalidOperationException("fast fail");
            });

        var handler2 = new Mock<INotificationHandler<TestNotification>>();
        handler2
            .Setup(h => h.Handle(notification, It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await gate.Task;
                throw new InvalidOperationException("slow fail");
            });

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<INotificationHandler<TestNotification>>)))
            .Returns(new List<INotificationHandler<TestNotification>> { handler1.Object, handler2.Object });

        var mediator = new Mediator(_serviceProviderMock.Object, new SimpleMediatorOptions
        {
            NotificationPublishMode = NotificationPublishMode.Parallel,
            AggregateExceptionsInParallel = false
        });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Publish(notification));
        Assert.Equal("fast fail", ex.Message);

        gate.TrySetResult();
    }

    [Fact]
    public async Task Publish_Parallel_AggregateExceptionsInParallel_True_Should_Aggregate()
    {
        var notification = new TestNotification();

        var handler1 = new Mock<INotificationHandler<TestNotification>>();
        var handler2 = new Mock<INotificationHandler<TestNotification>>();

        handler1
            .Setup(h => h.Handle(notification, It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Yield();
                throw new InvalidOperationException("bad handler 1");
            });
        handler2
            .Setup(h => h.Handle(notification, It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Yield();
                throw new InvalidOperationException("bad handler 2");
            });

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<INotificationHandler<TestNotification>>)))
            .Returns(new List<INotificationHandler<TestNotification>> { handler1.Object, handler2.Object });

        var mediator = new Mediator(_serviceProviderMock.Object, new SimpleMediatorOptions
        {
            NotificationPublishMode = NotificationPublishMode.Parallel,
            AggregateExceptionsInParallel = true
        });

        var ex = await Assert.ThrowsAsync<AggregateException>(() => mediator.Publish(notification));
        Assert.NotEmpty(ex.InnerExceptions);
    }
}
