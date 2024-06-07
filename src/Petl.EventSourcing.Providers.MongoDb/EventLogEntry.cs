namespace Petl.EventSourcing.Providers;

public record EventLogEntry<TData>(Guid Id, string GrainId, TData Data, int Version);