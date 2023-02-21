using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.TaskScheduling
{

	public class ScheduledTask : IScheduledTask
	{
		
		/// <summary>
		/// 
		/// https://crontab.guru/
		/// </summary>
		protected string schedule = "* * * * *";
		protected string name = "";
		public virtual string Schedule => this.schedule;
		public virtual string Name => this.name;
		public ScheduledTask()
		{
			this.schedule = "* * * * *";
			this.name = this.GetType().Name;
		}


		public virtual Task ExecuteAsync(CancellationToken cancellationToken)
		{
			return Task.FromResult(true);
		}
	}
}
