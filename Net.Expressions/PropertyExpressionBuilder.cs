using Net.Extensions;
using Net.Reflection;
using System;
using System.Linq.Expressions;

namespace Net.Expressions
{
    public static class PropertyExpressionBuilder
    {
        
        public static Expression ToPropertyExpressionBody(this Expression referenceExp,string property,Type castType =null)
        {
            var type = referenceExp.Type;
            var typeInfo = type.GetInfo();
            var left = property.TrimThenBy(".");
            var right = property == left ? null : property.TrimLeftBy($"{left}.");
            var propInfo = typeInfo[left];
            if (propInfo == null)
                throw new Exception($"{typeInfo.Type.FullName} {left }Property not found");
            if (propInfo.Kind == TypeKind.Unknown) return null;
            if (!right.IsEmpty())
            {
                if (propInfo.Kind != TypeKind.Complex) return null;
                return Expression.Property(referenceExp, propInfo.Raw).ToPropertyExpressionBody(right, castType);
            }
            var propExp = Expression.Property(referenceExp, propInfo.Raw);
            if (castType != null && propExp.Type != castType)
                return Expression.Convert(propExp, castType);
            return propExp;
        }
        public static LambdaExpression ToPropertyExpression(this Type type, string property,Type castType =null)
        {
            var parameter = Expression.Parameter(type, type.Name.ToLowerInvariant());
            var body = parameter.ToPropertyExpressionBody(property,castType);
            if (body == null) return null;
            return Expression.Lambda(body, parameter);
        }

        public static string ToPropertyPath(this Expression expression)
        {
            var pathFinder = new PropertyPathFinderVisitor();
            pathFinder.Visit(expression);
            return pathFinder.AccumulatedPath;
        }
    }
}
