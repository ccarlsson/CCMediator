using CCMediator.Abstractions;

namespace CCMediator.Tests;

// Test request and notification classes
public class TestRequest : IRequest<string>
{
    public required string Message { get; set; }
}



// Test notification and handler
public class TestNotification : INotification { }


public class TestRequestHandler : IRequestHandler<TestRequest, string>
{
    public Task<string> Handle(TestRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult($"Handled: {request.Message}");
    }
}


public class TestNotificationHandler : INotificationHandler<TestNotification>
{
    public Task Handle(TestNotification notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
