using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Petl.Reflection
{
    public static class ExpressionExtensions
    {
        public static Tuple<object, PropertyInfo> GetPropertyInfo<TEntity>(
            this Expression<Func<TEntity, object>> entityExpression,
            TEntity entity)
        {
            var members = new List<MemberExpression>();
            var expression = entityExpression.Body;

            while (expression != null)
            {
                var memberExpression = (MemberExpression)null;

                if (expression.NodeType == ExpressionType.Convert)
                {
                    memberExpression = ((UnaryExpression)expression).Operand as MemberExpression;
                }
                else if (expression.NodeType == ExpressionType.MemberAccess)
                {
                    memberExpression = expression as MemberExpression;
                }

                if (memberExpression == null)
                {
                    break;
                }

                members.Add(memberExpression);
                expression = memberExpression.Expression;
            }

            // the expression tree comes in linearly backwards (from right to left)
            members.Reverse();

            object current = entity;
            var currentProperty = (PropertyInfo)null;

            for (var i = 0; i < members.Count; i++)
            {
                var exp = members[i];

                currentProperty = current.GetType().GetProperty(exp.Member.Name);

                // the last expression we actually want to just return
                // blah.child.property => { current = child, property = property }
                if (i == members.Count - 1)
                {
                    break;
                }

                // otherwise, we need to try and go deeper
                var currentValue = currentProperty.GetValue(current);

                // do we need to create the instance so that we may go deeper?
                if (currentValue == null)
                {
                    currentValue = Activator.CreateInstance(currentProperty.PropertyType);
                    currentProperty.SetValue(current, currentValue);
                }

                current = currentValue;
            }

            return new Tuple<object, PropertyInfo>(current, currentProperty);
        }
    }
}
