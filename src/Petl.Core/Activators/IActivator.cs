namespace Petl.Activators
{
    public interface IActivator
    {
        void Activate<TInput, TOutput>(EvalContext<TInput, TOutput> context);
    }
}
