using System;
using System.Collections.Generic;
using System.Text;

namespace Mapna.Transmittals.Exchange.Internals
{
    public class TransmittalsExchangeOptions
    {
        public class ListOptions
        {
            public string Path { get; set; }
        }
        public class TransmittalsOptionsModel : ListOptions
        {

        }
        public class DocLibOptionsModel : ListOptions
        {
        }
        public class MasterListOptions : ListOptions
        {
        }
        public TransmittalsOptionsModel Transmittals { get; set; }
        public DocLibOptionsModel Documents { get; set; }
        public MasterListOptions MasterList { get; set; }

        public string ConnectionString { get; set; }
    }
}
