using Net.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Net.Expressions
{
    public class Queryable<T>
    {
        Expression<Func<T, bool>> Exp;
        private Queryable()
        {

        }
        public static Queryable<T> New(Expression<Func<T,bool>> expression)
        {
            var result = new Queryable<T>();
            result.Exp = expression;
            return result;
        }
        public Queryable<T> And(Expression<Func<T,bool>> exp)
        {
            var result = new Queryable<T>();
            var parameter = Expression.Parameter(typeof(T));
            var body= Expression.AndAlso(this.Exp.Body.ReplaceParameter(this.Exp.Parameters[0], parameter),
                exp.Body.ReplaceParameter(exp.Parameters[0], parameter));
            result.Exp = Expression.Lambda<Func<T, bool>>(body, parameter);
            return result;
        }
        public Queryable<T> Or(Expression<Func<T, bool>> exp)
        {
            var result = new Queryable<T>();
            var parameter = Expression.Parameter(typeof(T));
            var body = Expression.OrElse(this.Exp.Body.ReplaceParameter(this.Exp.Parameters[0], parameter),
                exp.Body.ReplaceParameter(exp.Parameters[0], parameter));
            result.Exp = Expression.Lambda<Func<T, bool>>(body, parameter);
            return result;
        }

        public Expression<Func<T,bool>> AsExpression()
        {
            return this.Exp;
        }

        public Func<T, bool> AsFunc()
        {
            return this.Exp.Compile();
        }
    }
}
