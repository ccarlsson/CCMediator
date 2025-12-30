# Software Requirements Specification  
## SimpleMediator — A Lightweight Mediator Library for .NET 10

## 1. Introduction

### 1.1 Purpose  
The purpose of **SimpleMediator** is to provide a minimal, easy‑to‑understand alternative to MediatR for .NET 10 applications. It emphasizes simplicity, predictability, and transparent message flow, making it ideal for teaching and small‑to‑medium projects.

### 1.2 Scope  
SimpleMediator will support:

- Request/Response messaging  
- Notification broadcasting  
- Pipeline behaviors  
- Dependency injection integration  
- Strongly typed generics  

SimpleMediator will *not* include:

- Cross‑process messaging  
- Retry policies  
- Built‑in logging or metrics  
- Code generation  
- Reflection‑heavy scanning by default  

### 1.3 Definitions  
- **Request** — A message expecting a single response.  
- **Notification** — A message broadcast to multiple handlers.  
- **Handler** — A class implementing a request or notification interface.  
- **Pipeline behavior** — A middleware component wrapping request execution.

---

## 2. Overall Description

### 2.1 Product Perspective  
SimpleMediator is a library consumed by .NET 10 applications. It integrates with the built‑in DI container and exposes a single `IMediator` interface.

### 2.2 Product Features  
- Send requests  
- Publish notifications  
- Register handlers  
- Pipeline behaviors  
- Cancellation support  

### 2.3 User Classes  
- Application developers  
- Students learning architecture  
- Library authors extending the mediator  

---

## 3. Functional Requirements

### 3.1 Request/Response Handling  
- Support `IRequest<TResponse>`  
- Exactly one handler per request  
- Throw on multiple handlers  
- Resolve handlers via DI  
- Async execution  

### 3.2 Notification Handling  
- Support `INotification`  
- Multiple handlers allowed  
- Sequential or parallel execution  
- Configurable error handling  

### 3.3 Pipeline Behaviors  
- Zero or more behaviors  
- Ordered execution  
- Short‑circuit capability  
- Request/response modification  

### 3.4 Dependency Injection Integration  
- Extension method: `AddSimpleMediator`  
- Automatic handler registration  
- Manual registration supported  

### 3.5 Error Handling  
- Throw `HandlerNotFoundException`  
- Throw `MultipleHandlersFoundException`  
- Wrap internal errors in `MediatorException`  

---

## 4. Non‑Functional Requirements

### 4.1 Performance  
- Minimal overhead for request dispatch  
- Linear scaling for notifications  
- Predictable pipeline behavior cost  

### 4.2 Reliability  
- Guaranteed handler order when sequential  
- Full cancellation token support  

### 4.3 Usability  
- Minimal, intuitive API  
- Avoid hidden magic  

### 4.4 Maintainability  
- Under 500 lines of code  
- Modular and testable architecture  
- XML documentation for public APIs  

### 4.5 Compatibility  
- Target .NET 10  
- Support C# 13 features  

---

## 5. System Architecture

### 5.1 Core Interfaces  
- `IMediator`  
- `IRequest<TResponse>`  
- `IRequestHandler<TRequest,TResponse>`  
- `INotification`  
- `INotificationHandler<TNotification>`  
- `IPipelineBehavior<TRequest,TResponse>`  

### 5.2 Execution Flow  

#### Send(request)
1. Resolve handler  
2. Build pipeline chain  
3. Execute behaviors  
4. Execute handler  
5. Return response  

#### Publish(notification)
1. Resolve all handlers  
2. Execute sequentially or in parallel  
3. Aggregate exceptions if configured  

---

## 6. Constraints  
- Avoid heavy reflection unless enabled  
- No external dependencies  
- Framework‑agnostic except DI  

---

## 7. Future Enhancements  
- Source generator for registration  
- OpenTelemetry instrumentation  
- FluentValidation integration  
- Request caching behavior  
