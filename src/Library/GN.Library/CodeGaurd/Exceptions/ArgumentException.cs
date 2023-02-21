
namespace GN.CodeGuard.Exceptions
{
    public class ArgumentException : GuardException
    {
        public ArgumentException(string message)
            : base(message)
        {

        }
        public ArgumentException(string message, string param)
            : this(string.Format("{0}. Parameter name: {1}",message,param))
        {

        }
    }
}