using InstagramApiSharp;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessLogic.Handlers.Util;
using QuarklessRepositories.Repository.ServicesRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quarkless.Worker.Actions
{
	public class MediaAction : IWActions
	{
		private readonly string Topic;
		private readonly int Limit;
		private readonly IAPIClientContainer context_;
		private readonly IReportHandler _reportHandler;
		private readonly IPostServicesRepository _postServicesRepository;
		public MediaAction(IAPIClientContainer context, IReportHandler reportHandler, string topic, int limit, params IServiceRepository[] serviceRepository)
		{
			this.Topic = topic;
			this.Limit = limit;
			this.context_ = context;
			this._reportHandler = reportHandler;

			foreach (var repo in serviceRepository)
			{
				if (repo is IPostServicesRepository)
					this._postServicesRepository = (IPostServicesRepository)repo;
			}
		}

		public async Task<object> Operate()
		{
			Console.WriteLine("BEGINING MEDIA");

			var res = await context_.Hashtag.GetTopHashtagMediaListAsync(Topic, PaginationParameters.MaxPagesToLoad(Limit));
			if (res.Succeeded)
			{
				var medias = res.Value.Medias;
				var mediaObject = medias.Select(_=>(object)_).ToList();
				return mediaObject;
			}
			else
			{
				if(res.Info.ResponseType == ResponseType.RequestsLimit || res.Info.ResponseType == ResponseType.NetworkProblem)
				{
					//await Task.Delay(TimeSpan.FromMinutes(2));
					return false;
				}
				Console.WriteLine("failed media");
				_reportHandler.MakeReport(res.Info);
				return null;
			}
		}

		public Task<bool> Operate(List<object> medias)
		{
			throw new NotImplementedException();
		}

		public Task<bool> Operate(object item)
		{
			throw new NotImplementedException();
		}
	}
}
