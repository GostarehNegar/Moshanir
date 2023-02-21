using System;

namespace Mapna.Transmittals.Exchange
{
    public class TransmitalException : Exception
    {
        public bool IsRecoverable { get; private set; }
        public TransmitalException(string message, bool isrecoverable = true) : base(message) { this.IsRecoverable = isrecoverable; }

    }

}
