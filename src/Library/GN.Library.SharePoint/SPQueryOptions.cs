using GN.Library.SharePoint.Internals;
using Microsoft.SharePoint.Client;
using System;

namespace GN.Library.SharePoint
{
    public class SPQueryOptions
    {
        public Type elementType { get; private set; }

        public string[] Columns { get; private set; }
        public int RowLimit { get; private set; }
        public SPQueryOptions(Type elementType = null)
        {
            RowLimit = 200;
            this.elementType = elementType;
        }
        public string GetColumnName(string n)
        {
            return elementType.GetColumnName(n);
        }
        internal SPQueryOptions WithType(Type elementType)
        {
            this.elementType = elementType;
            return this;
        }
        internal CamlQuery GetQuery()
        {
            return this.Columns == null || this.Columns.Length == 0
                ? CamlQuery.CreateAllItemsQuery(this.RowLimit)
                : CamlQuery.CreateAllItemsQuery(this.RowLimit, this.Columns);
        }
        public SPQueryOptions WithColums(params string[] args)
        {
            this.Columns = args;
            return this;
        }
        public SPQueryOptions WithRowLimit(int limit)
        {
            this.RowLimit = limit;
            return this;
        }

    }
}
