using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.TaskScheduling
{
    public class SomeOtherTask : ScheduledTask, IScheduledTask
    {
        public override string  Schedule => "0 5 * * *";
		public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(5000, cancellationToken);
        }
    }
}