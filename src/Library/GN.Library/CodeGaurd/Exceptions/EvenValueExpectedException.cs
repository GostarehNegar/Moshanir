namespace GN.CodeGuard.Exceptions
{
    public class EvenValueExpectedException<TValue> : GuardException
    {
        public EvenValueExpectedException(TValue value, string paramName)
            : base(GenerateMessage(paramName, value))
        {

        }

        private static string GenerateMessage(string paramName, TValue value)
        {
            return string.IsNullOrEmpty(paramName)
                ? string.Format("The value '{0}' is not a even number'", value)
                : string.Format("The value '{0}' of '{1}'  is not a even number'", value, paramName);
        }
    }
}