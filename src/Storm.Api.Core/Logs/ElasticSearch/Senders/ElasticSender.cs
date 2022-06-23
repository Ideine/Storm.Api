using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net;

namespace Storm.Api.Core.Logs.ElasticSearch.Senders
{
	public interface IElasticSender
	{
		Task Send(string entry);
		Task Send(IReadOnlyList<string> entries);
	}

	internal class ElasticSender : IElasticSender
	{
		private readonly ElasticLowLevelClient _client;
		private readonly string _index;

		public ElasticSender(ElasticLowLevelClient client, string index)
		{
			_client = client;
			_index = index;
		}

		public async Task Send(string entry)
		{
			StringResponse result = await _client.IndexAsync<StringResponse>(_index, PostData.String(entry));

			if (!(result.HttpStatusCode is int statusCode) || statusCode < 200 || statusCode >= 300)
			{
				throw new System.Exception($"Log entry failed with {nameof(result.HttpStatusCode)}: {result.HttpStatusCode}");
			}
		}

		public async Task Send(IReadOnlyList<string> entries)
		{
			StringBuilder content = new StringBuilder(entries.Count * (200 + 75));

			foreach (string entry in entries)
			{
				content.Append($"{{\"index\":{{}}");
				content.Append("\n");
				content.Append(entry);
				content.Append("\n");
			}

			string body = content.ToString();
			StringResponse result = await _client.BulkAsync<StringResponse>(_index, PostData.String(body));

			if (!(result.HttpStatusCode is int statusCode) || statusCode < 200 || statusCode >= 300)
			{
				throw new System.Exception($"Log entry failed with {nameof(result.HttpStatusCode)}: {result.HttpStatusCode}");
			}
		}
	}
}