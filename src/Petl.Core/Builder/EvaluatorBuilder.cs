using Petl.Activators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Petl.Builder
{
    public sealed class EvaluatorBuilder : ICanAddEvalSet, IEvaluatorBuilder
    {
        private List<object> _evalSets = new List<object>();
        private IActivator _defaultActivator;

        public IEvaluator Build()
        {
            return new Evaluator(_defaultActivator, _evalSets);
        }

        public ICanAddSource<TInput, TOutput> Map<TInput, TOutput>(Func<TInput, bool> canHandleCallback = null)
        {
            var evalSet = new EvalSet<TInput, TOutput>()
            {
                CanHandle = canHandleCallback
            };

            _evalSets.Add(evalSet);

            return evalSet;
        }

        public ICanAddEvalSet WithActivator(IActivator activator)
        {
            _defaultActivator = activator;
            return this;
        }
    }
}
