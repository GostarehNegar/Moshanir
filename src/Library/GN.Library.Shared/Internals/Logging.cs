using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Internals
{
    public interface Log
    {
        string Level { get; set; }
        string Message { get; }
    }


    public class LogEvent
        : Log
    {
        public string Message { get; set; }
        public string Level { get; set; }
    }

    //public interface CommandLineResponse
    //{

    //}
}
