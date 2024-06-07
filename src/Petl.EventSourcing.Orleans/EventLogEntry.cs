using Orleans.Runtime;

namespace Petl.EventSourcing;

public record EventLogEntry<TData>(GrainId GrainId, TData Data, int Version);