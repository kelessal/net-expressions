using Net.Extensions;
using System.Linq.Expressions;

namespace Net.Expressions
{
    public class PropertyPathFinderVisitor: ExpressionVisitor
    {
        public string AccumulatedPath { get; private set; }
        private void AddToAccumulatedPath(string name)
        {
            AccumulatedPath = AccumulatedPath.IsEmpty() ? name : $"{AccumulatedPath}.{name}";
        }
        public override Expression Visit(Expression node)
        {
            if(node.NodeType==ExpressionType.MemberAccess)
            {
                this.AddToAccumulatedPath((node as MemberExpression).Member.Name);
            }
            return base.Visit(node);
        }
    }
}
