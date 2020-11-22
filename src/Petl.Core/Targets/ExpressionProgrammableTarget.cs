using Petl.Reflection;
using System;
using System.Linq.Expressions;

namespace Petl.Targets
{
    public sealed class ExpressionProgrammableTarget<TInput, TOutput> : ProgrammableTarget<TInput, TOutput>
    {
        private readonly Expression<Func<TOutput, object>> _expression;

        public ExpressionProgrammableTarget(Expression<Func<TOutput, object>> expression)
        {
            _expression = expression;
        }

        protected override void SetValue(EvalContext<TInput, TOutput> context, object value)
        {
            var expressionInfo = _expression.GetPropertyInfo(context.Target);
            expressionInfo.Item2.SetValue(expressionInfo.Item1, value);
        }
    }
}
