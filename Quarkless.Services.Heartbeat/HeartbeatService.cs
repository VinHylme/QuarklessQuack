using System;
using System.Threading.Tasks;
using QuarklessContexts.Enums;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.SearchProvider;
using QuarklessLogic.Handlers.WorkerManagerService;
using QuarklessLogic.Logic.InstagramAccountLogic;
using QuarklessLogic.Logic.ProfileLogic;
using QuarklessLogic.Logic.ProxyLogic;
using QuarklessLogic.Logic.TopicLookupLogic;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;

namespace Quarkless.Services.Heartbeat
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
			IProxyLogic proxyLogic,  IHeartbeatLogic heartbeatLogic, 
			ISearchProvider searchProvider, IAPIClientContext context,
			IInstagramAccountLogic accountLogic, ITopicLookupLogic topicLookup)
		{
			_profileLogic = profileLogic;
			_proxyLogic = proxyLogic;
			_heartbeatLogic = heartbeatLogic;
			_instagramAccountLogic = accountLogic;
			_topicLookup = topicLookup;
			_searchProvider = searchProvider;
			_workerManager = new WorkerManager(context, _instagramAccountLogic, 2, 1);
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
				ProxyUsing = await _proxyLogic.GetProxyAssignedTo(instagramAccount.AccountId, instagramAccount.Id)
			};
		}

		public async Task Start(CustomerAccount customer, ExtractOperationType operation)
		{
			var currentAccount = await _instagramAccountLogic.GetInstagramAccountShort(customer.UserId, customer.InstagramAccountId);
			if (currentAccount == null)
				return;
			if (currentAccount.AgentState != (int) AgentState.Running)
				return;

			await PerformTask(await GetFullUserDetails(currentAccount), operation);
		}

		private async Task PerformTask(FullUserDetail customer, ExtractOperationType operation)
		{
			var metaBuilder = new MetadataBuilderManager(customer, _heartbeatLogic, _workerManager, 
				_searchProvider, _instagramAccountLogic,_topicLookup);

			switch (operation)
			{
				case ExtractOperationType.Base:
					await metaBuilder.BaseExtract();
					Console.WriteLine("Waiting before next cycle...");
					await Task.Delay(TimeSpan.FromMinutes(4));
					break;
				case ExtractOperationType.External:
					await metaBuilder.ExternalExtract();
					Console.WriteLine("Waiting before next cycle...");
					await Task.Delay(TimeSpan.FromMinutes(3));
					break;
				case ExtractOperationType.UserInfo:
					await metaBuilder.UserInformationExtract(); 
					Console.WriteLine("Waiting before next cycle...");
					await Task.Delay(TimeSpan.FromMinutes(1));
					break;
				case ExtractOperationType.TargetListing:
					await metaBuilder.TargetListingExtract();
					Console.WriteLine("Waiting before next cycle...");
					await Task.Delay(TimeSpan.FromMinutes(5));
					break;
			}
			Console.WriteLine("Finished action {0} with customer {1}", operation.ToString(), customer.InstagramAccount.Username);
		}
	}
}
