using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using System.Threading.Tasks;
using System.Timers;
using Dasync.Collections;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessLogic.ContentSearch.GoogleSearch;
using QuarklessLogic.ContentSearch.YandexSearch;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Logic.InstagramAccountLogic;
using QuarklessLogic.Logic.ProfileLogic;
using QuarklessLogic.Logic.ProxyLogic;
using QuarklessLogic.Logic.ResponseLogic;
using QuarklessLogic.ServicesLogic.AgentLogic;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;

namespace Quarkless.Services.Heartbeat
{
	public class HeartbeatService : IHeartbeatService
	{
		// each user account is assigned a proxy
		// scraping websites require additional scrapping proxies (could do with connecting them here)
		private readonly IAgentLogic _agentLogic;
		private readonly IProfileLogic _profileLogic;
		private readonly IProxyLogic _proxyLogic;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly IAPIClientContext _context;
		private readonly IResponseResolver _responseResolver;
		private readonly IGoogleSearchLogic _googleSearchLogic;
		private readonly IYandexImageSearch _yandexImageSearch;
		private readonly IInstagramAccountLogic _instagramAccountLogic;

		//private readonly IMetadataExtract _metadataExtract;
		private ConcurrentQueue<FullUserDetail> Customers { get; set; }
		private ConcurrentQueue<FullUserDetail> WorkingCustomers { get; set; }
		private List<Worker> Workers { get; set; }
		public HeartbeatService(IAgentLogic agentLogic, IProfileLogic profileLogic, 
			IProxyLogic proxyLogic, IAPIClientContext context, IHeartbeatLogic heartbeatLogic,
			IResponseResolver responseResolver,
			IGoogleSearchLogic googleSearchLogic, IYandexImageSearch yandexImageSearch,
			IInstagramAccountLogic accountLogic)
		{
			_agentLogic = agentLogic;
			_profileLogic = profileLogic;
			_proxyLogic = proxyLogic;
			_context = context;
			_heartbeatLogic = heartbeatLogic;
			_responseResolver = responseResolver;
			_googleSearchLogic = googleSearchLogic;
			_yandexImageSearch = yandexImageSearch;
			_instagramAccountLogic = accountLogic;
			//_metadataExtract = metadataExtract;
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

		/// <summary>
		/// Checks for new accounts who have been registered as well if any status of accounts have changed
		/// </summary>
		/// <returns></returns>
		public async Task RefreshCustomerAccounts()
		{
			if(Customers==null || !Customers.Any())
				Customers = (await _agentLogic.GetAllAccounts())
					.Where(s=>s.AgentState == (int) AgentState.Running)
					.Select(async _ => await GetFullUserDetails(_)).Select(_ => _.Result).EnqueueAll();

			var newCustomerRequest = (await _agentLogic.GetAllAccounts()).ToList();
			var elemsToRemove = new List<ShortInstagramAccountModel>();

			foreach (var customer in newCustomerRequest)
			{
				// if there is a match check if the status has changed
				var find = Customers.SingleOrDefault(_ => _.InstagramAccount.Id.Equals(customer.Id));
				if (find != null)
				{
					if (customer.AgentState != (int)AgentState.Running)
					{
						elemsToRemove.Add(customer);
					}
				}
				else
				{
					if(customer.AgentState == (int) AgentState.Running 
						&& !WorkingCustomers.Any(c=>c.InstagramAccount.Equals(customer)))
						Customers.Enqueue(await GetFullUserDetails(customer));
				}
			}

			if(elemsToRemove.Any())
				Customers = new ConcurrentQueue<FullUserDetail>(Customers.Where(_=> elemsToRemove.All(x => x.Id != _.InstagramAccount.Id)));
		}

		/// <summary>
		/// Checks if there are any new worker accounts available on the database
		/// </summary>
		/// <returns></returns>
		public async Task RefreshWorkers()
		{
			if(Workers == null || !Workers.Any())
				Workers = (await _agentLogic.GetAllAccounts(1)).Select(_ => new Worker
				{
					InstagramAccount = _,
					IsCurrentlyAssigned = false
				}).ToList();

			var newWorkerRequests = await _agentLogic.GetAllAccounts(1);
			foreach (var worker in newWorkerRequests)
			{
				var find = Workers.SingleOrDefault(_ => _.InstagramAccount.Equals(worker));

				if(find==null)
					Workers.Add(new Worker
					{
						InstagramAccount = worker,
						IsCurrentlyAssigned = false
					});
			}
		}
		
		public async Task Start()
		{
			WorkingCustomers = new ConcurrentQueue<FullUserDetail>();
			//await RefreshCustomerAccounts();
			//await RefreshWorkers();

//			var checkCustomerTimer = new Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
//			checkCustomerTimer.Start();
//			checkCustomerTimer.Elapsed += async (o, e) => await RefreshCustomerAccounts();
//
//			var checkWorkersTimer = new Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
//			checkWorkersTimer.Start();
//			checkWorkersTimer.Elapsed += async (o, e) => await RefreshWorkers();

			while (true)
			{
				await RefreshCustomerAccounts();
				await RefreshWorkers();

				var assignWorker = Workers.FirstOrDefault(w => !w.IsCurrentlyAssigned);
				if (assignWorker == null)
					continue;
				Workers.ForEach(worker =>
				{
					if (worker.Equals(assignWorker))
						worker.IsCurrentlyAssigned = true;
				});
				Customers.TryDequeue(out var customer);
				if (customer == null)
					continue;

				WorkingCustomers.Enqueue(customer);
				PerformTask(customer, assignWorker);
				await Task.Delay(TimeSpan.FromSeconds(1));
			}
		}

		private void PerformTask(FullUserDetail customer, Worker assignWorker)
		{
			var assignment = new Assignment
			{
				Customer = customer,
				CustomerTopic = customer.Profile.Topics,
				Worker = assignWorker
			};
			var metaBuilder = new MetadataBuilderManager(assignment, _context, _heartbeatLogic, _responseResolver,
				_googleSearchLogic, _yandexImageSearch, _instagramAccountLogic);

			Task.Run(() =>
			{
				var baseExtract = metaBuilder.BaseExtract();
				var userInfo = metaBuilder.UserInformationExtract();
				var externalExtract = metaBuilder.ExternalExtract();
				var targetListing = metaBuilder.TargetListingExtract();

				Task.WaitAll(baseExtract, userInfo, externalExtract, targetListing);
			}).ContinueWith(async c =>
			{
				await Task.Delay(TimeSpan.FromMinutes(5)); // Sleep for one minute before requeue
				Console.WriteLine("Finished with customer {0}", customer.InstagramAccount.Username);
				Customers.Enqueue(customer);
				WorkingCustomers = new ConcurrentQueue<FullUserDetail>
					(WorkingCustomers.Where(x => x != customer));
				Workers.ForEach(worker =>
				{
					if (worker.Equals(assignWorker))
						worker.IsCurrentlyAssigned = false;
				});
			});
		}
	}
}
