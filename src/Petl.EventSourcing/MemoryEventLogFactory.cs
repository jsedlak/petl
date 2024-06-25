namespace Petl.EventSourcing;

public sealed class MemoryEventLogFactory : IEventLogFactory
{
    public IEventLog<TView, TEntry> Create<TView, TEntry>(Type grainType, string viewId) where TView : class, new() where TEntry : class
    {
        return new MemoryEventLog<TView, TEntry>();
    }
}