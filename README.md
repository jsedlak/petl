# Petl

Petl started off as a concept of a "Programmable ETL", using a fluent interface to map inputs to outputs, in the hopes of solving the use case of "I have Object A, but I need to store Object B" with testability and repetition.

It has morphed over the years, and while that concept will probably be revived at some point, the library has moved on.

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

# Petl Event Sourcing

An implementation of the mechanisms related to Event Sourcing. This is based on my interest in Orleans, though I may entertain the idea of other platforms, such as dapr.

The `Petl.EventSourcing` library provides the base interfaces for implementing contracts related to Event Sourcing, such that developers may create provider specific implementations, such as `Petl.EventSourcing.Providers.MongoDb`

The `Petl.EventSourcing.Orleans` library offers an `EventSourcedGrain` that supports (will support) common scenarios related to Event Sourcing. 

```csharp
public sealed class AccountGrain : 
    EventSourcedGrain<BankAccount, BankAccountBaseEvent>,
    IAccountGrain
{
    public ValueTask<bool> Withdraw(double amount)
    {
        if (amount > State.Balance)
        {
            return ValueTask.FromResult(false);
        }

        Raise(new AmountWithdrawn { Amount = amount });

        return ValueTask.FromResult(true);
    }
}
```

Event and State are managed through a single interface, `IEventLog<TView, TEntry>`. The goal is to support the possibility to truncate the event log by snapshotting the view.

```csharp
public interface IEventLog<TView, TEntry>
    where TView : class, new()
    where TEntry : class
{
    // ...
}
```

An instance of the event log is associated with every `EventSourcedGrain`, providing a window into the underlying data. The event log may load a snapshot and/or events to hydrate and maintain the state of the grain's data.

A MongoDB implementation is being provided with Petl.

```csharp
public class MongoDbEventLog<TView, TEvent> : IEventLog<TView, TEvent>
    where TView : class, new()
    where TEvent : class
{
    // ...
}
```

To use the MongoDB provider, reference the appropriate libraries and configure the services in your `Program.cs`

```csharp
await Host.CreateDefaultBuilder(args)
    .UseOrleans(silo =>
    {
        silo
            .ConfigureServices((services) =>
            {
                // configure the grain storage serializer as the event serializer
                services.AddOrleansEventSerializer();
                
                // add the mongo event log
                services.AddMongoEventSourcing("your-database");
            })
            .UseLocalhostClustering()
            .UseMongoDBClient(sp =>
                MongoClientSettings.FromConnectionString("mongodb://localhost:27017/")
            )
            .UseDashboard();
    })
    .RunConsoleAsync();
```

