using System;
using System.Collections.Generic;
using System.Text;

namespace Mapna.Transmittals.Exchange.Internals
{
    public class TransmittalsExchangeOptions
    {
        public static string MapnaDefaultEndpoint = "https://mycart.mapnagroup.com/group_app/ws_dc/npx/sendtrn/";
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


        public string MapnaEndpoint => MapnaDefaultEndpoint;

        public int MaxTrialsInSendinfTransmittals => 2;

        
    }
}
