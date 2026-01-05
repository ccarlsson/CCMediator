# CCMediator

A lightweight, dependency-injection-friendly mediator library for .NET 10, inspired by MediatR.
Provides request/response and notification handling with minimal dependencies.

## Features

- **Request/Response**: Send requests and receive responses via strongly-typed handlers.
- **Notifications**: Publish notifications to multiple handlers (sequential or parallel).
- **Pipeline behaviors**: Add middleware around request handling.
- **Dependency Injection**: Integration with `Microsoft.Extensions.DependencyInjection`.
- **Fast dispatch**: Per-request-type cached dispatch using compiled delegates (no per-call reflection).
- **.NET 10**: Modern C# and .NET features.

## Getting Started

### Installation

Build and pack the library:

```sh
dotnet pack -c Release
```

Find the `.nupkg` in `CCMediator/bin/Release/` and add it to your projects via a local NuGet source or publish to NuGet.org.

### Usage

#### Register with DI (no scanning / explicit registration)

This is the default and avoids reflection-based assembly scanning.

```csharp
using Microsoft.Extensions.DependencyInjection;
using CCMediator;

var services = new ServiceCollection();
services.AddCCMediator();

// Register handlers explicitly
services.AddTransient<IRequestHandler<Ping, string>, PingHandler>();
services.AddTransient<INotificationHandler<MyNotification>, MyNotificationHandler>();

var provider = services.BuildServiceProvider();
```

#### Register with DI (opt-in scanning)

If you prefer convenience over startup cost, you can explicitly enable scanning:

```csharp
using Microsoft.Extensions.DependencyInjection;
using CCMediator;

var services = new ServiceCollection();
services.AddCCMediatorWithScanning(typeof(Startup).Assembly);
var provider = services.BuildServiceProvider();
```

**Trade-offs**:
- Explicit registration: fastest startup, no reflection scan, most predictable.
- Scanning: fewer registrations to write, but uses reflection (`Assembly.GetTypes()` and `GetInterfaces()`) during startup.

#### Define a request and handler

```csharp
public record Ping(string Message) : IRequest<string>;

public class PingHandler : IRequestHandler<Ping, string>
{
    public Task<string> Handle(Ping request, CancellationToken cancellationToken)
        => Task.FromResult($"Pong: {request.Message}");
}
```

#### Send a request

```csharp
var mediator = provider.GetRequiredService<IMediator>();
var response = await mediator.Send(new Ping("Hello"));
```

#### Define and publish a notification

```csharp
public record MyNotification(string Info) : INotification;

public class MyNotificationHandler : INotificationHandler<MyNotification>
{
    public Task Handle(MyNotification notification, CancellationToken cancellationToken)
    {
        Console.WriteLine(notification.Info);
        return Task.CompletedTask;
    }
}

// Usage:
await mediator.Publish(new MyNotification("Something happened!"));
```

## Configuration

You can configure publishing behavior via `CCMediatorOptions`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using CCMediator;

var services = new ServiceCollection();

services.AddCCMediator(options =>
{
    options.NotificationPublishMode = NotificationPublishMode.Sequential;
    options.SequentialPublishErrorHandling = NotificationPublishErrorHandling.ContinueAndAggregateExceptions;

    // For parallel mode:
    // options.NotificationPublishMode = NotificationPublishMode.Parallel;
    // options.AggregateExceptionsInParallel = true;
});
```

## Pipeline behaviors

Register `IPipelineBehavior<TRequest,TResponse>` to wrap request handling.
Execution order matches DI registration order.

```csharp
services.AddTransient<IPipelineBehavior<Ping, string>, LoggingBehavior>();

public sealed class LoggingBehavior : IPipelineBehavior<Ping, string>
{
    public async Task<string> Handle(Ping request, Func<Task<string>> next, CancellationToken cancellationToken)
    {
        Console.WriteLine("Before");
        var result = await next();
        Console.WriteLine("After");
        return result;
    }
}
```

## Benchmarks

Benchmarks are included in the `BenchmarkSuite1` project.
To run them:

```sh
dotnet run -c Release -p BenchmarkSuite1
```

## License

MIT License. See [LICENSE.txt](LICENSE.txt) for details.
