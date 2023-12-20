using Petl.Fluent;

namespace Petl.InMemory;

public static class InMemoryBuilderExtensions
{
    public static PetlServicesBuilder WithInMemoryDispatcher(this PetlServicesBuilder builder)
    {
        builder.WithResponseDispatcher<InMemoryResponseDispatcher>();

        return builder;
    }
}
