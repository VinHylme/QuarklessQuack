using Quarkless.Queue.Interfaces.Jobs;
using Quarkless.Queue.Jobs.JobOptions;
using Quarkless.Queue.RestSharpClient;
using QuarklessContexts.Models.Requests;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Quarkless.Queue.Jobs.JobTypes
{
	public static class LongRunningJobExtension
	{
		public static void QueueLongRunningJob(this IJobRunner jobRunner, Action<LongRunningJobOptions> configureJob)
		{
			 jobRunner.Queue<LongRunningJob, LongRunningJobOptions>(configureJob);
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
			Console.WriteLine("LongRunningTask: Started");

			if(jobOptions.Rest==null) return;
			
			switch (jobOptions.Rest.RequestType)
			{
				case RequestType.POST:
					 _restSharpClient.PostRequest(jobOptions.Rest.BaseUrl, jobOptions.Rest.ResourceAction, jobOptions.Rest.JsonBody,
						jobOptions.Rest.User, jobOptions.Rest.Parameters,jobOptions.Rest.RequestHeaders);
					break;
				case RequestType.GET:
					 _restSharpClient.GetRequest(jobOptions.Rest.ResourceAction,jobOptions.Rest.Parameters,jobOptions.Rest.RequestHeaders);
					break;
			}

			var stopWatch = Stopwatch.StartNew();
			Thread.Sleep(TimeSpan.FromSeconds(jobOptions.ExecutionTime.Minute));

			Console.WriteLine($"LongRunningTask: Finished ({stopWatch.Elapsed})");
			return;
		}
	}
}
