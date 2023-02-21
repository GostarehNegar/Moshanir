using System.Linq.Expressions;

namespace GN.Library.SharePoint.Internals.LinqQuery.Vistitors
{
    internal class WhereEvaluator: ExpressionVisitor
    {
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.AndAlso)
            {

            }
            return base.VisitBinary(node);
            return base.VisitBinary(node);
        }
        public void GetInnermostWhere(Expression expression)
        {
            Visit(expression);
            
        }
    }

}
