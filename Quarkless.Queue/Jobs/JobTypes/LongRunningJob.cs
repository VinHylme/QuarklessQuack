using Quarkless.Queue.Jobs.JobOptions;
using QuarklessContexts.Enums;
using QuarklessContexts.JobClass;
using QuarklessLogic.Handlers.RestSharpClient;
using System;
using System.Diagnostics;
using System.Threading;

namespace Quarkless.Queue.Jobs.JobTypes
{
	public static class LongRunningJobExtension
	{
		public static string AddScheduledJob(this IJobRunner jobRunner, Action<LongRunningJobOptions> configureJob)
		{
			 return jobRunner.Queue<LongRunningJob, LongRunningJobOptions>(configureJob);
		}
	}

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
				case RequestType.POST:
					 _restSharpClient.PostRequest(jobOptions.Rest.BaseUrl, jobOptions.Rest.ResourceAction, jobOptions.Rest.JsonBody,
						jobOptions.Rest.User, jobOptions.Rest.Parameters, jobOptions.Rest.RequestHeaders);
					break;
				case RequestType.GET:
					 _restSharpClient.GetRequest(jobOptions.Rest.BaseUrl,jobOptions.Rest.ResourceAction,jobOptions.Rest.Parameters,jobOptions.Rest.RequestHeaders);
					break;
			}
			return;
		}
	}
}
