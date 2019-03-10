using System.Linq.Expressions;

namespace Net.Expressions
{
    class ConstantReplacerVisitor : ExpressionVisitor
    {
        private ConstantExpression _source;
        private Expression _target;

        public ConstantReplacerVisitor (ConstantExpression source, Expression target)
        {
            _source = source;
            _target = target;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            return node == _source ? _target : base.VisitConstant(node);
        }

    }
}
