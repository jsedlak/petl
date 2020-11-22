namespace Petl
{
    public class EvalContext<TInput, TOutput>
    {
        public EvalOptions Options { get; set; }

        public TInput Source { get; set; }

        public TOutput Target { get; set; }
    }
}
