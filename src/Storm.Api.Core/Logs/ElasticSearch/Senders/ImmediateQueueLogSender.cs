using System.Threading.Tasks;
using Storm.Api.Core.Logs.Senders;

namespace Storm.Api.Core.Logs.ElasticSearch.Senders
{
	internal class ImmediateQueueLogSender : AbstractImmediateQueueLogSender
	{
		private readonly IElasticSender _client;

		public ImmediateQueueLogSender(ILogService service, IElasticSender client) : base(service)
		{
			_client = client;
		}

		protected override Task Send(string entry)
		{
			return _client.Send(entry);
		}
	}
}