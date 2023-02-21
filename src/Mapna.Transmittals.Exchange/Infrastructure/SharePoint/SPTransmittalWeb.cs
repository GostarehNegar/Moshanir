using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Text;
using GN.Library.SharePoint;
using System.Threading.Tasks;

namespace Mapna.Transmittals.Exchange.Internals
{
    class SPTransmittalWeb : Web
    {
        private TransmittalsExchangeOptions options;
        public SPTransmittalWeb(ClientRuntimeContext context, ObjectPath objectPath) : base(context, objectPath)
        {
            this.options = GN.AppHost.GetService<TransmittalsExchangeOptions>();
        }
        
        public async Task<SPTransmittalList> GetTransmitalsList()
        {
            var res = await this.GetListByPath(this.options.Transmittals.Path);
            //var res = await this.GetListByPath("/Transmittals/");
            return res.Extend<SPTransmittalList>();
        }
        public async Task<SPMaterList> GetMasterList()
        {
            var res = await this.GetListByPath(this.options.MasterList.Path);
            return res.Extend<SPMaterList>();
        }
        public async Task<SPJobsList> GetJobsList()
        {
            var res = await this.GetListByPath("/Jobs/");
            return res.Extend<SPJobsList>();
        }
        public async Task<SPDocLib> GetDocLib()
        {
            var res = await this.GetListByPath(this.options.Documents.Path);
            return res.Extend<SPDocLib>();
        }
    }
}
