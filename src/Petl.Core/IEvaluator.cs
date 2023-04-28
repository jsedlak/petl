namespace Petl
{
    public interface IEvaluator
    {
        TOutput Evaluate<TInput, TOutput>(TInput input, EvalOptions? options = null);
    }
}
