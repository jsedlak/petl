using Petl.Builder;
using Petl.Converters;
using Petl.Targets;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Petl.Sources
{
    public abstract class ProgrammableSource<TInput, TOutput> : ICanAddTarget<TInput, TOutput>
    {
        private IEnumerable<ProgrammableTarget<TInput, TOutput>> _targets;

        public ProgrammableSource(IEnumerable<ProgrammableTarget<TInput, TOutput>> targets = null)
        {
            _targets = targets;
        }

        protected abstract object GetValue(EvalContext<TInput, TOutput> context);

        public void Eval(EvalContext<TInput, TOutput> context)
        {
            var value = GetValue(context);
            foreach (var target in _targets)
            {
                target.Eval(context, value);
            }
        }

        public ICanAddTarget<TInput, TOutput> AddTarget(ProgrammableTarget<TInput, TOutput> target, System.Action<ICanAddConverter> configure = null)
        {
            if(configure != null)
            {
                configure(target);
            }

            _targets = _targets == null ? new[] { target } : _targets.Union(new[] { target });
            return this;
        }
    }
}
