using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapna.Transmittals.Exchange.GhodsNiroo
{
    public class GhodsNirooTransmittalOptions
    {
        public const string Default = "url=https://gpp.ghods-niroo.com/sites/GhodsNiroo/gpp/Projects/gpp, UserName=emoarefi, Password=Gnec_Map98, Domain=Gnce";
        public string ConnectionString { get; set; } = Default;

        public GhodsNirooTransmittalOptions()
        {
            this.ConnectionString = Default;
        }
        public GhodsNirooTransmittalOptions Validate()
        {
            this.ConnectionString = string.IsNullOrWhiteSpace(ConnectionString) ? Default: ConnectionString;

            return this;
        }
    }
}
