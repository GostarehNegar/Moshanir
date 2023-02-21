using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.TaskScheduling
{
	/// <summary>
	/// https://crontab.guru/every-2-minutes
	/// </summary>
	public class CronHelper
	{
		public static string EveryMinute => "* * * * *";
		public static string Every2Minutes => "*/2 * * * *";
		public static string Every5Minutes => "*/5 * * * *";

		public static string GetEveryNMinutes(int n) => $"*/2 * * * *";
		public static string EveryDayAtOneAM = "0 1 * * *";
		public static string EveryFriday = "0 0 * * FRI";


	}
}
