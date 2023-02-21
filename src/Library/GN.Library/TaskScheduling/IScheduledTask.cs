using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.TaskScheduling
{
    public interface IScheduledTask
    {
		/// <summary>
		/// 
		/// https://crontab.guru/
		/// </summary>
		string Schedule { get; }
		string Name { get; }
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}