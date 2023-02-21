using System.Linq.Expressions;

namespace GN.Library.SharePoint.Internals.LinqQuery.Vistitors
{

    internal class ConstantsEvaluator : ExpressionVisitor
    {
        // https://stackoverflow.com/questions/44127341/how-do-expression-trees-allow-consumers-to-evaluate-variables
        //public Filter Filter { get; private set; }
        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value != null)
            {
                var ttt = node.Value.GetType();
                

            }
            return base.VisitConstant(node);
        }
        
        public void Evaluate(Expression expression)
        {
            Visit(expression);
        }
    }

}
