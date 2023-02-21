using System.Linq.Expressions;

namespace GN.Library.SharePoint.Internals.LinqQuery.Vistitors
{
    internal class OrderByFinder : ExpressionVisitor
    {
        private MethodCallExpression _expression;
        public string Value { get; private set; }
        public MethodCallExpression Find(Expression expression)
        {
            Visit(expression);
            return _expression;
        }
        public string FindValue(Expression expression)
        {
            Visit(expression);
            return this.Value;
        }
        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            if (expression.Method.Name == "OrderBy")
            {
                _expression = expression;
                if (_expression.Arguments.Count == 2
                    && _expression.Arguments[1] is UnaryExpression _e 
                    && _e.Operand is LambdaExpression _l
                    && _l.Body is MemberExpression _m
                    )
                {
                   this.Value = _m.Member.Name;
                }
                else
                {
                    throw new System.Exception(
                        $"Invalid or too complex OrderBy expression. We only can support simple expressions like OrderBy(x=>x.Name), and nothing more complex.");
                }
            }
            Visit(expression.Arguments[0]);

            return expression;
        }
    }

    internal class OrderByDescendingFinder : ExpressionVisitor
    {
        private MethodCallExpression _expression;
        public string Value { get; private set; }
        public MethodCallExpression Find(Expression expression)
        {
            Visit(expression);
            return _expression;
        }
        public string FindValue(Expression expression)
        {
            Visit(expression);
            return this.Value;
        }
        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            if (expression.Method.Name == "OrderByDescending")
            {
                _expression = expression;
                if (_expression.Arguments.Count == 2
                    && _expression.Arguments[1] is UnaryExpression _e
                    && _e.Operand is LambdaExpression _l
                    && _l.Body is MemberExpression _m
                    )
                {
                    this.Value = _m.Member.Name;
                }
                else
                {
                    throw new System.Exception(
                        $"Invalid or too complex OrderBy expression. We only can support simple expressions like OrderBy(x=>x.Name), and nothing more complex.");
                }
            }
            Visit(expression.Arguments[0]);

            return expression;
        }
    }
}
