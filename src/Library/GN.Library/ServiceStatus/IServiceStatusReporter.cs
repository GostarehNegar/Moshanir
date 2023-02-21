using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.ServiceStatus
{
    public class StatusReportContext
    {
        public IAppServices Services { get; private set; }
        public StringBuilder Writer { get; private set; }
        public StatusReportContext(IAppServices services)
        {
            this.Writer = new StringBuilder();
            this.Services = services;
        }
        public void InfoFormat(string fmt, params object[] args)
        {
            Writer.AppendLine(string.Format(fmt, args));
        }
        public void WarnFormat(string fmt, params object[] args)
        {
            Writer.AppendLine("WARNING: " + string.Format(fmt, args));
        }
        public void ErrorFormat(string fmt, params object[] args)
        {
            Writer.AppendLine("$ERROR:" + string.Format(fmt, args));
        }

    }
    public interface IServiceStatusReporter
    {
        void GenerateStatusReport(StatusReportContext context);
    }
}
