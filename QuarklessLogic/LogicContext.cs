using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessLogic.Logic.CollectionsLogic;
using QuarklessLogic.Logic.CommentLogic;
using QuarklessLogic.Logic.DiscoverLogic;
using QuarklessLogic.Logic.HashtagLogic;
using QuarklessLogic.Logic.InstaAccountOptionsLogic;
using QuarklessLogic.Logic.InstaUserLogic;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessLogic
{
	public class LogicContext : ILogic
	{
		#region RegisterLogics
		private readonly IDiscoverLogic _discoverLogic;
		private readonly ICommentLogic _commentLogic;
		private readonly IHashtagLogic _hashtagLogic;
		private readonly ICollectionsLogic _collectionsLogic;
		private readonly IInstaAccountOptionsLogic _instaAccountOptionsLogic;
		private readonly IInstaUserLogic _instaUserLogic;
		#endregion
		private readonly IAPIClientContainer aPIClient;
		private readonly IReportHandler reportHandler;

		public LogicContext(IAPIClientContainer clientContainer, IReportHandler reportHandler)
		{
			this.aPIClient = clientContainer;
			this.reportHandler = reportHandler;

			_discoverLogic = new DiscoverLogic(aPIClient,reportHandler);
			_commentLogic = new CommentLogic(reportHandler,aPIClient);
			_collectionsLogic = new CollectionsLogic(aPIClient, reportHandler);
			//_hashtagLogic = new HashtagLogic(reportHandler,aPIClient);
			_instaAccountOptionsLogic = new InstaAccountOptionsLogic(aPIClient, reportHandler);
			_instaUserLogic = new InstaUserLogic(aPIClient,reportHandler);

		}

		public IInstaUserLogic InstaUserLogic
		{
			get
			{
				return _instaUserLogic;
			}
		}
		public IInstaAccountOptionsLogic InstaAccountOptionsLogic
		{
			get
			{
				return _instaAccountOptionsLogic;
			}
		}
		public ICollectionsLogic CollectionsLogic
		{
			get
			{
				return _collectionsLogic;
			}
		}
		public IHashtagLogic HashtagLogic
		{
			get
			{
				return _hashtagLogic;
			}
		}
		public IDiscoverLogic DiscoverLogic
		{
			get
			{
				return _discoverLogic;
			}
		}
		public ICommentLogic CommentLogic
		{
			get
			{
				return _commentLogic;
			}
		}
	}
}
