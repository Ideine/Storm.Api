using System;
using System.Threading.Tasks;
using Storm.Api.Core.Logs;

namespace Storm.Api.Core.Workers
{
	public class BackgroundItemQueueWorker<TWorkItem> : BackgroundItemWorker<TWorkItem>, IWorker<TWorkItem>
		where TWorkItem : class
	{
		public BackgroundItemQueueWorker(ILogService logService, Func<TWorkItem, Task<bool>> itemAction, int? discardAfterFailAttemptsCount = 1)
			: base(logService, itemAction, discardAfterFailAttemptsCount)
		{
		}
	}
}