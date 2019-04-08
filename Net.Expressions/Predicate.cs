using Net.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Net.Expressions
{
    enum LinkType
    {
        And,
        Or
    }
    class LinkItem<T>
    {
        public Expression<Func<T,bool>> Expression { get; set; }
        public LinkType LinkType { get; set; }
    }
    public class Predicate<T>
    {
        List<LinkItem<T>> Items = new List<LinkItem<T>>();
        private Predicate()
        {

        }
        public static Predicate<T> New(Expression<Func<T,bool>> expression)
        {
            var result = new Predicate<T>();
            result.Items.Add(new LinkItem<T> {Expression=expression });
            return result;
        }
        public Predicate<T> And(Expression<Func<T,bool>> exp)
        {
            this.Items.Add(new LinkItem<T>
            {
                Expression = exp,
                LinkType = LinkType.And
            });
            return this;
        }
        public Predicate<T> Or(Expression<Func<T, bool>> exp)
        {
            this.Items.Add(new LinkItem<T>
            {
                Expression = exp,
                LinkType = LinkType.Or
            });
            return this;
        }

        public Expression<Func<T,bool>> AsExpression()
        {
            if (this.Items.IsEmpty()) return _ => true;
            var parameter = Expression.Parameter(typeof(T));
            var first = this.Items.First();
            Expression result = first.Expression.Body
                .ReplaceParameter(first.Expression.Parameters[0], parameter);
            
            foreach(var item in this.Items.Skip(1))
            {
                switch (item.LinkType)
                {
                    case LinkType.And:
                        result = Expression.AndAlso(result, item.Expression.Body.ReplaceParameter(item.Expression.Parameters[0], parameter));
                        break;
                    case LinkType.Or:
                        result = Expression.OrElse(result, item.Expression.Body.ReplaceParameter(item.Expression.Parameters[0], parameter));
                        break;
                    default:
                        break;
                }
            }
            return Expression.Lambda(result, parameter) as Expression<Func<T,bool>>;
        }

        public Func<T, bool> AsFunc()
        {
            var exp = this.AsExpression();
            return exp.Compile() as Func<T, bool>;
        }
    }
}
