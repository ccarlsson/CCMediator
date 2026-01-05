using Microsoft.Extensions.DependencyInjection;
using CCMediator.Abstractions;
using CCMediator.Exceptions;
using CCMediator.Implementation;

namespace CCMediator.Tests;

public class MediatorSingleHandlerEnforcementTests
{
    private sealed record Ping(string Message) : IRequest<string>;

    private sealed class PingHandlerOne : IRequestHandler<Ping, string>
    {
        public Task<string> Handle(Ping request, CancellationToken cancellationToken) => Task.FromResult("one");
    }

    private sealed class PingHandlerTwo : IRequestHandler<Ping, string>
    {
        public Task<string> Handle(Ping request, CancellationToken cancellationToken) => Task.FromResult("two");
    }

    [Fact]
    public async Task Send_Should_Throw_When_Multiple_Handlers_Are_Registered()
    {
        var services = new ServiceCollection();
        services.AddScoped<IMediator, Mediator>();
        services.AddTransient<IRequestHandler<Ping, string>, PingHandlerOne>();
        services.AddTransient<IRequestHandler<Ping, string>, PingHandlerTwo>();

        using var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<MultipleHandlersFoundException>(() => mediator.Send(new Ping("test")));
    }
}
