using GN.Library.Shared.ServiceDiscovery;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace GN.Library.ServiceDiscovery
{
    public interface IServiceDataProvider
    {
        ServiceData GetData();
    }
}
