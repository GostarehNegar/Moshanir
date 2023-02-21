namespace GN.CodeGuard.Exceptions
{
    public class GreaterThenExpectedException<TValue> : GuardException
    {
        public GreaterThenExpectedException(TValue value, TValue max, string paramName)
            : base(GenerateMessage(paramName, value, max))
        {

        }

        private static string GenerateMessage(string paramName, TValue value, TValue max)
        {
            return string.IsNullOrEmpty(paramName)
                ? string.Format( "The value '{0}' is larger than '{1}'",value,max)
                : string.Format("The value '{0}' of '{1}' is larger than '{2}'",value,paramName,max);
        }
    }
}