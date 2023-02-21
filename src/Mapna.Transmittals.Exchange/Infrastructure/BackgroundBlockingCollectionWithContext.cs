using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mapna.Transmittals.Exchange.Internals
{
    public class BackgroundBlockingCollectionContext
    {
        public string GGG = "1";
        public async Task Invoke(CancellationToken token)
        {
            await Task.Delay(1000);
        }
    }
    public class BackgroundBlockingCollectionWithContext : GN.Library.TaskScheduling.BackgroundMultiBlockingTaskHostedService
    {
        //private BlockingCollection<Func<CancellationToken, Task>> queue = new BlockingCollection<Func<CancellationToken, Task>>();

        public BackgroundBlockingCollectionWithContext() : base(5)
        {

        }
    }

}
