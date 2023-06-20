using System.Collections.Generic;
using System.Text;

namespace Mapna.Transmittals.Exchange
{
    public class SubmitTransmittalReply
    {
        public int Failed { get; set; }
        public string Error { get; set; }
        public string TransmittalId { get; set; }
    }

}
