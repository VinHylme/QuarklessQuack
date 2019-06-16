using InstagramApiSharp.Classes.Models;
using Quarkless.Worker.Actions;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessLogic.Handlers.Util;
using QuarklessRepositories.Repository.ServicesRepositories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Quarkless.Worker.TaskScheduler
{
	public enum WorkerType
	{
		Fetcher,
		Extracter
	}
	public enum WorkerStatus
	{
		Idle,
		Running,
		Error,
	}
	public class WorkerAPI
	{
		public Guid _IdIndex { get; set; }
		public WorkerType _WorkerType { get; set; }
		public IAPIClientContainer _worker { get; set; }
		public WorkerAPI(IAPIClientContainer aPI, WorkerType worktype, Guid guid = default(Guid))
		{
			_worker = aPI;
			this._WorkerType = worktype;
			if(guid==default(Guid))
				this._IdIndex = Guid.NewGuid();
		}

		public async Task<object> Execute(ActionType action_, IReportHandler reportHandler, string topic, int limit, List<object> items, params IServiceRepository[] serviceRepositories)
		{
			var action = WActions.InitializeFactories.Execute(action_, _worker, reportHandler, topic, limit, serviceRepositories);
			if(items!=null)
				return await action.Operate(items);
			else
				return await action.Operate();
		}
		public async Task<object> Execute(ActionType action_, IReportHandler reportHandler, string topic, int limit, object items, params IServiceRepository[] serviceRepositories)
		{
			var action = WActions.InitializeFactories.Execute(action_, _worker, reportHandler, topic, limit, serviceRepositories);
			if (items != null)
				return await action.Operate(items);
			else
				return await action.Operate();
		}
	}
}
