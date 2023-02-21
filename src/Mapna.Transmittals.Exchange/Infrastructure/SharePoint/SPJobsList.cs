using GN.Library.SharePoint.Internals;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GN.Library.SharePoint;
using System.Linq;

namespace Mapna.Transmittals.Exchange.Internals
{
    public class SPJobsList : ListEx<SPJobItem>
    {
        public SPJobsList(ClientRuntimeContext context, ObjectPath objectPath) : base(context, objectPath)
        {
        }

        public ValueTask<SPJobItem> FindJob(string sourceId)
        {
            return this.GetQueryable()
                .Where(x => x.SourceId == sourceId)
                .FirstOrDefaultAsync();
        }
        public async Task<SPJobItem> GetOrCreateItem(SPJobItem job)
        {
            var result = await this.FindJob(job.SourceId);
            if (result == null)
            {
                result = await this.InsertItem(job);
            }
            return result;
        }
    }
}
