using System;
using System.Threading.Tasks;
using QuarklessContexts.Enums;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessLogic.ContentSearch.GoogleSearch;
using QuarklessLogic.ContentSearch.YandexSearch;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Logic.InstagramAccountLogic;
using QuarklessLogic.Logic.ProfileLogic;
using QuarklessLogic.Logic.ProxyLogic;
using QuarklessLogic.Logic.ResponseLogic;
using QuarklessLogic.Logic.TopicLookupLogic;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;

namespace Quarkless.Services.Heartbeat
{
	public class HeartbeatService : IHeartbeatService
	{
		// each user account is assigned a proxy
		// scraping websites require additional scrapping proxies (could do with connecting them here)
		private readonly IProfileLogic _profileLogic;
		private readonly IProxyLogic _proxyLogic;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly IAPIClientContext _context;
		private readonly IResponseResolver _responseResolver;
		private readonly IGoogleSearchLogic _googleSearchLogic;
		private readonly IYandexImageSearch _yandexImageSearch;
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private readonly ITopicLookupLogic _topicLookup;

		public HeartbeatService(IProfileLogic profileLogic, 
			IProxyLogic proxyLogic, IAPIClientContext context, IHeartbeatLogic heartbeatLogic,
			IResponseResolver responseResolver,
			IGoogleSearchLogic googleSearchLogic, IYandexImageSearch yandexImageSearch,
			IInstagramAccountLogic accountLogic, ITopicLookupLogic topicLookup)
		{
			_profileLogic = profileLogic;
			_proxyLogic = proxyLogic;
			_context = context;
			_heartbeatLogic = heartbeatLogic;
			_responseResolver = responseResolver;
			_googleSearchLogic = googleSearchLogic;
			_yandexImageSearch = yandexImageSearch;
			_instagramAccountLogic = accountLogic;
			_topicLookup = topicLookup;
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

		public async Task Start(CustomerAccount customer, WorkerAccount worker, ExtractOperationType operation)
		{
			var currentAccount = await _instagramAccountLogic.GetInstagramAccountShort(customer.UserId, customer.InstagramAccountId);
			if (currentAccount == null)
				return;
			if (currentAccount.AgentState != (int) AgentState.Running)
				return;

			var workerAccount =
				await _instagramAccountLogic.GetInstagramAccountShort(worker.UserId, worker.InstagramAccountId);
			
			if (workerAccount == null)
				return;
			
			await PerformTask(await GetFullUserDetails(currentAccount), new Worker{InstagramAccount = workerAccount}, operation);
		}

		private async Task PerformTask(FullUserDetail customer, Worker assignWorker, ExtractOperationType operation)
		{
			var assignment = new Assignment
			{
				Customer = customer,
				CustomerTopic = customer.Profile.ProfileTopic,
				Worker = assignWorker
			};
			var metaBuilder = new MetadataBuilderManager(assignment, _context, _heartbeatLogic, _responseResolver,
				_googleSearchLogic, _yandexImageSearch, _instagramAccountLogic,_topicLookup);

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
