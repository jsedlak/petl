using Petl.Targets;
using System;

namespace Petl.Builder
{
    public interface ICanAddTarget<TInput, TOutput>
    {
        ICanAddTarget<TInput, TOutput> AddTarget(ProgrammableTarget<TInput, TOutput> target, Action<ICanAddConverter> configure = null);
    }
}
