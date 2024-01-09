using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapna.Transmittals.Exchange.GhodsNiroo.Incoming
{
    class IncomingException : Exception
    {
        public bool Retryable { get; }

        public IncomingException(string message, bool retryable = true, Exception inner = null) : base(message, inner) { this.Retryable = retryable; }
    }
}
