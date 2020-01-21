using Quarkless.Models.Common.Enums;
using Quarkless.Models.RestSharpClientManager;
using Quarkless.Models.RestSharpClientManager.Interfaces;
using Quarkless.Models.Timeline.Interfaces.TaskScheduler;
using Quarkless.Models.Timeline.TaskScheduler;

namespace Quarkless.Logic.Timeline.TaskScheduler
{
	public class LongRunningJob : IJob<LongRunningJobOptions>
	{
		private readonly IRestSharpClientManager _restSharpClient;
		public LongRunningJob(IRestSharpClientManager restSharpClient)
		{
			_restSharpClient = restSharpClient;
		}

		public void Perform(LongRunningJobOptions jobOptions)
		{
			if(jobOptions.Rest==null) return;
			
			switch (jobOptions.Rest.RequestType)
			{
				case RequestType.Post:
					 _restSharpClient.PostRequest(jobOptions.Rest.BaseUrl, jobOptions.Rest.ResourceAction, jobOptions.Rest.JsonBody,
						new UserStore
						{
							OAccessToken = jobOptions.Rest.User.OAccessToken,
							OAccountId = jobOptions.Rest.User.OAccountId,
							OInstagramAccountUser = jobOptions.Rest.User.OInstagramAccountUser,
							OInstagramAccountUsername = jobOptions.Rest.User.OInstagramAccountUsername,
							ORefreshToken = jobOptions.Rest.User.ORefreshToken
						}, jobOptions.Rest.Parameters, jobOptions.Rest.RequestHeaders);
					break;
				case RequestType.Get:
					 _restSharpClient.GetRequest(jobOptions.Rest.BaseUrl,jobOptions.Rest.ResourceAction,
						 jobOptions.Rest.Parameters,jobOptions.Rest.RequestHeaders);
					break;
			}
			return;
		}
	}
}
