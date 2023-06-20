using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Shared
{
    
    public interface IProcedureCall
    {
        Task<TResponse> Call<TRequest, TResponse>(TRequest request, int timeOut = LibraryConstants.DefaultTimeout, string subject = null);
        Task<TResponse> Call<TRequest, TResponse>(TRequest request, string subject, int timeOut = LibraryConstants.DefaultTimeout);
    }
}
