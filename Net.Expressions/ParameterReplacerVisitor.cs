using System.Linq;
using System.Linq.Expressions;

namespace Net.Expressions
{
    class ParameterReplacerVisitor : ExpressionVisitor
    {
        private ParameterExpression _source;
        private Expression _target;

        public ParameterReplacerVisitor
                (ParameterExpression source, Expression target)
        {
            _source = source;
            _target = target;
        }


        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            // Leave all parameters alone except the one we want to replace.
            var parameters = node.Parameters
                                 .Where(p => p != _source).ToList();
            var newExpression = Visit(node.Body);
            return Expression.Lambda(newExpression, parameters);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            // Replace the source with the target, visit other params as usual.
            return node == _source ? _target : base.VisitParameter(node);
        }
       
    }
}
