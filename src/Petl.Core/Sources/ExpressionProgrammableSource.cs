using Petl.Targets;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Petl.Sources
{
    public sealed class ExpressionProgrammableSource<TInput, TOutput> : ProgrammableSource<TInput, TOutput>
    {
        private readonly Expression<Func<TInput, object>> _expression;

        public ExpressionProgrammableSource(Expression<Func<TInput, object>> expression, IEnumerable<ProgrammableTarget<TInput, TOutput>>? targets = null)
            : base(targets)
        {
            _expression = expression;
        }

        protected override object GetValue(EvalContext<TInput, TOutput> context)
        {
            var func = _expression.Compile();
            return func(context.Source);
        }
    }
}
