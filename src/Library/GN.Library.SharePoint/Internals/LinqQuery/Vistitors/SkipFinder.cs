using System;
using System.Linq.Expressions;

namespace GN.Library.SharePoint.Internals.LinqQuery.Vistitors
{
    internal class SkipFinder : ExpressionVisitor
    {
        private MethodCallExpression _expression;
        public int? Value { get; private set; }
        public MethodCallExpression Find(Expression expression)
        {
            Visit(expression);
            return _expression;
        }
        public int? FindValue(Expression expression)
        {
            Visit(expression);
            return this.Value;
        }
        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            if (expression.Method.Name == "Skip")
            {
                _expression = expression;
                if (_expression.Arguments.Count > 1 && _expression.Arguments[1] is ConstantExpression cs)
                {
                    this.Value = Convert.ToInt32(cs.Value);
                }
            }
            Visit(expression.Arguments[0]);

            return expression;
        }
    }
}
