namespace Petl.EventSourcing;

public interface IEventLogFactory
{
    IEventLog<TView, TEntry> Create<TView, TEntry> (Type grainType, string viewId)
        where TView : class, new()
        where TEntry : class;
}