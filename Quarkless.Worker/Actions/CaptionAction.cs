using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessLogic.Handlers.Util;
using QuarklessRepositories.Repository.ServicesRepositories;
using QuarklessRepositories.Repository.ServicesRepositories.CaptionsRepository;
using QuarklessRepositories.Repository.ServicesRepositories.HashtagsRepository;

namespace Quarkless.Worker.Actions
{
	public class CaptionAction : IWActions
	{
		private readonly string Topic;
		private readonly IAPIClientContainer context_;
		private readonly IReportHandler _reportHandler;
		private readonly ICaptionsRepository _captionsRepository;
		private readonly IHashtagsRepository _hashtagsRepository;
		public CaptionAction(IAPIClientContainer context, IReportHandler reportHandler, string topic, int limit, params IServiceRepository[] serviceRepository)
		{
			this.Topic = topic;
			this.context_ = context;
			this._reportHandler = reportHandler;

			foreach (var repo in serviceRepository)
			{
				if (repo is ICaptionsRepository)
					this._captionsRepository = (ICaptionsRepository)repo;
				if (repo is IHashtagsRepository)
					this._hashtagsRepository = (IHashtagsRepository)repo;
			}

			_reportHandler.SetupReportHandler("CaptionActions", context.GetContext.InstagramAccount.AccountId, context.GetContext.InstagramAccount.Username);
		}
		public Task<object> Operate()
		{
			throw new NotImplementedException();
		}

		public async Task<bool> Operate(List<object> medias)
		{
			try
			{
				List<CaptionsModel> Captions = new List<CaptionsModel>();
				List<HashtagsModel> Hashtags = new List<HashtagsModel>();

				Console.WriteLine("BEGINING CAPTIONS");
				foreach (InstaMedia media in medias)
				{
					CaptionsModel caption = new CaptionsModel();
					string caption_ext = media.Caption?.Text ?? string.Empty;
					if (media.Caption == null) { continue; }
					if(media.Caption.Text.Count(_ => _ == '#') > 1)
					{
						var ext = media.Caption.Text.Split('#').ToList();
						
						caption_ext = ext.ElementAtOrDefault(0);
						if (ext.Count > 1) { 
							List<string> hashtags_ext = new List<string>();
							for(int i = 1; i < ext.Count-1; i++)
							{
								hashtags_ext.Add(ext[i]);
							}
							if (hashtags_ext.Count > 0) { 
								Hashtags.Add(new HashtagsModel
								{
									Hashtags = hashtags_ext.Select(a => a).ToList(),
									Topic = this.Topic,
								});
							}
						}
						else { 
							continue;
						}
					}
					caption.DateCreated = media.Caption.CreatedAtUtc;
					caption.MediaId = media.Pk;
					caption.NumberOfLikesOnMedia = media.LikesCount;
					caption.Topic = this.Topic;
					caption.User_Id = media.User.Pk;
					caption.User_Username = media.User.UserName;
					caption.Text = caption_ext;
					Captions.Add(caption);
				}
				bool addCaption = false;
				bool addHashtags = false;
				if (Captions.Count > 0) {
					addCaption = await _captionsRepository.AddCaptions(Captions);
				}
				if (Hashtags.Count > 0) {
					addHashtags = await _hashtagsRepository.AddHashtags(Hashtags);
				}
				return addCaption&&addHashtags;
			}
			catch(Exception ee)
			{
				Console.WriteLine("failed caption");
				_reportHandler.MakeReport(ee);
				return false;
			}
		}

		public async Task<bool> Operate(object item)
		{
			try
			{
				List<CaptionsModel> Captions = new List<CaptionsModel>();
				List<HashtagsModel> Hashtags = new List<HashtagsModel>();
				InstaMedia media = (InstaMedia) item;
				Console.WriteLine("BEGINING CAPTIONS");
				CaptionsModel caption = new CaptionsModel();
				string caption_ext = media.Caption?.Text ?? string.Empty;

				if (media.Caption.Text.Count(_ => _ == '#') > 1)
				{
					var ext = media.Caption.Text.Split('#').ToList();

					caption_ext = ext.ElementAtOrDefault(0);
					if (ext.Count > 1)
					{
						List<string> hashtags_ext = new List<string>();
						for (int i = 1; i < ext.Count - 1; i++)
						{
							hashtags_ext.Add(ext[i]);
						}
						if (hashtags_ext.Count > 0) { 
							Hashtags.Add(new HashtagsModel
							{
								Hashtags = hashtags_ext.Select(a => a).ToList(),
								Topic = this.Topic,
							});
						}
					}
				}
				caption.DateCreated = media.Caption.CreatedAtUtc;
				caption.MediaId = media.Pk;
				caption.NumberOfLikesOnMedia = media.LikesCount;
				caption.Topic = this.Topic;
				caption.User_Id = media.User.Pk;
				caption.User_Username = media.User.UserName;
				caption.Text = caption_ext;
				Captions.Add(caption);
				
				bool addCaption = false;
				bool addHashtags = false;
				if (Captions.Count > 0)
				{
					addCaption = await _captionsRepository.AddCaptions(Captions);
				}
				if (Hashtags.Count > 0)
				{
					addHashtags = await _hashtagsRepository.AddHashtags(Hashtags);
				}
				return addCaption && addHashtags;
			}
			catch (Exception ee)
			{
				Console.WriteLine("failed caption");
				_reportHandler.MakeReport(ee);
				return false;
			}
		}
	}
}
