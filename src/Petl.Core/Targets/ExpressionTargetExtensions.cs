using Petl.Builder;
using System;
using System.Linq.Expressions;

namespace Petl.Targets
{
    public static class ExpressionTargetExtensions
    {
        public static ICanAddTarget<TInput, TOutput> ToExpression<TInput, TOutput>(
            this ICanAddTarget<TInput, TOutput> source,
            Expression<Func<TOutput, object>> expression,
            Action<ICanAddConverter>? configure = null)
        {
            var target = new ExpressionProgrammableTarget<TInput, TOutput>(expression);
            return source.AddTarget(target, configure);
        }
    }
}
