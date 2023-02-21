
namespace GN.CodeGuard.Exceptions
{
    public class ArgumentNullException : GuardException
    {
        public ArgumentNullException()
            : base("Argument is null")
        {

        }
        public ArgumentNullException(string param)
            : base(string.Format("Argument is null. Parameter name: {0}", param))
        {

        }
    }
}