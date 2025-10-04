# SimpleMediator

A lightweight, dependency-injection-friendly mediator library for .NET 10, inspired by MediatR.
Provides simple request/response and notification handling with minimal dependencies.

## Features

- **Request/Response**: Send requests and receive responses via strongly-typed handlers.
- **Notifications**: Publish notifications to multiple handlers.
- **Dependency Injection**: Seamless integration with `Microsoft.Extensions.DependencyInjection`.
- **No Reflection at Runtime**: Optimized for performance using compiled delegates.
- **.NET 10**: Modern C# and .NET features.

## Getting Started

### Installation

Build and pack the library:

```sh
dotnet pack -c Release
```

Find the `.nupkg` in `SimpleMediator/bin/Release/` and add it to your projects via a local NuGet source or publish to NuGet.org.

### Usage

1. **Register with DI**

```csharp
using Microsoft.Extensions.DependencyInjection;
using SimpleMediator;

var services = new ServiceCollection();
services.AddSimpleMediator(typeof(Startup).Assembly); // Scan your assemblies for handlers
var provider = services.BuildServiceProvider();
```

2. **Define a Request and Handler**

```csharp
public record Ping(string Message) : IRequest<string>;

public class PingHandler : IRequestHandler<Ping, string>
{
    public Task<string> Handle(Ping request, CancellationToken cancellationToken)
        => Task.FromResult($"Pong: {request.Message}");
}
```

3. **Send a Request**

```csharp
var mediator = provider.GetRequiredService<IMediator>();
string response = await mediator.Send(new Ping("Hello"));
```

4. **Publish a Notification**

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

## Benchmarks

Benchmarks are included in the `BenchmarkSuite1` project.
To run them:

```sh
dotnet run -c Release -p BenchmarkSuite1
```

## License

MIT License. See [LICENSE.txt](LICENSE.txt) for details.
