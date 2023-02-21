using System.Collections.Generic;
using System.Text;

namespace Mapna.Transmittals.Exchange
{
    public class SubmitTransmittalReply
    {
        public bool Failed { get; set; }
        public string Error { get; set; }
        public string TransmittalId { get; set; }
    }

}
