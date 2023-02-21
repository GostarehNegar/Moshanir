using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Functional.Pipelines
{
    public class RequestResonseState<TReq, TRes>
    {
        public RequestResonseState(TReq request = default, TRes response = default)
        {
            Request = request;
            Response = response;
        }
        public TReq Request { get; }
        public TRes Response { get; }

        RequestResonseState<TReq, TRes> WithRequest(TReq request) => new RequestResonseState<TReq, TRes>(request, this.Response);
        RequestResonseState<TReq, TRes> WithRsponse(TRes response) => new RequestResonseState<TReq, TRes>(this.Request, response);
    }
}
