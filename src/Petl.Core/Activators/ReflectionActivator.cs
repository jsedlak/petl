using System;

namespace Petl.Activators
{
    public class ReflectionActivator : IActivator
    {
        public void Activate<TInput, TOutput>(EvalContext<TInput, TOutput> context)
        {
            context.Target = Activator.CreateInstance<TOutput>();
        }
    }
}
