using Petl.Sources;

namespace Petl.Builder
{
    public interface ICanAddSource<TInput, TOutput>
    {
        ICanAddTarget<TInput, TOutput> AddSource(ProgrammableSource<TInput, TOutput> source);
    }
}
