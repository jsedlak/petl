# Petl.Mediator

The Mediator project offers my own approach to the pattern, with support for event handler error capturing. Instead of Commands/Events, or Commands/Notifications, it operates on the basis of Request/Response.

```csharp
// Add petl with in memory dispatcher, registering handlers
services.AddPetl(builder => builder
    .WithInMemoryDispatcher()
    .RegisterRequestHandlers(typeof(TestRequestHandler).Assembly)
    .RegisterResponseHandlers(typeof(AttributedResponseHandler).Assembly)
);
```

Response Dispatchers may come in many flavors, and Response Handlers may associate themselves with a particular Dispatcher. This provides the ability to dispatch responses both locally and through a message bus without worrying about how the responses are processed.

Unattributed response handlers are handled by the default (TODO) dispatcher.

```csharp
[ResponseDispatcher(typeof(InMemoryResponseDispatcher))]
public sealed class AttributedResponseHandler : IResponseHandler<CountIncreasedResponse>
{
    // ...
}
```