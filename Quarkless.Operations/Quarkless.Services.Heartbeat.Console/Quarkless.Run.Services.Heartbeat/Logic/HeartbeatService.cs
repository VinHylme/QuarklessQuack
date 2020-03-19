using System;
using System.Threading.Tasks;
using Quarkless.Base.ContentSearch.Models.Interfaces;
using Quarkless.Base.Heartbeat.Models.Interfaces;
using Quarkless.Base.InstagramAccounts.Models;
using Quarkless.Base.InstagramAccounts.Models.Enums;
using Quarkless.Base.InstagramAccounts.Models.Interfaces;
using Quarkless.Base.Profile.Models.Interfaces;
using Quarkless.Base.Proxy.Models.Interfaces;
using Quarkless.Base.Topic.Models.Interfaces;
using Quarkless.Base.WorkerManager.Models.Interfaces;
using CustomerAccount = Quarkless.Run.Services.Heartbeat.Models.CustomerAccount;
using ExtractOperationType = Quarkless.Run.Services.Heartbeat.Models.Enums.ExtractOperationType;
using FullUserDetail = Quarkless.Run.Services.Heartbeat.Models.FullUserDetail;
using IHeartbeatService = Quarkless.Run.Services.Heartbeat.Models.Interfaces.IHeartbeatService;

namespace Quarkless.Run.Services.Heartbeat.Logic
{
	public class HeartbeatService : IHeartbeatService
	{
		private readonly IProfileLogic _profileLogic;
		private readonly IProxyLogic _proxyLogic;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private readonly ITopicLookupLogic _topicLookup;
		private readonly ISearchProvider _searchProvider;
		private readonly IWorkerManager _workerManager;
		public HeartbeatService(IProfileLogic profileLogic,
			IProxyLogic proxyLogic, IHeartbeatLogic heartbeatLogic,
			ISearchProvider searchProvider, IInstagramAccountLogic accountLogic,
			ITopicLookupLogic topicLookup, IWorkerManager workerManager)
		{
			_profileLogic = profileLogic;
			_proxyLogic = proxyLogic;
			_heartbeatLogic = heartbeatLogic;
			_instagramAccountLogic = accountLogic;
			_topicLookup = topicLookup;
			_searchProvider = searchProvider;
			_workerManager = workerManager;
		}

		/// <summary>
		/// Get full details for a given account > e.g. profile etc
		/// </summary>
		/// <param name="instagramAccount"></param>
		/// <returns></returns>
		private async Task<FullUserDetail> GetFullUserDetails(ShortInstagramAccountModel instagramAccount)
		{
			return new FullUserDetail
			{
				InstagramAccount = instagramAccount,
				Profile = await _profileLogic.GetProfile(instagramAccount.AccountId, instagramAccount.Id),
				ProxyUsing = await _proxyLogic.GetProxyAssigned(instagramAccount.AccountId, instagramAccount.Id)
			};
		}

		public async Task Start(CustomerAccount customer, ExtractOperationType operation)
		{
			var currentAccount = await _instagramAccountLogic.GetInstagramAccountShort(customer.UserId, customer.InstagramAccountId);
			if (currentAccount?.AgentState == null) return;

			switch ((AgentState)currentAccount.AgentState)
			{
				case AgentState.Running:
				case AgentState.Blocked:
				case AgentState.Sleeping:
				case AgentState.Challenge:
					break;
				case AgentState.AwaitingActionFromUser:
				case AgentState.DeepSleep:
				case AgentState.NotStarted:
				case AgentState.NotWakeTime:
					return;
			}

			await PerformTask(await GetFullUserDetails(currentAccount), operation);
		}

		private async Task PerformTask(FullUserDetail customer, ExtractOperationType operation)
		{
			var metaBuilder = new MetadataBuilderManager(customer, _heartbeatLogic, _workerManager,
				_searchProvider, _instagramAccountLogic, _topicLookup);

			switch (operation)
			{
				case ExtractOperationType.Base:
					await metaBuilder.BaseExtract();
					Console.WriteLine("Waiting before next cycle...");
					await Task.Delay(TimeSpan.FromMinutes(10));
					break;
				case ExtractOperationType.External:
					await metaBuilder.ExternalExtract();
					Console.WriteLine("Waiting before next cycle...");
					await Task.Delay(TimeSpan.FromMinutes(8));
					break;
				case ExtractOperationType.UserInfo:
					await metaBuilder.UserInformationExtract();
					Console.WriteLine("Waiting before next cycle...");
					await Task.Delay(TimeSpan.FromMinutes(1.35));
					break;
				case ExtractOperationType.TargetListing:
					await metaBuilder.TargetListingExtract();
					Console.WriteLine("Waiting before next cycle...");
					await Task.Delay(TimeSpan.FromMinutes(10));
					break;
			}
			Console.WriteLine("Finished action {0} with customer {1}", operation.ToString(), customer.InstagramAccount.Username);
		}
	}
}
