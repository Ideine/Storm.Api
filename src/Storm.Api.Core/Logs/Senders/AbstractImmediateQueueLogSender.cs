using System.Threading.Tasks;
using Storm.Api.Core.Workers;

namespace Storm.Api.Core.Logs.Senders
{
	public abstract class AbstractImmediateQueueLogSender : ILogSender
	{
		private readonly BackgroundItemQueueWorker<string> _worker;

		protected AbstractImmediateQueueLogSender(ILogService logService)
		{
			_worker = new BackgroundItemQueueWorker<string>(
				logService,
				Send,
				shouldLogFailure: false,//false otherwhise it's an infinite loop.,
				nbMaxTry: 5
			);
		}

		public void Enqueue(LogLevel level, string entry)
		{
			_worker.Queue(entry);
		}

		protected abstract Task Send(string entry);
	}
}