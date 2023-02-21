
using GN.Library.SharePoint.Internals.LinqQuery.Vistitors;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace GN.Library.SharePoint.Internals.LinqQuery
{
    interface IQueryExecutor
    {
        Task<object> Execute(Expression expression, bool IsEnumerable);
    }
    class QueryExecutor_Deprecated<T> : IQueryExecutor where T : SPItem
    {
        private readonly SPListAdapter<T> adapter;
        private readonly SPQueryOptions options;
        private readonly List _splist;

        public QueryExecutor_Deprecated(SPListAdapter<T> adapter, SPQueryOptions options)
        {
            this.adapter = adapter;
            this.options = options;
            this._splist = adapter.SPList;
        }
        private static bool IsQueryOverDataSource(Expression expression)
        {
            // If expression represents an unqueried IQueryable data source instance, 
            // expression is of type ConstantExpression, not MethodCallExpression. 
            return (expression is MethodCallExpression);
        }

        private Filter last;
        // https://learn.microsoft.com/en-us/sharepoint/dev/schema/contains-element-query
        private void AddFilter(XmlNode node, Filter filter)
        {
            if (filter == null)
            {
                return;
            }
            switch (filter.Operator)
            {
                case FilterOps.AND:
                    {
                        var n = node.OwnerDocument.CreateElement("And");
                        node.AppendChild(n);
                        AddFilter(n, filter.Left);
                        AddFilter(n, filter.Right);

                    }

                    break;
                case FilterOps.OR:
                    {
                        var n = node.OwnerDocument.CreateElement("Or");
                        node.AppendChild(n);
                        AddFilter(n, filter.Left);
                        AddFilter(n, filter.Right);

                    }
                    break;

                case FilterOps.CONTANIS:
                    {
                        var n = node.OwnerDocument.CreateElement("Contains");
                        node.AppendChild(n);
                        AddFilter(n, filter.Left);
                        AddFilter(n, filter.Right);

                    }
                    break;
                case FilterOps.Eq:
                    {
                        if (filter.Right.Value == null)
                        {
                            /*
                             * https://learn.microsoft.com/en-us/sharepoint/dev/schema/isnull-element-query
                             * <IsNull>
                              <FieldRef Name = "Field_Name"/>
                              <Value Type = "Field_Type"/>
                              <XML />
                            </IsNull>
                             */
                            var n = node.OwnerDocument.CreateElement("IsNull");
                            node.AppendChild(n);
                            AddFilter(n, filter.Left);
                        }
                        else
                        {
                            var n = node.OwnerDocument.CreateElement("Eq");
                            node.AppendChild(n);
                            AddFilter(n, filter.Left);
                            AddFilter(n, filter.Right);
                        }


                    }
                    break;
                case FilterOps.GT:
                    {
                        var n = node.OwnerDocument.CreateElement("Gt");
                        node.AppendChild(n);
                        AddFilter(n, filter.Left);
                        AddFilter(n, filter.Right);

                    }
                    break;
                case FilterOps.GTE:
                    {
                        var n = node.OwnerDocument.CreateElement("Geq");
                        node.AppendChild(n);
                        AddFilter(n, filter.Left);
                        AddFilter(n, filter.Right);

                    }
                    break;
                case FilterOps.LT:
                    {
                        var n = node.OwnerDocument.CreateElement("Lt");
                        node.AppendChild(n);
                        AddFilter(n, filter.Left);
                        AddFilter(n, filter.Right);

                    }
                    break;
                case FilterOps.LTE:
                    {
                        var n = node.OwnerDocument.CreateElement("Leq");
                        node.AppendChild(n);
                        AddFilter(n, filter.Left);
                        AddFilter(n, filter.Right);

                    }
                    break;
                case FilterOps.NEq:
                    {
                        /* https://learn.microsoft.com/en-us/sharepoint/dev/schema/isnotnull-element-query
                         * <IsNotNull>
                              <FieldRef Name = "Field_Name"/>
                              <Value Type = "Field_Type"/>
                              <XML />
                            </IsNotNull>
                         * */
                        if (filter.Right.Value == null)
                        {
                            var n = node.OwnerDocument.CreateElement("IsNotNull");
                            node.AppendChild(n);
                            AddFilter(n, filter.Left);
                        }
                        else
                        {
                            var n = node.OwnerDocument.CreateElement("Neq");
                            node.AppendChild(n);
                            AddFilter(n, filter.Left);
                            AddFilter(n, filter.Right);
                        }

                    }
                    break;

                case FilterOps.PROP:
                    {

                        var n = node.OwnerDocument.CreateElement("FieldRef");
                        //var c = this._splist.GetColumnName(filter.Value?.ToString());
                        var c = this.options.GetColumnName(filter.Value?.ToString());
                        n.SetAttribute("Name", c);
                        node.AppendChild(n);
                    }
                    break;
                case FilterOps.VAL:
                    {
                        var n = node.OwnerDocument.CreateElement("Value");
                        var p = last?.Value?.ToString();
                        //var val = this.adapter.GetFilterVal(p, filter.Value);
                        var val = this._splist.GetFilterVal(this.options.GetColumnName(p), filter.Value);
                        n.SetAttribute("Type", val.Item1);
                        n.InnerText = val.Item2;
                        node.AppendChild(n);

                    }
                    break;
                default:
                    break;
            }

            last = filter;
        }
        private void AddFilter(CamlQuery query, Filter filter)
        {
            if (filter == null)
            {
                return;
            }
            var xml = new XmlDocument();
            xml.LoadXml(query.ViewXml);
            var viewNode = xml.SelectSingleNode("/View");
            var queryNode = viewNode.SelectSingleNode("Query");
            if (queryNode == null)
            {
                queryNode = viewNode.OwnerDocument.CreateElement("Query");
                viewNode.AppendChild(queryNode);
            }
            var whereNode = queryNode.OwnerDocument.SelectSingleNode("Where");
            if (whereNode == null)
            {
                whereNode = queryNode.OwnerDocument.CreateElement("Where");
                queryNode.AppendChild(whereNode);
            }

            var whereNodeq = queryNode.OwnerDocument.SelectSingleNode("Where");
            AddFilter(whereNode, filter);
            query.ViewXml = xml.InnerXml;




        }
        public void AddOrderBy(CamlQuery query, string prop, bool ascending)
        {
            var xml = new XmlDocument();
            xml.LoadXml(query.ViewXml);
            var viewNode = xml.SelectSingleNode("/View");
            var queryNode = viewNode.SelectSingleNode("Query");
            if (queryNode == null)
            {
                queryNode = viewNode.OwnerDocument.CreateElement("Query");
                viewNode.AppendChild(queryNode);
            }
            var orderByNode = queryNode.OwnerDocument.SelectSingleNode("OderBy");
            if (orderByNode == null)
            {
                orderByNode = queryNode.OwnerDocument.CreateElement("OrderBy");
                queryNode.AppendChild(orderByNode);
            }
            var field = orderByNode.OwnerDocument.CreateElement("FieldRef");
            field.SetAttribute("Ascending", ascending ? "TRUE" : "FALSE");
            field.SetAttribute("Name", this.options.GetColumnName(prop));
            orderByNode.AppendChild(field);
            query.ViewXml = xml.InnerXml;
        }
        public async Task<object> Execute(Expression expression, bool IsEnumerable)
        {
            await Task.CompletedTask;
            // The expression must represent a query over the data source. 
            if (!IsQueryOverDataSource(expression))
                throw new InvalidProgramException("No query over the data source was specified.");

            var take = new TakeFinder().FindValue(expression);
            var skip = new SkipFinder().FindValue(expression);
            var orderBy = new OrderByFinder().FindValue(expression);
            var orderByDescending = new OrderByDescendingFinder().FindValue(expression);
            InnermostWhereFinder whereFinder = new InnermostWhereFinder();
            MethodCallExpression whereExpression = whereFinder.GetInnermostWhere(expression);
            var filterEvaluator = new FilterEvaluator();
            if (whereExpression != null)
            {
                filterEvaluator.Evaluate(whereExpression);
            }

            var pageLength = 200;
            if (skip.HasValue && skip.Value > this.options.RowLimit)
            {
                this.options.WithRowLimit(skip.Value);
            }
            //CamlQuery query = take.HasValue ? CamlQuery.CreateAllItemsQuery(take.Value) : CamlQuery.CreateAllItemsQuery();
            CamlQuery query = this.options.GetQuery();
            //await this.adapter.SPList.ValidateQuery(this.options);
            await this._splist.ValidateQuery(this.options);
            AddFilter(query, filterEvaluator.Filter);
            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                this.AddOrderBy(query, orderBy, true);
            }
            if (!string.IsNullOrWhiteSpace(orderByDescending))
            {
                this.AddOrderBy(query, orderByDescending, false);
            }
            //var items = await this.list.GetItems(query);
            var items = new List<SPItem>();
            var page = skip.HasValue ? skip.Value / pageLength + 1 : 1;
            var context = this._splist.Context;
            while (page > 0)
            {
                ListItemCollection _items = this._splist.GetItems(query);
                context.Load(_items);
                await context.ExecuteQueryAsync();
                query.ListItemCollectionPosition = _items.ListItemCollectionPosition;
                items.AddRange(_items
                    .Select(x => new SPItem().Init(x, null))
                    .ToArray());
                page--;
            }
            var result = skip.HasValue ? items.Skip(skip.Value) : items;
            result = take.HasValue ? result.Take(take.Value) : result;

            return result.Select(x => x.To<T>()).ToArray();

            // Find the call to Where() and get the lambda expression predicate.
            //var skip = new SkipFinder().FindValue(expression);
            //var take = new TakeFinder().FindValue(expression);
            //var orderBy = new OrderByFinder().FindValue(expression);
            //var orderByDescending = new OrderByDescendingFinder().FindValue(expression);
            //InnermostWhereFinder whereFinder = new InnermostWhereFinder();
            //MethodCallExpression whereExpression = whereFinder.GetInnermostWhere(expression);

            //var filterEvaluator = new FilterEvaluator();
            //if (whereExpression != null)
            //{
            //    filterEvaluator.Evaluate(whereExpression);

            //}
            //var res = await this.list.FetchAll(q =>
            //{
            //    if (take.HasValue)
            //    {
            //        q.Take(take.Value);
            //    }
            //    if (skip.HasValue)
            //    {
            //        q.Skip(skip.Value);
            //    }
            //    if (filterEvaluator?.Filter != null)
            //    {
            //        q.Filter(filterEvaluator.Filter);
            //    }
            //    if (!string.IsNullOrWhiteSpace(orderBy))
            //    {
            //        q.OrderBy(orderBy);
            //    }
            //    if (!string.IsNullOrWhiteSpace(orderByDescending))
            //    {
            //        q.OrderBy(orderByDescending,true);
            //    }

            //});

            //return res.ToArray();


        }
    }
    //class TerraServerQueryContext
    //{
    //    // Executes the expression tree that is passed to it. 
    //    internal static object Execute(Expression expression, bool IsEnumerable)
    //    {
    //        // The expression must represent a query over the data source. 
    //        if (!IsQueryOverDataSource(expression))
    //            throw new InvalidProgramException("No query over the data source was specified.");

    //        // Find the call to Where() and get the lambda expression predicate.
    //        InnermostWhereFinder whereFinder = new InnermostWhereFinder();
    //        MethodCallExpression whereExpression = whereFinder.GetInnermostWhere(expression);
    //        var ff = new FilterEvaluator();
    //        //var q = new StoreFetchPayload();
    //        ff.Evaluate(whereExpression);

    //        LambdaExpression lambdaExpression = (LambdaExpression)((UnaryExpression)(whereExpression.Arguments[1])).Operand;

    //        //// Send the lambda expression through the partial evaluator.
    //        //lambdaExpression = (LambdaExpression)Evaluator.PartialEval(lambdaExpression);

    //        //// Get the place name(s) to query the Web service with.
    //        //LocationFinder lf = new LocationFinder(lambdaExpression.Body);
    //        //List<string> locations = lf.Locations;
    //        //if (locations.Count == 0)
    //        //    throw new InvalidQueryException("You must specify at least one place name in your query.");

    //        //// Call the Web service and get the results.
    //        //Place[] places = WebServiceHelper.GetPlacesFromTerraServer(locations);

    //        //// Copy the IEnumerable places to an IQueryable.
    //        //IQueryable<Place> queryablePlaces = places.AsQueryable<Place>();

    //        // Copy the expression tree that was passed in, changing only the first 
    //        // argument of the innermost MethodCallExpression.
    //        //ExpressionTreeModifier treeCopier = new ExpressionTreeModifier(queryablePlaces);
    //        //Expression newExpressionTree = treeCopier.Visit(expression);

    //        // This step creates an IQueryable that executes by replacing Queryable methods with Enumerable methods. 
    //        //if (IsEnumerable)
    //        //    return queryablePlaces.Provider.CreateQuery(newExpressionTree);
    //        //else
    //        //    return queryablePlaces.Provider.Execute(newExpressionTree);
    //        return new string[] { };
    //    }

    //    private static bool IsQueryOverDataSource(Expression expression)
    //    {
    //        // If expression represents an unqueried IQueryable data source instance, 
    //        // expression is of type ConstantExpression, not MethodCallExpression. 
    //        return (expression is MethodCallExpression);
    //    }
    //}

    class QueryExecutor<T> : IQueryExecutor where T : SPItem
    {
        //private readonly SPListAdapter<T> adapter;
        private readonly SPQueryOptions options;
        private readonly List _splist;

        public QueryExecutor(List list, SPQueryOptions options)
        {
            this.options = options;
            this._splist = list;
        }
        private static bool IsQueryOverDataSource(Expression expression)
        {
            // If expression represents an unqueried IQueryable data source instance, 
            // expression is of type ConstantExpression, not MethodCallExpression. 
            return (expression is MethodCallExpression);
        }

        private Filter last;
        private XmlElement last_prop_node;
        // https://learn.microsoft.com/en-us/sharepoint/dev/schema/contains-element-query
        private void AddFilter(XmlNode node, Filter filter)
        {
            if (filter == null)
            {
                return;
            }
            switch (filter.Operator)
            {
                case FilterOps.AND:
                    {
                        var n = node.OwnerDocument.CreateElement("And");
                        node.AppendChild(n);
                        AddFilter(n, filter.Left);
                        AddFilter(n, filter.Right);

                    }

                    break;
                case FilterOps.OR:
                    {
                        var n = node.OwnerDocument.CreateElement("Or");
                        node.AppendChild(n);
                        AddFilter(n, filter.Left);
                        AddFilter(n, filter.Right);

                    }
                    break;

                case FilterOps.CONTANIS:
                    {
                        var n = node.OwnerDocument.CreateElement("Contains");
                        node.AppendChild(n);
                        AddFilter(n, filter.Left);
                        AddFilter(n, filter.Right);

                    }
                    break;
                case FilterOps.Eq:
                    {
                        if (filter.Right.Value == null)
                        {
                            /*
                             * https://learn.microsoft.com/en-us/sharepoint/dev/schema/isnull-element-query
                             * <IsNull>
                              <FieldRef Name = "Field_Name"/>
                              <Value Type = "Field_Type"/>
                              <XML />
                            </IsNull>
                             */
                            var n = node.OwnerDocument.CreateElement("IsNull");
                            node.AppendChild(n);
                            AddFilter(n, filter.Left);
                        }
                        else
                        {
                            var n = node.OwnerDocument.CreateElement("Eq");
                            node.AppendChild(n);
                            AddFilter(n, filter.Left);
                            AddFilter(n, filter.Right);
                        }


                    }
                    break;
                case FilterOps.GT:
                    {
                        var n = node.OwnerDocument.CreateElement("Gt");
                        node.AppendChild(n);
                        AddFilter(n, filter.Left);
                        AddFilter(n, filter.Right);

                    }
                    break;
                case FilterOps.GTE:
                    {
                        var n = node.OwnerDocument.CreateElement("Geq");
                        node.AppendChild(n);
                        AddFilter(n, filter.Left);
                        AddFilter(n, filter.Right);

                    }
                    break;
                case FilterOps.LT:
                    {
                        var n = node.OwnerDocument.CreateElement("Lt");
                        node.AppendChild(n);
                        AddFilter(n, filter.Left);
                        AddFilter(n, filter.Right);

                    }
                    break;
                case FilterOps.LTE:
                    {
                        var n = node.OwnerDocument.CreateElement("Leq");
                        node.AppendChild(n);
                        AddFilter(n, filter.Left);
                        AddFilter(n, filter.Right);

                    }
                    break;
                case FilterOps.NEq:
                    {
                        /* https://learn.microsoft.com/en-us/sharepoint/dev/schema/isnotnull-element-query
                         * <IsNotNull>
                              <FieldRef Name = "Field_Name"/>
                              <Value Type = "Field_Type"/>
                              <XML />
                            </IsNotNull>
                         * */
                        if (filter.Right.Value == null)
                        {
                            var n = node.OwnerDocument.CreateElement("IsNotNull");
                            node.AppendChild(n);
                            AddFilter(n, filter.Left);
                        }
                        else
                        {
                            var n = node.OwnerDocument.CreateElement("Neq");
                            node.AppendChild(n);
                            AddFilter(n, filter.Left);
                            AddFilter(n, filter.Right);
                        }

                    }
                    break;

                case FilterOps.PROP:
                    {

                        var n = node.OwnerDocument.CreateElement("FieldRef");
                        //var c = this._splist.GetColumnName(filter.Value?.ToString());
                        var c = this.options.GetColumnName(filter.Value?.ToString());
                        n.SetAttribute("Name", c);
                        //n.SetAttribute("LookupId", "TRUE");
                        node.AppendChild(n);
                        this.last_prop_node = n;
                    }
                    break;
                case FilterOps.VAL:
                    {
                        var n = node.OwnerDocument.CreateElement("Value");
                        if (string.IsNullOrWhiteSpace(last?.Value?.ToString()))
                        {
                            throw new Exception("Unexpcted Excetion. PROP is missing");
                        }
                        var val = this._splist.GetFilterVal(this.options.GetColumnName(last?.Value?.ToString()), filter.Value);
                        if (this.last_prop_node!=null && val.Item1 == "Lookup" && filter.Value != null && (filter.Value.GetType() == typeof(int)))
                        {
                            this.last_prop_node.SetAttribute("LookupId", "TRUE");
                        }
                        //var val = this._splist.GetFilterValEx(this.options.GetColumnName(p), filter.Value);
                        //val.Keys.Where(x => x != "Value")
                        //    .ToList()
                        //    .ForEach(k => n.SetAttribute(k, val[k]));
                        //n.InnerText = val["Value"];
                        n.SetAttribute("Type", val.Item1);
                        n.InnerText = val.Item2;
                        node.AppendChild(n);

                    }
                    break;
                default:
                    break;
            }

            last = filter;
        }
        private void AddFilter(CamlQuery query, Filter filter)
        {
            if (filter == null)
            {
                return;
            }
            var xml = new XmlDocument();
            xml.LoadXml(query.ViewXml);
            var viewNode = xml.SelectSingleNode("/View");
            var queryNode = viewNode.SelectSingleNode("Query");
            if (queryNode == null)
            {
                queryNode = viewNode.OwnerDocument.CreateElement("Query");
                viewNode.AppendChild(queryNode);
            }
            var whereNode = queryNode.OwnerDocument.SelectSingleNode("Where");
            if (whereNode == null)
            {
                whereNode = queryNode.OwnerDocument.CreateElement("Where");
                queryNode.AppendChild(whereNode);
            }

            var whereNodeq = queryNode.OwnerDocument.SelectSingleNode("Where");
            AddFilter(whereNode, filter);
            query.ViewXml = xml.InnerXml;




        }
        public void AddOrderBy(CamlQuery query, string prop, bool ascending)
        {
            var xml = new XmlDocument();
            xml.LoadXml(query.ViewXml);
            var viewNode = xml.SelectSingleNode("/View");
            var queryNode = viewNode.SelectSingleNode("Query");
            if (queryNode == null)
            {
                queryNode = viewNode.OwnerDocument.CreateElement("Query");
                viewNode.AppendChild(queryNode);
            }
            var orderByNode = queryNode.OwnerDocument.SelectSingleNode("OderBy");
            if (orderByNode == null)
            {
                orderByNode = queryNode.OwnerDocument.CreateElement("OrderBy");
                queryNode.AppendChild(orderByNode);
            }
            var field = orderByNode.OwnerDocument.CreateElement("FieldRef");
            field.SetAttribute("Ascending", ascending ? "TRUE" : "FALSE");
            field.SetAttribute("Name", this.options.GetColumnName(prop));
            orderByNode.AppendChild(field);
            query.ViewXml = xml.InnerXml;
        }
        public async Task<object> Execute(Expression expression, bool IsEnumerable)
        {
            await Task.CompletedTask;
            // The expression must represent a query over the data source. 
            if (!IsQueryOverDataSource(expression))
                throw new InvalidProgramException("No query over the data source was specified.");

            var take = new TakeFinder().FindValue(expression);
            var skip = new SkipFinder().FindValue(expression);
            var orderBy = new OrderByFinder().FindValue(expression);
            var orderByDescending = new OrderByDescendingFinder().FindValue(expression);
            InnermostWhereFinder whereFinder = new InnermostWhereFinder();
            MethodCallExpression whereExpression = whereFinder.GetInnermostWhere(expression);
            var filterEvaluator = new FilterEvaluator();
            if (whereExpression != null)
            {
                filterEvaluator.Evaluate(whereExpression);
            }

            var pageLength = 200;
            if (skip.HasValue && skip.Value > this.options.RowLimit)
            {
                this.options.WithRowLimit(skip.Value);
            }
            //CamlQuery query = take.HasValue ? CamlQuery.CreateAllItemsQuery(take.Value) : CamlQuery.CreateAllItemsQuery();
            CamlQuery query = this.options.GetQuery();
            //await this.adapter.SPList.ValidateQuery(this.options);
            await this._splist.ValidateQuery(this.options);
            AddFilter(query, filterEvaluator.Filter);
            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                this.AddOrderBy(query, orderBy, true);
            }
            if (!string.IsNullOrWhiteSpace(orderByDescending))
            {
                this.AddOrderBy(query, orderByDescending, false);
            }
            //var items = await this.list.GetItems(query);
            var items = new List<SPItem>();
            var page = skip.HasValue ? skip.Value / pageLength + 1 : 1;
            var context = this._splist.Context;
            while (page > 0)
            {
                ListItemCollection _items = this._splist.GetItems(query);
                context.Load(_items);
                await context.ExecuteQueryAsync();
                query.ListItemCollectionPosition = _items.ListItemCollectionPosition;
                items.AddRange(_items
                    .Select(x => new SPItem().Init(x, null))
                    .ToArray());
                page--;
            }
            var result = skip.HasValue ? items.Skip(skip.Value) : items;
            result = take.HasValue ? result.Take(take.Value) : result;
            return result.Select(x => x.To<T>()).ToArray();
        }
    }
}
