using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.Helpers
{
	internal struct VoidTypeStruct { }
	public static class UtilityHelpers
	{
		public static bool WildCardMatch(string value, string pattern)
		{
			if (value == null || pattern == null)
				return false;
			var exp = "^" + Regex.Escape(pattern).Replace("\\?", ".").Replace("\\*", ".*") + "$";
			return Regex.IsMatch(value, exp);
		}
		public static T Cast<T>(Delegate source) where T : class
		{
			return Cast(source, typeof(T)) as T;
		}
		public static Delegate Cast(Delegate source, Type type)
		{
			if (source == null)
				return null;

			Delegate[] delegates = source.GetInvocationList();
			if (delegates.Length == 1)
				return Delegate.CreateDelegate(type,
					delegates[0].Target, delegates[0].Method);

			Delegate[] delegatesDest = new Delegate[delegates.Length];
			for (int nDelegate = 0; nDelegate < delegates.Length; nDelegate++)
				delegatesDest[nDelegate] = Delegate.CreateDelegate(type,
					delegates[nDelegate].Target, delegates[nDelegate].Method);
			return Delegate.Combine(delegatesDest);
		}

		public static async Task TimeoutAfter(this Task task, int millisecondsTimeout, CancellationToken token, bool Throw = true)
		{
			if (task == await Task.WhenAny(task, Task.Delay(millisecondsTimeout)))
				await task;
			else if (Throw)
				throw new TimeoutException();

		}
		public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, int millisecondsTimeout, CancellationToken token, bool Throw = true)
		{
			if (task == await Task.WhenAny(task, Task.Delay(millisecondsTimeout, token)))
				return await task.ConfigureAwait(false);
			else if (Throw)
				throw new TimeoutException();
			return default(TResult);
		}
		internal static void MarshalTaskResults<TResult>(
					Task source, TaskCompletionSource<TResult> proxy)
		{
			switch (source.Status)
			{
				case TaskStatus.Faulted:
					proxy.TrySetException(source.Exception);
					break;
				case TaskStatus.Canceled:
					proxy.TrySetCanceled();
					break;
				case TaskStatus.RanToCompletion:
					Task<TResult> castedSource = source as Task<TResult>;
					proxy.TrySetResult(
						castedSource == null ? default(TResult) : // source is a Task
							castedSource.Result); // source is a Task<TResult>
					break;
			}
		}
		public static Task TimeoutAfter<TResult>(this Task task, int millisecondsTimeout)
		{
			// Short-circuit #1: infinite timeout or task already completed
			if (task.IsCompleted || (millisecondsTimeout == Timeout.Infinite))
			{
				// Either the task has already completed or timeout will never occur.
				// No proxy necessary.
				return task;
			}

			// tcs.Task will be returned as a proxy to the caller
			TaskCompletionSource<TResult> tcs =
				new TaskCompletionSource<TResult>();

			// Short-circuit #2: zero timeout
			if (millisecondsTimeout == 0)
			{
				// We've already timed out.
				tcs.SetException(new TimeoutException());
				return tcs.Task;
			}

			// Set up a timer to complete after the specified timeout period
			Timer timer = new Timer(state =>
			{
				// Recover your state information
				var myTcs = (TaskCompletionSource<VoidTypeStruct>)state;

				// Fault our proxy with a TimeoutException
				myTcs.TrySetException(new TimeoutException());
			}, tcs, millisecondsTimeout, Timeout.Infinite);

			// Wire up the logic for what happens when source task completes
			task.ContinueWith((antecedent, state) =>
			{
				// Recover our state data
				var tuple =
					(Tuple<Timer, TaskCompletionSource<TResult>>)state;

				// Cancel the Timer
				tuple.Item1.Dispose();

				// Marshal results to proxy
				MarshalTaskResults(antecedent, tuple.Item2);
			},
			Tuple.Create(timer, tcs),
			CancellationToken.None,
			TaskContinuationOptions.ExecuteSynchronously,
			TaskScheduler.Default);

			return tcs.Task;
		}

		public static Task<TResult> CreateTaskWithTimeOut<TResult>(Func<TResult> work, CancellationToken token = default(CancellationToken), int timeOut = 1 * 60 * 1000, bool Throw = true)
		{
			TResult result = default(TResult);

			var task = Task.Run<TResult>(() =>
			{
				while (true)
				{
					try
					{
						token.ThrowIfCancellationRequested();
						result = work();
						return result;
					}
					catch { }
				}
			}, token);
			return task.TimeoutAfter(timeOut, token, Throw);
		}

	}
}
