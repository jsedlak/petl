<img src="design/logo-dark.png" alt="Logo Dark" width="400">
<img src="design/logo-light.png" alt="Logo Light" width="400">

# PETL

Petl aims to be both a Programmable ETL (hence the name) as well as house a strong Mediator Pattern implementation.

## Mediator

The Petl Mediator works with `Request` and `Response` objects, which have a direct correlation to `Command` and `Event`. Each Response may be dispatched through any number of methods such as in memory, through Channels or via ServiceBus.

Add Petl to your Dependency Injection:

```
services.AddPetl(builder => builder
    .WithInMemoryDispatcher()
    .RegisterRequestHandlers(typeof(TestRequestHandler).Assembly)
    .RegisterResponseHandlers(typeof(AttributedResponseHandler).Assembly)
)
```

Inject the `IRequestProcessor` service and use it to process a request.

```
var requestProcessor = provider.GetRequiredService<IRequestProcessor>();

await requestProcessor.ProcessAsync<TestRequest, TestResult>(
    new TestRequest { Count = 10 }, 
    CancellationToken.None
);
```

In the background, responses are dispatched via any available registered services.