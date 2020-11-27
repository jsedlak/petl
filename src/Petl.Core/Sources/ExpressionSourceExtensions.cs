using Petl.Builder;
using System;
using System.Linq.Expressions;

namespace Petl.Sources
{
    public static class ExpressionSourceExtensions
    {
        public static ICanAddTarget<TInput, TOutput> FromExpression<TInput, TOutput>(
            this ICanAddSource<TInput, TOutput> evalSet,
            Expression<Func<TInput, object>> expression)
        {
            var source = new ExpressionProgrammableSource<TInput, TOutput>(expression);
            evalSet.AddSource(source);
            return source;
        }
    }
}
