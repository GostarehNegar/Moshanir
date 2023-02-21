using System;
using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;


namespace GN.Library.Transactions
{
    /// <summary>
    /// Default (async, as in: the <see cref="CompleteAsync"/> method returns a <see cref="Task"/> to be awaited) transaction scope 
    /// that sets up an ambient <see cref="ITransactionContext"/> and removes
    /// it when the scope is disposed. Call <code>await scope.Complete();</code> in order to end the scope
    /// by committing any actions enlisted to be executed.
    /// </summary>
    public class TransactionScopeContainer : IDisposable
    {
        readonly ITransactionContext _previousTransactionContext = AmbientTransactionContext.Current;
        readonly TransactionContext _transactionContext = new TransactionContext();

        /// <summary>
        /// Creates a new transaction context and mounts it on <see cref="AmbientTransactionContext.Current"/>, making it available for Rebus
        /// to pick up. The context can also be retrieved simply via <see cref="TransactionContext"/>
        /// </summary>
        public TransactionScopeContainer() => AmbientTransactionContext.SetCurrent(_transactionContext);
		public TransactionScopeContainer(IAppContext ctx)
		{
			_transactionContext.Context = ctx;
			AmbientTransactionContext.SetCurrent(_transactionContext);
		}

		/// <summary>
		/// Gets the transaction context instance that this scope is holding
		/// </summary>
		public ITransactionContext TransactionContext => _transactionContext;

        /// <summary>
        /// Ends the current transaction by either committing it or aborting it, depending on whether someone voted for abortion
        /// </summary>
        public Task CompleteAsync() => _transactionContext.Complete();

        /// <summary>
        /// Ends the current transaction by either committing it or aborting it, depending on whether someone voted for abortion (synchronous version)
        /// </summary>
        public void Complete() => AsyncHelpers.RunSync(() => _transactionContext.Complete());

        /// <summary>
        /// Disposes the transaction context and removes it from <see cref="AmbientTransactionContext.Current"/> again
        /// </summary>
        public void Dispose()
        {
            try
            {
                _transactionContext.Dispose();
            }
            finally
            {
                AmbientTransactionContext.SetCurrent(_previousTransactionContext);
            }
        }
    }
    static class AsyncHelpers
    {
        /// <summary>
        /// Executes a task synchronously on the calling thread by installing a temporary synchronization context that queues continuations
        ///  </summary>
        public static void RunSync(Func<Task> task)
        {
            var currentContext = SynchronizationContext.Current;
            var customContext = new CustomSynchronizationContext(task);

            try
            {
                SynchronizationContext.SetSynchronizationContext(customContext);

                customContext.Run();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(currentContext);
            }
        }

		/// <summary>
		/// Synchronization context that can be "pumped" in order to have it execute continuations posted back to it
		/// </summary>
		class CustomSynchronizationContext : SynchronizationContext
		{
			readonly ConcurrentQueue<Tuple<SendOrPostCallback, object>> _items = new ConcurrentQueue<Tuple<SendOrPostCallback, object>>();
			readonly AutoResetEvent _workItemsWaiting = new AutoResetEvent(false);
			readonly Func<Task> _task;

			ExceptionDispatchInfo _caughtException;

			bool _done;

			public CustomSynchronizationContext(Func<Task> task)
			{
				if (task == null) throw new ArgumentNullException(nameof(task), "Please remember to pass a Task to be executed");
				_task = task;
			}

			public override void Post(SendOrPostCallback function, object state)
			{
				_items.Enqueue(Tuple.Create(function, state));
				_workItemsWaiting.Set();
			}

			/// <summary>
			/// Enqueues the function to be executed and executes all resulting continuations until it is completely done
			/// </summary>
			public void Run()
			{
				Post(async _ =>
				{
					try
					{
						await _task();
					}
					catch (Exception exception)
					{
						_caughtException = ExceptionDispatchInfo.Capture(exception);
						throw;
					}
					finally
					{
						Post(state => _done = true, null);
					}
				}, null);

				while (!_done)
				{
					Tuple<SendOrPostCallback, object> task;

					if (_items.TryDequeue(out task))
					{
						task.Item1(task.Item2);

						if (_caughtException == null) continue;

						_caughtException.Throw();
					}
					else
					{
						_workItemsWaiting.WaitOne();
					}
				}
			}

			public override void Send(SendOrPostCallback d, object state)
			{
				throw new NotSupportedException("Cannot send to same thread");
			}

			public override SynchronizationContext CreateCopy()
			{
				return this;
			}
		}
	}
}