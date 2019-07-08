using Quarkless.Services.Interfaces;
using Quarkless.Services.Interfaces.Actions;
using Quarkless.Services.StrategyBuilders;
using QuarklessContexts.Models;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.ServicesModels.HeartbeatModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;
using Quarkless.MediaAnalyser;
using System.Collections.Generic;
using System.Linq;
using InstagramApiSharp.Classes.Models;
using MoreLinq;
using QuarklessLogic.Handlers.RequestBuilder.Consts;
using QuarklessContexts.Classes.Carriers;

namespace Quarkless.Services.ActionBuilders.MaintainActions
{
	public class AccountCheckerAction : IActionCommit
	{
		private readonly IContentManager _content;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly ProfileModel _profile;
		private UserStoreDetails _user;
		private AccountCheckerStrategySettings _accountCheckerStrategySettings;
		public AccountCheckerAction(IContentManager content, IHeartbeatLogic heartbeatLogic, ProfileModel profile)
		{
			_content = content;
			_heartbeatLogic = heartbeatLogic;
			_profile = profile;
		}
		public IActionCommit IncludeStrategy(IStrategySettings strategy)
		{
			_accountCheckerStrategySettings = strategy as AccountCheckerStrategySettings;
			return this;
		}

		public IActionCommit IncludeUser(UserStoreDetails userStoreDetails)
		{
			_user = userStoreDetails;
			return this;
		}
		struct ContainMedia
		{
			public byte[] mediaData;
			public string mediaId;
			public int mediaType;
		}
		public ResultCarrier<IEnumerable<TimelineEventModel>> Push(IActionOptions actionOptions)
		{
			ResultCarrier<IEnumerable<TimelineEventModel>> Results = new ResultCarrier<IEnumerable<TimelineEventModel>>();
			if (actionOptions != null)
			{
				var accountCheckerActionOptions = actionOptions as AccountCheckerActionOptions;
				var currentUsersMedia = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchUserOwnProfile, _profile.Topics.TopicFriendlyName, _profile.InstagramAccountId)
										.GetAwaiter().GetResult().ToList();

				//remove duplicates from user's gallery
				List<ContainMedia> usersMediaInBytes = new List<ContainMedia>();
				if (currentUsersMedia != null)
				{
					foreach(var userMedia in currentUsersMedia)
					{
						if (userMedia.HasValue)
						{
							var medias = userMedia.Value.ObjectItem.Medias;
							foreach(var umedia in medias)
							{
								foreach(var url in umedia.MediaUrl)
								{
									usersMediaInBytes.Add(new ContainMedia
									{
										mediaData = url.DownloadMedia(),
										mediaId = umedia.MediaId,
										mediaType = (int) umedia.MediaType
									});
								}
							}
						}
					}

					var images = usersMediaInBytes.Where(a => a.mediaType == (int)InstaMediaType.Image);
					var duplicates = images.Select(s=>s.mediaData).DuplicateImages();
					if(duplicates!=null && duplicates.Count() > 0) { 
						List<TimelineEventModel> tosend = new List<TimelineEventModel>();
						for (int x = 0; x < duplicates.Count(); x++)
						{
							RestModel restModel = new RestModel
							{
								BaseUrl = string.Format(UrlConstants.DeleteMedia,images.ElementAt(x).mediaId,images.ElementAt(x).mediaType),
								User = _user,
								RequestType = QuarklessContexts.Enums.RequestType.POST,
							};
							tosend.Add(new TimelineEventModel
							{
								ActionName = $"ActionChecker_{_accountCheckerStrategySettings.AccountCheckerStrategy.ToString()}",
								Data = restModel,
								ExecutionTime = accountCheckerActionOptions.ExecutionTime.AddMinutes((_accountCheckerStrategySettings.OffsetPerAction.TotalMinutes)) 
							});
						}
						Results.IsSuccesful = true;
						Results.Results = tosend;
						return Results;
					}
				}
				else
				{
					Results.IsSuccesful = false;
					Results.Info =  new ErrorResponse
					{
						Message = $"user's media is empty, user: {_user.OAccountId}, instaId: {_user.OInstagramAccountUsername}"
					};
					return Results;
				}
			}
			Results.IsSuccesful = false;
			Results.Info = new ErrorResponse
			{
				Message = $"accountcheck option is empty, user: {_user.OAccountId}, instaId: {_user.OInstagramAccountUsername}"
			};
			return Results;
		}
	}
}
