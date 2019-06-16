using Quarkless.Services;
using Quarkless.Services.PostServices;
using Quarkless.Worker.Actions;
using Quarkless.Worker.Models;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessLogic.Handlers.Util;
using QuarklessLogic.Logic.DiscoverLogic;
using QuarklessLogic.Logic.InstagramAccountLogic;
using QuarklessRepositories.RepositoryClientManager;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quarkless.Worker.WorkerPool
{
	public class Worker : IWorker
	{
		
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private readonly IAPIClientContext _aPIClientContext;
		private readonly IReportHandler _reportHandler;
		private readonly IRepositoryContext _repositoryContext;
		private readonly IUtilProviders _util;
		public Worker(IInstagramAccountLogic instagramAccountLogic, IRepositoryContext repositoryContext,
			IAPIClientContext aPIClientContext, IUtilProviders utilproviders, IReportHandler reportHandler)
		{
			this._instagramAccountLogic = instagramAccountLogic;
			this._aPIClientContext = aPIClientContext;
			this._util = utilproviders;
			this._reportHandler = reportHandler;
			this._repositoryContext = repositoryContext;
			_reportHandler.SetupReportHandler("Worker");
		}

		public async Task<bool> Begin(SchedulerSettings schedulerSettings)
		{
			try
			{
				foreach(var account in schedulerSettings.Accounts)
				{
					var instaAccounts = await GetAccount(account.AccountId);
					foreach (var insta_account in instaAccounts)
					{
						account.APIClients.Enqueue(new APIClientContainer(_aPIClientContext, insta_account.AccountId, insta_account._id));
					}
					var scheduler = new Scheduler(account.APIClients, _util , _repositoryContext, _reportHandler);
					scheduler.Schedule(schedulerSettings.TimerSettings, schedulerSettings.ExecutionSettings, schedulerSettings.ActionExecuters.ToArray());
				}

				return true;
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return false;
			}
		}
		public async Task<IEnumerable<InstagramAccountModel>> GetAccount(params string[] accounts)
		{
			List<InstagramAccountModel> accs = new List<InstagramAccountModel>();
			foreach (var account in accounts)
			{
				var resp = await _instagramAccountLogic.GetInstagramAccountsOfUser(account, 1);
				if (resp != null && resp.Count() > 0)
				{
					foreach (var acc in resp)
					{
						accs.Add(acc);
					}
				}
				continue;
			}
			return accs;
		}
	}
}
