using Net.Extensions;
using Net.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Net.Expressions
{
    public static class CompareExpressionBuilder
    {
        static MethodInfo miStartsWith = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
        static MethodInfo miContains = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        static MethodInfo miEndsWith = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
        static MethodInfo miToLower = typeof(string).GetMethod("ToLower", new Type[] { });
        static MethodInfo miStringContains = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        static MethodInfo miListStringContains = typeof(List<string>).GetMethod("Contains", new[] { typeof(string) });
        static MethodInfo miToString = typeof(object).GetMethod("ToString", new Type[] { });

        static HashSet<CompareType> StringCompareTypes = new HashSet<CompareType>(new[]
        {
            CompareType.Contains,
            CompareType.StartsWith,
            CompareType.EndsWith
        });
        public static Expression ToCompareExpressionBody(this Expression refExpression, string property, CompareType compareType, object[] values, params string[] langs)
        {
            var typeInfo = refExpression.Type.GetInfo();
            var left = property.TrimThenBy(".");
            var right = property == left ? null : property.TrimLeftBy($"{left}.");
            var propInfo = typeInfo[left.ToUpperFirstLetter()];
            Expression propExp = null;
            if (propInfo == null)
            {
                propInfo = typeInfo.ElementTypeInfo[left.ToUpperFirstLetter()];

                if (propInfo == null) return null;

                var miContainQueryable = typeof(Enumerable).FindMethod("Contains", p => p.GetParameters().Length == 2, typeof(object));

                var paramExp = Expression.Parameter(typeInfo.ElementTypeInfo.Type, "p");
                propExp = Expression.Property(paramExp, propInfo.Raw);

                var constantExp = Expression.Constant(values);
                var valuesContainsExp = Expression.Call(miContainQueryable, constantExp, propExp);
                return Expression.Lambda(valuesContainsExp, paramExp);
            }
            else
            {
                if (propInfo.Kind == TypeKind.Unknown && propInfo.Type != typeof(Dictionary<string, string>)) return null;
                propExp = Expression.Property(refExpression, propInfo.Raw);
            }
            if (right.IsEmpty())
            {
                var canMakeExpression = propInfo.Kind == TypeKind.Primitive || propInfo.IsPrimitiveCollection || propInfo.Type == typeof(Dictionary<string, string>);
                if (!canMakeExpression) return null;
                if (propInfo.Kind == TypeKind.Primitive)
                    return CreatePrimitiveCompareExpression(propExp, compareType, values);
                var collectionParameter = Expression.Parameter(propInfo.ElementTypeInfo.Type, propInfo.ElementTypeInfo.Type.Name.ToLowerInvariant());
                var primitiveCollCompExp = CreatePrimitiveCompareExpression(collectionParameter, compareType, values);
                var primtiveCollLambda = Expression.Lambda(primitiveCollCompExp, collectionParameter);
                var miPrimitiveAnyQueryable = typeof(Enumerable).FindMethod("Any", p => p.GetParameters().Length == 2, propInfo.ElementTypeInfo.Type);
                return Expression.Call(miPrimitiveAnyQueryable, propExp, primtiveCollLambda);
            }

            if (propInfo.Kind == TypeKind.Primitive) return null;
            if (propInfo.IsPrimitiveCollection) return null;
            var subType = propInfo.Kind == TypeKind.Collection ?
                propInfo.ElementTypeInfo.Type : propInfo.Type;
            var subExpression = propExp.ToCompareExpressionBody(right, compareType, values);
            if (subExpression == null) return null;
            if (propInfo.Kind == TypeKind.Complex)
                return subExpression;
            var miAnyQueryable = typeof(Enumerable).FindMethod("Any", p => p.GetParameters().Length == 2, subType);
            var anyExp = Expression.Call(miAnyQueryable, propExp, subExpression);

            if(compareType == CompareType.NotEqual)
                return Expression.Equal(anyExp, Expression.Constant(false));

            return anyExp;
        }
        public static LambdaExpression ToCompareExpression(this Type type, string property, CompareType compareType, object[] values)
        {
            var parameter = Expression.Parameter(type, type.Name.ToLowerInvariant());
            var body = parameter.ToCompareExpressionBody(property, compareType, values);
            if (body == null) return null;
            return Expression.Lambda(body, parameter);
        }

        static Expression CreatePrimitiveCompareExpression(Expression reference, CompareType compareType, object[] values)
        {

            var isStringCompare = StringCompareTypes.Contains(compareType);
            var convertType = isStringCompare ? typeof(string) : reference.Type;
            values = values.Select(p => p.ChangeType(convertType)).ToArray();
            if (convertType == typeof(string))
            {
                values = values.Where(p => !p.IsNull()).Select(p => (p as string).ToLower()).ToArray();
                if (reference.Type != typeof(string))
                {
                    reference = Expression.Call(reference, miToString);
                }
                reference = Expression.Call(reference, miToLower);
            }
            Expression result = null;


            for (var i = 0; i < values.Length; i++)
            {
                Expression filterExp = null;
                switch (compareType)
                {
                    case CompareType.Contains:
                        filterExp = Expression.Call(reference, miStringContains, Expression.Constant(values[i]));
                        break;
                    case CompareType.EndsWith:
                        filterExp = Expression.Call(reference, miEndsWith, Expression.Constant(values[i]));
                        break;
                    case CompareType.Equal:
                        filterExp = Expression.Equal(reference, Expression.Constant(values[i]));
                        break;
                    case CompareType.StartsWith:
                        filterExp = Expression.Call(reference, miStartsWith, Expression.Constant(values[i]));
                        break;
                    case CompareType.GreaterThanOrEqual:
                        filterExp = Expression.GreaterThanOrEqual(reference, Expression.Constant(values[i]));
                        break;
                    case CompareType.GreaterThan:
                        filterExp = Expression.GreaterThan(reference, Expression.Constant(values[i]));
                        break;
                    case CompareType.LessThanOrEqual:
                        filterExp = Expression.LessThanOrEqual(reference, Expression.Constant(values[i]));
                        break;
                    case CompareType.LowerThan:
                        filterExp = Expression.LessThan(reference, Expression.Constant(values[i]));
                        break;
                    case CompareType.NotEqual:
                        filterExp = Expression.NotEqual(reference, Expression.Constant(values[i]));
                        break;
                }
                result =
                    result == null ?
                        filterExp : compareType == CompareType.NotEqual ?
                            Expression.AndAlso(result, filterExp) : Expression.OrElse(result, filterExp);
            }

            return result;
        }
    }
}
