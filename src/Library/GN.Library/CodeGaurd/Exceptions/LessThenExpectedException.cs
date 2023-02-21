namespace GN.CodeGuard.Exceptions
{
    public class LessThenExpectedException<TValue> : GuardException
    {
        public LessThenExpectedException(TValue value, TValue max, string paramName)
            : base(GenerateMessage(paramName, value, max))
        {

        }

        private static string GenerateMessage(string paramName, TValue value, TValue max)
        {
            return string.IsNullOrEmpty(paramName)
                ? string.Format("The value '{0}' is less than '{1}'", value, max)
                : string.Format("The value '{0}' of '{1}' is less than '{2}'", value, paramName, max);
        }
    }
}