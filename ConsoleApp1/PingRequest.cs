    public class PingRequest : IRequest<string>
    {
        public string Message { get; set; }
    }

    public class PingHandler : IRequestHandler<PingRequest, string>
    {
        public string Handle(PingRequest request)
        {
            return $"Pong: {request.Message}";
        }
    }
    