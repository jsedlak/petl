using Petl.Activators;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Petl
{
    public sealed class Evaluator : IEvaluator
    {
        private IActivator _defaultActivator;
        private IEnumerable<object> _evalSets;

        public Evaluator(IActivator defaultActivator, IEnumerable<object> evalSets)
        {
            _defaultActivator = defaultActivator;
            _evalSets = evalSets;
        }

        public TOutput Evaluate<TInput, TOutput>(TInput input, EvalOptions options = null)
        {
            options = options ?? new EvalOptions();
            options.Activator = options.Activator ?? _defaultActivator;

            // build the context
            var context = new EvalContext<TInput, TOutput>
            {
                Options = options ,
                Source = input
            };

            // bind the target
            options.Activator.Activate<TInput, TOutput>(context);

            // find the matching sets
            var matchingSets = _evalSets
                .Where(m => m is EvalSet<TInput, TOutput>)
                .Select(m => m as EvalSet<TInput, TOutput>);

            foreach (var set in matchingSets)
            {
                if (!set.CanHandle(input))
                {
                    continue;
                }

                set.Evaluate(context);
                return context.Target;
            }

            return context.Target;
        }
    }
}
