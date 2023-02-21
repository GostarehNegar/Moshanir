using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace GN.Library.SharePoint.Internals.LinqQuery.Vistitors
{
    internal class InnermostWhereFinder : ExpressionVisitor
    {
        private MethodCallExpression innermostWhereExpression;

        public MethodCallExpression GetInnermostWhere(Expression expression)
        {
            Visit(expression);
            return innermostWhereExpression;
        }

        //[return: NotNullIfNotNull("node")]
        public override Expression Visit(Expression node)
        {
            if (node.NodeType == ExpressionType.AndAlso)
            {

            }
            return base.Visit(node);
        }
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.AndAlso)
            {

            }
            return base.VisitBinary(node);
        }
        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            if (expression.Method.Name == "Where")
                innermostWhereExpression = expression;
                //innermostWhereExpression = expression;
            Visit(expression.Arguments[0]);

            return expression;
        }
    }

    
}
