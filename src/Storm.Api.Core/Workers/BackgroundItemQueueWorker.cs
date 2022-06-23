using System;
using System.Threading.Tasks;
using Storm.Api.Core.Logs;

namespace Storm.Api.Core.Workers
{
	public class BackgroundItemQueueWorker<TWorkItem> : BackgroundItemWorker<TWorkItem>, IWorker<TWorkItem>
		where TWorkItem : class
	{
		public BackgroundItemQueueWorker(ILogService logService, Func<TWorkItem, Task> itemAction, bool shouldLogFailure, int nbMaxTry)
			: base(logService, itemAction, shouldLogFailure, nbMaxTry)
		{
		}
	}
}