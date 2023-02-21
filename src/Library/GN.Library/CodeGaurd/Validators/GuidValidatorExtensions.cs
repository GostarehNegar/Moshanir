using System;
using GN.CodeGuard.Internals;

namespace GN.CodeGuard
{
    public static class GuidValidatorExtensions
    {
        public static ArgBase<Guid> IsNotEmpty(this ArgBase<Guid> arg)
        {
            if (arg.Value.Equals(Guid.Empty))
                arg.ThrowArgument("Guid is empty");

            return arg;
        }
    }
}