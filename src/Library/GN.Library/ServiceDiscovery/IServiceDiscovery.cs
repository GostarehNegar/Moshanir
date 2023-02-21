using GN.Library.Shared.ServiceDiscovery;
using System;
using System.Collections.Generic;
using System.Text;


namespace GN.Library.ServiceDiscovery
{
    public interface IServiceDiscovery
    {
        IEnumerable<ServiceData> GetServices();
    }
}
