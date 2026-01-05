using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using SimpleMediator;

namespace Benchmarks;

[MemoryDiagnoser]
public class MediatorBenchmarks
{
    private IMediator _mediator = null!;
    private ServiceProvider _sp = null!;
    private Ping _ping = new();

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddSimpleMediatorWithScanning(typeof(MediatorBenchmarks).Assembly);
        _sp = services.BuildServiceProvider();
        _mediator = _sp.GetRequiredService<IMediator>();
    }

    [Benchmark]
    public Task<int> Send_Request() => _mediator.Send<int>(_ping);

    [Benchmark]
    public Task Publish_Notification() => _mediator.Publish(new Ponged());

    [GlobalCleanup]
    public void Cleanup() => _sp.Dispose();

    public sealed record Ping() : IRequest<int>;

    public sealed class PingHandler : IRequestHandler<Ping, int>
    {
        public Task<int> Handle(Ping request, CancellationToken cancellationToken) => Task.FromResult(42);
    }

    public sealed record Ponged() : INotification;

    public sealed class PongedHandler : INotificationHandler<Ponged>
    {
        public Task Handle(Ponged notification, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}