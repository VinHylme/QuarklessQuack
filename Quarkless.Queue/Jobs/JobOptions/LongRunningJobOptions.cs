using Quarkless.Queue.Interfaces.Jobs;
using QuarklessContexts.Models.Requests;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Queue.Jobs.JobOptions
{
	public enum RequestType
	{
		POST,
		GET,
		PATCH,
		PUT,
		DELETE
	}
	public class UserStore
	{
		public string AccountId { get; set; }
		public string InstaAccountId { get; set; }
		public string AccessToken { get; set; }
		public UserStore(string accountId, string instaAccountId, string accessToken = null)
		{
			this.AccountId = accountId;
			this.InstaAccountId = instaAccountId;
			this.AccessToken = accessToken;
		}
	}
	public class RestModel
	{
		public UserStore User { get; set; }
		public string BaseUrl { get; set; }
		public string JsonBody { get; set; }
		public string ResourceAction { get; set; }
		public IEnumerable<Parameter> Parameters { get; set; }
		public RequestType RequestType { get; set; }
		public List<HttpHeader> RequestHeaders { get; set; }

		public RestModel()
		{
			RequestHeaders = new List<HttpHeader>();
		}

	}
	public class LongRunningJobOptions : IJobOptions
	{
		public RestModel Rest { get; set; }
		public DateTimeOffset ExecutionTime { get; set; }
	}
}
