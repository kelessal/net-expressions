using System;
using System.Linq.Expressions;

namespace Net.Expressions
{
    class LambdaTopMemberExtendVisitor : ExpressionVisitor
    {
        private bool IsTopMember = true;
        private Func<Expression, Expression> ExtendFn;
        public LambdaTopMemberExtendVisitor(Func<Expression,Expression> extendFn)
        {
            this.ExtendFn = extendFn;
        }

     
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            
            var newExpression = Visit(node.Body);
            return Expression.Lambda(newExpression, node.Parameters);
        }

        public override Expression Visit(Expression node)
        {
            if (!IsTopMember) return base.Visit(node);
            this.IsTopMember = false;
            var result = this.ExtendFn(node);
            return result;
        }

    }
}
