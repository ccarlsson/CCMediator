using Moq;
using Xunit;
using CCMediator;
using CCMediator.Implementation;
using CCMediator.Internal;

namespace CCMediator.Tests;

public class PipelineBehaviorTests
{
    private readonly Mock<IHandlerResolver> _resolverMock;
    private readonly Mediator _mediator;

    public PipelineBehaviorTests()
    {
        _resolverMock = new Mock<IHandlerResolver>();
        _mediator = new Mediator(_resolverMock.Object, new CCMediatorOptions());
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

        _resolverMock
            .Setup(r => r.GetSingleRequestHandler(typeof(TestRequest), typeof(string)))
            .Returns(handlerMock.Object);

        _resolverMock
            .Setup(r => r.GetPipelineBehaviors(typeof(TestRequest), typeof(string)))
            .Returns(new object[] { b1.Object, b2.Object });

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

        _resolverMock
            .Setup(r => r.GetSingleRequestHandler(typeof(TestRequest), typeof(string)))
            .Returns(handlerMock.Object);

        _resolverMock
            .Setup(r => r.GetPipelineBehaviors(typeof(TestRequest), typeof(string)))
            .Returns(new object[] { behavior.Object });

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

        _resolverMock
            .Setup(r => r.GetSingleRequestHandler(typeof(TestRequest), typeof(string)))
            .Returns(handlerMock.Object);

        _resolverMock
            .Setup(r => r.GetPipelineBehaviors(typeof(TestRequest), typeof(string)))
            .Returns(new object[] { behavior.Object });

        var response = await _mediator.Send(request);

        Assert.Equal("base-modified", response);
    }
}
