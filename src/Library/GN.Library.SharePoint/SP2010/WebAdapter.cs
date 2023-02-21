using GN.Library.SharePoint.SP2010.WebReferences.Lists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.SharePoint.SP2010
{
    class WebAdapter
    {
        public static void Test()
        {
            /// Here we use sharepoint webservices.
            /// This is deprecated in favor of csom ClientContext.
            /// 
            var s = new Lists();
            s.Url = "http://projects.gnco.ir/parnian/_vti_bin/Lists.asmx";
            s.UseDefaultCredentials = true;
            var f = s.GetListCollection();
            var adapter = new GN.Library.SharePoint.SP2010.WebReferences.Webs.Webs();
            adapter.Url = "http://projects.gnco.ir/parnian/_vti_bin/Webs.asmx";
            adapter.UseDefaultCredentials = true;
            var webs = adapter.GetWebCollection();
            



        }
    }
}
