using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace GN.Library.SharePoint.Internals
{
    public class SPListSchema
    {
        private FieldCollection _fields;
        private Type _itemType;
        private string[] _attributeNames;
        public SPListSchema(FieldCollection fields, Type itemType)
        {
            this._fields = fields;
            this._itemType = itemType;
            this._attributeNames = this._itemType.GetProperties()
                .Select (x=>x.GetColumnName())
                .Where (x=> !string.IsNullOrWhiteSpace(x))
                .ToArray();
        }
    }
}
