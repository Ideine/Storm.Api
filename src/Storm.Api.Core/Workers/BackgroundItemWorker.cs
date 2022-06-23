using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Storm.Api.Core.Extensions;
using Storm.Api.Core.Logs;

namespace Storm.Api.Core.Workers
{
	public abstract class BackgroundItemWorker<TWorkItem> : IWorker<TWorkItem>
		where TWorkItem : class
	{
		private readonly ILogService _logService;
		private readonly BackgroundWorker _worker;

		private readonly ConcurrentQueue<(TWorkItem, ExecHistory)> _waitingQueue = new ConcurrentQueue<(TWorkItem, ExecHistory)>();
		private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);

		private readonly Func<TWorkItem, Task> _itemAction;

		private readonly int _nbMaxRetry;
		private readonly int _baseRetryDelayMs;
		private readonly bool _shouldLogFailure;

		protected BackgroundItemWorker(ILogService logService, Func<TWorkItem, Task> itemAction, bool shouldLogFailure, int nbMaxTry, int baseRetryDelayMs = 5000)
		{
			_logService = logService;
			_itemAction = itemAction;

			_nbMaxRetry = nbMaxTry;
			_baseRetryDelayMs = baseRetryDelayMs;
			_shouldLogFailure = shouldLogFailure;
			_worker = new BackgroundWorker(_logService, RunAsync);
			_worker.Start();
		}

		public void Queue(TWorkItem item) => Queue(item, new ExecHistory(capacity: _nbMaxRetry));

		private void Queue(TWorkItem item, ExecHistory execHistory)
		{
			_waitingQueue.Enqueue((item, execHistory));
			_semaphore.Release();
			_worker.Start();
		}

		private async Task RunAsync(CancellationToken ct)
		{
			while (true)
			{
				await _semaphore.WaitAsync(ct);
				ct.ThrowIfCancellationRequested();
				if (_waitingQueue.TryDequeue(out (TWorkItem, ExecHistory) tuple))
				{
					try
					{
						(var item, var execHistory) = tuple;
						try
						{
							await _itemAction.Invoke(item);
							OnItemSuccess(item);
						}
						catch (Exception ex)
						{
							execHistory.ExceptionList.Add(ex);
							if (execHistory.ExceptionList.Count < _nbMaxRetry)
							{
								ReEnqueue(
									item:             item,
									executionHistory: execHistory,
									delayMs:          ExponentialDelayMs(
										baseMs:         _baseRetryDelayMs,
										retryIteration: execHistory.ExceptionList.Count
									)
								);
							}
							else 
							{
								if (_shouldLogFailure)
								{
									_logService.Error(x => x
										.WriteProperty("type", GetType().FullName)
										.WriteMethodInfo()
										.WriteException(
											property: "exceptions",
											exception: new AggregateException(execHistory.ExceptionList)
										)
										.DumpObject("item", item)
									);
								}
								await OnItemError(item);
							}
						}
					}
					finally { _semaphore.Release(); }
				}
			}
		}

		private async void ReEnqueue(TWorkItem item, ExecHistory executionHistory, int delayMs)
		{
			try
			{
				await Task.Delay(delayMs);
				Queue(item, executionHistory);
			}
			catch { /* faily silently because of async void */ }
		}

		private static int ExponentialDelayMs(int baseMs, int retryIteration) => retryIteration * (retryIteration + 1) / 2 * baseMs;

		protected virtual void OnItemSuccess(TWorkItem item)
		{

		}

		protected virtual Task OnItemError(TWorkItem item)
		{
			return Task.CompletedTask;
		}

		#region Nested Class

		protected class ExecHistory
		{
			public ExecHistory(int capacity)
			{
				ExceptionList = new List<Exception>(capacity);
			}

			public List<Exception> ExceptionList { get; }
		}

		#endregion Nested Class
	}
}