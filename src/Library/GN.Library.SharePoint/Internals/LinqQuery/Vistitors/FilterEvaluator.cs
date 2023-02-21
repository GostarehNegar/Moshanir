using System;
using System.Linq.Expressions;
using System.Reflection;
namespace GN.Library.SharePoint.Internals.LinqQuery.Vistitors
{
    internal class FilterEvaluator : ExpressionVisitor
    {
        // https://stackoverflow.com/questions/44127341/how-do-expression-trees-allow-consumers-to-evaluate-variables
        public Filter Filter { get; private set; }
        //protected override Expression VisitParameter(ParameterExpression node)
        //{
        //    return base.VisitParameter(node);
        //}
        //protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
        //{
        //    return base.VisitRuntimeVariables(node);
        //}
        internal Tuple<Filter, Filter> GetPropValue(BinaryExpression node, bool Throw = true)
        {
            if (node.Left is MemberExpression _left && node.Right is Expression _right)
            {
                return new Tuple<Filter, Filter>(Filter.Prop(_left.Member.Name), Filter.Val(_Evaluate(_right)));
            }
            return null;
            if (node.Left is MemberExpression member && node.Right is ConstantExpression val)
            {
                return new Tuple<Filter, Filter>(Filter.Prop(member.Member.Name), Filter.Val(val.Value));
            }
            else if (node.Left is MemberExpression _member && node.Right is MemberExpression _val && _val.Member is FieldInfo field && _val.Expression is ConstantExpression c)
            {
                return new Tuple<Filter, Filter>(Filter.Prop(_member.Member.Name), Filter.Val(field.GetValue(c.Value)));
            }
            if (Throw)
            {
                throw new Exception($"Inavlid or Complex Expression. {node.ToString()}.");
            }
            return null;
        }
        
        private object _Evaluate(Expression exp)
        {
            if (1 == 0)
            {
                switch (exp)
                {
                    case ConstantExpression c:
                        return c.Value;
                    case MemberExpression m:
                        switch (m.Member)
                        {
                            case FieldInfo field:
                                if (m.Expression is ConstantExpression c1)
                                {
                                    return field.GetValue(c1.Value);
                                }
                                break;
                            default:
                                break;
                        }
                        break;

                }
            }
            return Expression.Lambda<Func<object>>(Expression.Convert(exp, typeof(object))).Compile()(); ;

        }
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (this.Filter == null && node.Method?.Name == "Contains" && node.Object is MemberExpression exp &&
                node.Arguments != null && node.Arguments.Count > 0 && node.Arguments[0] is Expression exp2)
            {
                this.Filter = Filter.Contains(Filter.Prop(exp.Member.Name), Filter.Val(_Evaluate(exp2)));
            }
            return base.VisitMethodCall(node);
        }
        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.And:
                    this.Filter = Filter.And(
                            new FilterEvaluator().Evaluate(node.Left),
                            new FilterEvaluator().Evaluate(node.Right));
                    break;
                case ExpressionType.OrElse:
                case ExpressionType.Or:
                    this.Filter = Filter.Or(
                            new FilterEvaluator().Evaluate(node.Left),
                            new FilterEvaluator().Evaluate(node.Right));
                    break;
                default:
                    // throw new Exception("Invalid Where Clause");

                    break;
            }
            /// If filter is not null, we have already handled the node. 
            /// 
            /// Actually we should have stopped further visiting
            /// when above statements has set the filer, for instance 
            /// in case of 'and'. But unfortunately I couldnot figure
            /// it out. 
            /// So we simply check the filter here.

            if (this.Filter == null)
            {
                Tuple<Filter, Filter> propVal = null;
                switch (node.NodeType)
                {
                    case ExpressionType.GreaterThan:
                        propVal = this.GetPropValue(node);
                        this.Filter = Filter.GT(propVal.Item1, propVal.Item2);
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        propVal = this.GetPropValue(node);
                        this.Filter = Filter.GTE(propVal.Item1, propVal.Item2);
                        break;
                    case ExpressionType.Equal:
                        propVal = this.GetPropValue(node);
                        this.Filter = Filter.Eq(propVal.Item1, propVal.Item2);
                        break;
                    case ExpressionType.LessThan:
                        propVal = this.GetPropValue(node);
                        this.Filter = Filter.LT(propVal.Item1, propVal.Item2);
                        break;
                    case ExpressionType.LessThanOrEqual:
                        propVal = this.GetPropValue(node);
                        this.Filter = Filter.LTE(propVal.Item1, propVal.Item2);
                        break;
                    case ExpressionType.NotEqual:
                        propVal = this.GetPropValue(node);
                        this.Filter = Filter.NEq(propVal.Item1, propVal.Item2);
                        break;

                    default:
                        throw new Exception("Invalid Where Clause");
                        break;
                }
            }
            return base.VisitBinary(node);
        }

        public Filter Evaluate(Expression expression)
        {
            Visit(expression);
            return this.Filter;
        }
    }

}
