using InstagramApiSharp.Classes.Models;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessLogic.Handlers.Util;
using QuarklessRepositories.Repository.ServicesRepositories;
using QuarklessRepositories.Repository.ServicesRepositories.CommentsRepository;
using QuarklessRepositories.Repository.ServicesRepositories.HashtagsRepository;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Quarkless.Worker.Actions
{
	public class HashtagsAction : IWActions
	{
		private readonly string Topic;
		private readonly int Limit;
		private readonly IAPIClientContainer context_;
		private readonly IReportHandler _reportHandler;
		private readonly IHashtagsRepository _hashtagsRepository;
		public HashtagsAction(IAPIClientContainer context, IReportHandler reportHandler, string topic, int limit, params IServiceRepository[] serviceRepository)
		{
			this.Topic = topic;
			this.Limit = limit;
			this.context_ = context;
			this._reportHandler = reportHandler;

			foreach (var repo in serviceRepository)
			{
				if (repo is IHashtagsRepository)
					this._hashtagsRepository = (IHashtagsRepository)repo;
			}

			_reportHandler.SetupReportHandler("CommentActions", context.GetContext.InstagramAccount.AccountId, context.GetContext.InstagramAccount.Username);
		}
		public Task<object> Operate()
		{
			throw new NotImplementedException();
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
