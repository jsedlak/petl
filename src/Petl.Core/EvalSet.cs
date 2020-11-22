using Petl.Builder;
using Petl.Sources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Petl
{
    public sealed class EvalSet<TInput, TOutput> : ICanAddSource<TInput, TOutput>
    {
        private IEnumerable<ProgrammableSource<TInput, TOutput>> _sources;

        public EvalSet()
        {

        }

        public EvalSet(IEnumerable<ProgrammableSource<TInput, TOutput>> sources)
        {
            _sources = sources;
        }

        public void Evaluate(EvalContext<TInput, TOutput> context)
        {
            foreach(var source in _sources)
            {
                source.Eval(context);
            }
        }

        public ICanAddTarget<TInput, TOutput> AddSource(ProgrammableSource<TInput, TOutput> source)
        {
            _sources = _sources == null ? new[] { source } : _sources.Union(new[] { source });
            return source;
        }

        public Func<TInput, bool> CanHandle { get; set; }
    }
}
