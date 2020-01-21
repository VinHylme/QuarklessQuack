using System.Collections.Generic;
using System.Linq;
using InstagramApiSharp.Classes.Models;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.ContentInfo.Interfaces;
using Quarkless.Models.Heartbeat;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.RequestBuilder.Interfaces;
using Quarkless.Models.SearchResponse;
using Quarkless.Models.Services.Automation.Interfaces;
using Quarkless.Models.Services.Automation.Models.ActionOptions;
using Quarkless.Models.Services.Automation.Models.StrategySettings;
using Quarkless.Models.Storage.Interfaces;
using Quarkless.Models.Timeline;

namespace Quarkless.Logic.Services.Automation.Actions.MaintainActions
{
	public class AccountCheckerAction : IActionCommit
	{
		private readonly IContentInfoBuilder _content;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly IUrlReader _urlReader;
		private UserStoreDetails user;
		private AccountCheckerStrategySettings _accountCheckerStrategySettings;
		public AccountCheckerAction(IContentInfoBuilder content, IHeartbeatLogic heartbeatLogic, IUrlReader urlReader)
		{
			_content = content;
			_heartbeatLogic = heartbeatLogic;
			_urlReader = urlReader;
		}
		public IActionCommit IncludeStrategy(IStrategySettings strategy)
		{
			_accountCheckerStrategySettings = strategy as AccountCheckerStrategySettings;
			return this;
		}

		public IActionCommit IncludeUser(UserStoreDetails userStoreDetails)
		{
			user = userStoreDetails;
			return this;
		}

		public IActionCommit IncludeStorage(IStorage storage)
		{
			throw new System.NotImplementedException();
		}

		struct ContainMedia
		{
			public byte[] mediaData;
			public string mediaId;
			public int mediaType;
		}
		public ResultCarrier<IEnumerable<TimelineEventModel>> Push(IActionOptions actionOptions)
		{
			var results = new ResultCarrier<IEnumerable<TimelineEventModel>>();
			if (actionOptions != null)
			{
				var accountCheckerActionOptions = actionOptions as AccountCheckerActionOptions;
				var currentUsersMedia = _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
					{
						MetaDataType = MetaDataType.FetchUserOwnProfile,
						ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id, 
						InstagramId = user.ShortInstagram.Id
					}).GetAwaiter().GetResult().ToList();

				var postAnalyser = accountCheckerActionOptions.PostAnalyser;
				//remove duplicates from user's gallery
				var usersMediaInBytes = new List<ContainMedia>();
				{
					foreach(var userMedia in currentUsersMedia)
					{
						if (userMedia == null) continue;
						var medias = userMedia.ObjectItem.Medias;
						foreach(var umedia in medias)
						{
							foreach(var url in umedia.MediaUrl)
							{
								usersMediaInBytes.Add(new ContainMedia
								{
									mediaData = postAnalyser.Manager.DownloadMedia(url),
									mediaId = umedia.MediaId,
									mediaType = (int) umedia.MediaType
								});
							}
						}
					}

					var images = usersMediaInBytes.Where(a => a.mediaType == (int)InstaMediaType.Image);
					var duplicates = postAnalyser.Manipulation.ImageEditor.DuplicateImages(images.Select(s=>s.mediaData),0.95);
					var enumerable = duplicates as byte[][] ?? duplicates.ToArray();
					if(enumerable.Any()) { 
						var tosend = new List<TimelineEventModel>();
						for (int x = 0; x < enumerable.Count(); x++)
						{
							RestModel restModel = new RestModel
							{
								BaseUrl = string.Format(_urlReader.DeleteMedia,images.ElementAt(x).mediaId,images.ElementAt(x).mediaType),
								User = user,
								RequestType = RequestType.Post,
							};
							tosend.Add(new TimelineEventModel
							{
								ActionName = $"ActionChecker_{_accountCheckerStrategySettings.AccountCheckerStrategy.ToString()}",
								Data = restModel,
								ExecutionTime = accountCheckerActionOptions.ExecutionTime.AddMinutes((_accountCheckerStrategySettings.OffsetPerAction.TotalMinutes)) 
							});
						}
						results.IsSuccessful = true;
						results.Results = tosend;
						return results;
					}
				}
			}
			results.IsSuccessful = false;
			results.Info = new ErrorResponse
			{
				Message = $"accountcheck option is empty, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}"
			};
			return results;
		}
	}
}
