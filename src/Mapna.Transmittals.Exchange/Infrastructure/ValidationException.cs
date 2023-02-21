namespace Mapna.Transmittals.Exchange
{
    public class ValidationException : TransmitalException
    {
        public ValidationException(string message, bool isRetryable=true) : base(message, isRetryable)
        {
        }
    }

}
