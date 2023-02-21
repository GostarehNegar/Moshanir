using System;

namespace GN.CodeGuard.Exceptions
{
    public abstract class GuardException: Exception
    {
        protected GuardException(string message):base(message)
        {
            
        }
    }
}