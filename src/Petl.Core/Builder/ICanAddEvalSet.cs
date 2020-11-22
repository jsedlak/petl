using Petl.Activators;
using System;

namespace Petl.Builder
{
    public interface ICanAddEvalSet
    {
        ICanAddEvalSet WithActivator(IActivator activator);

        ICanAddSource<TInput, TOutput> Map<TInput, TOutput>(Func<TInput, bool> canHandleCallback = null);
    }
}
