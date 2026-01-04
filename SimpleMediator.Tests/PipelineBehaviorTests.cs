using Moq;
using Xunit;

namespace SimpleMediator.Tests;

public class PipelineBehaviorTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mediator _mediator;

    public PipelineBehaviorTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _mediator = new Mediator(_serviceProviderMock.Object, new SimpleMediatorOptions());
    }

    [Fact]
    public async Task Send_Should_Execute_Behaviors_In_Registration_Order()
    {
        var events = new List<string>();

        var request = new TestRequest { Message = "Hello" };

        var handlerMock = new Mock<IRequestHandler<TestRequest, string>>();
        handlerMock
            .Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync("ok")
            .Callback(() => events.Add("handler"));

        var b1 = new Mock<IPipelineBehavior<TestRequest, string>>();
        b1.Setup(b => b.Handle(request, It.IsAny<Func<Task<string>>>(), It.IsAny<CancellationToken>()))
            .Returns<TestRequest, Func<Task<string>>, CancellationToken>(async (_, next, __) =>
            {
                events.Add("b1-before");
                var result = await next();
                events.Add("b1-after");
                return result;
            });

        var b2 = new Mock<IPipelineBehavior<TestRequest, string>>();
        b2.Setup(b => b.Handle(request, It.IsAny<Func<Task<string>>>(), It.IsAny<CancellationToken>()))
            .Returns<TestRequest, Func<Task<string>>, CancellationToken>(async (_, next, __) =>
            {
                events.Add("b2-before");
                var result = await next();
                events.Add("b2-after");
                return result;
            });

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IRequestHandler<TestRequest, string>>)))
            .Returns(new[] { handlerMock.Object });

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestRequest, string>>)))
            .Returns(new[] { b1.Object, b2.Object });

        var response = await _mediator.Send(request);

        Assert.Equal("ok", response);
        Assert.Equal(new[] { "b1-before", "b2-before", "handler", "b2-after", "b1-after" }, events);
    }

    [Fact]
    public async Task Send_Behavior_Can_ShortCircuit_Handler()
    {
        var request = new TestRequest { Message = "Hello" };

        var handlerMock = new Mock<IRequestHandler<TestRequest, string>>();
        handlerMock
            .Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync("handler");

        var behavior = new Mock<IPipelineBehavior<TestRequest, string>>();
        behavior
            .Setup(b => b.Handle(request, It.IsAny<Func<Task<string>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("short");

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IRequestHandler<TestRequest, string>>)))
            .Returns(new[] { handlerMock.Object });

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestRequest, string>>)))
            .Returns(new[] { behavior.Object });

        var response = await _mediator.Send(request);

        Assert.Equal("short", response);
        handlerMock.Verify(h => h.Handle(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Send_Behavior_Can_Modify_Response()
    {
        var request = new TestRequest { Message = "Hello" };

        var handlerMock = new Mock<IRequestHandler<TestRequest, string>>();
        handlerMock
            .Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync("base");

        var behavior = new Mock<IPipelineBehavior<TestRequest, string>>();
        behavior
            .Setup(b => b.Handle(request, It.IsAny<Func<Task<string>>>(), It.IsAny<CancellationToken>()))
            .Returns<TestRequest, Func<Task<string>>, CancellationToken>(async (_, next, __) =>
            {
                var result = await next();
                return result + "-modified";
            });

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IRequestHandler<TestRequest, string>>)))
            .Returns(new[] { handlerMock.Object });

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<TestRequest, string>>)))
            .Returns(new[] { behavior.Object });

        var response = await _mediator.Send(request);

        Assert.Equal("base-modified", response);
    }
}
