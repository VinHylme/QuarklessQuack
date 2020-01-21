using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Quarkless.Models.Comments;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.ContentInfo.Interfaces;
using Quarkless.Models.Heartbeat;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.RequestBuilder.Interfaces;
using Quarkless.Models.SearchResponse;
using Quarkless.Models.Services.Automation.Enums.Actions.ActionType;
using Quarkless.Models.Services.Automation.Enums.Actions.StrategyType;
using Quarkless.Models.Services.Automation.Interfaces;
using Quarkless.Models.Services.Automation.Models.ActionOptions;
using Quarkless.Models.Services.Automation.Models.StrategySettings;
using Quarkless.Models.Storage.Interfaces;
using Quarkless.Models.Timeline;
using Quarkless.Models.Topic;

namespace Quarkless.Logic.Services.Automation.Actions.EngageActions
{
	public class HolderComment
	{
		public CTopic Topic {get;set; }
		public string MediaId {get;set; }
	}
	public class CreateCommentAction : IActionCommit
	{
		private readonly IContentInfoBuilder _builder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly IUrlReader _urlReader;
		private UserStoreDetails user;
		private CommentingStrategySettings commentingStrategySettings;
		public CreateCommentAction(IContentInfoBuilder contentManager, IHeartbeatLogic heartbeatLogic, IUrlReader urlReader)
		{
			_heartbeatLogic = heartbeatLogic;
			_builder = contentManager;
			_urlReader = urlReader;
		}
		private HolderComment CommentingByLocation()
		{
			var by = new By
			{
				ActionType = (int)ActionType.CreateCommentMedia,
				User = user.Profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchMediaByUserLocationTargetList, 
					ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
					InstagramId = user.ShortInstagram.Id
				})
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0);

			if (fetchMedias == null) return null;
			var metaS = fetchMedias as Meta<Media>[] ?? fetchMedias.ToArray();
			var select = metaS.ElementAtOrDefault(SecureRandom.Next(metaS.Count()));
			if (select == null) return null;
			select.SeenBy.Add(by);

			_heartbeatLogic.UpdateMetaData<Media>(new MetaDataCommitRequest<Media>
			{
				MetaDataType = MetaDataType.FetchMediaByUserLocationTargetList,
				ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
				InstagramId = user.ShortInstagram.Id,
				Data = select,
			}).GetAwaiter().GetResult();

			return new HolderComment { Topic = user.Profile.ProfileTopic.Category, MediaId = select.ObjectItem.Medias.FirstOrDefault()?.MediaId };
		}

		private HolderComment CommentingByTopic()
		{
			var by = new By
			{
				ActionType = (int)ActionType.CreateCommentMedia,
				User = user.Profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchMediaByCommenters,
					ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
					InstagramId = user.ShortInstagram.Id
				})
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0);

			var metaS = fetchMedias as Meta<Media>[] ?? fetchMedias.ToArray();

			var select = metaS.ElementAtOrDefault(SecureRandom.Next(metaS.Count()));
				if (select == null) return null;
				select.SeenBy.Add(by);

			_heartbeatLogic.UpdateMetaData<Media>(new MetaDataCommitRequest<Media>
			{
				MetaDataType = MetaDataType.FetchMediaByCommenters,
				ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
				InstagramId = user.ShortInstagram.Id,
				Data = select
			}).GetAwaiter().GetResult();
			return new HolderComment { Topic = user.Profile.ProfileTopic.Category, MediaId = select.ObjectItem.Medias.FirstOrDefault()?.MediaId};
		}

		private HolderComment CommentingByLikers()
		{
			var by = new By
			{
				ActionType = (int)ActionType.CreateCommentMedia,
				User = user.ShortInstagram.Id
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchMediaByLikers,
					ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
					InstagramId = user.ShortInstagram.Id
				})
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0);
			
			if (fetchMedias == null) return null;
			var metaS = fetchMedias as Meta<Media>[] ?? fetchMedias.ToArray();

			var select = metaS.ElementAtOrDefault(SecureRandom.Next(metaS.Count()));
			if (select == null) return null;

			select.SeenBy.Add(by);
			_heartbeatLogic.UpdateMetaData<Media>(new MetaDataCommitRequest<Media>
			{
				MetaDataType = MetaDataType.FetchMediaByCommenters,
				ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
				InstagramId = user.ShortInstagram.Id,
				Data = select
			}).GetAwaiter().GetResult();
			return new HolderComment { Topic = user.Profile.ProfileTopic.Category, MediaId = select.ObjectItem.Medias.FirstOrDefault()?.MediaId };
		}

		public IActionCommit IncludeStrategy(IStrategySettings strategy)
		{
			commentingStrategySettings = strategy as CommentingStrategySettings;
			return this;
		}
		/// <summary>
		/// TO DO : IMPLEMENT OTHER STRATEGIES
		/// </summary>
		/// <param name="actionOptions"></param>
		/// <returns></returns>
		public ResultCarrier<IEnumerable<TimelineEventModel>> Push(IActionOptions actionOptions)
		{
			Console.WriteLine($"Create Comment Action Started: {user.OAccountId}, {user.OInstagramAccountUsername}, {user.OInstagramAccountUser}");
			var commentingActionOptions = actionOptions as CommentingActionOptions;
			var results = new ResultCarrier<IEnumerable<TimelineEventModel>>();
			if(commentingActionOptions==null && user==null)
			{
				results.IsSuccessful = false;
				results.Info = new ErrorResponse
				{
					Message = $"commenting options is null, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.NotFound
				};
				return results;
			}
			try
			{
				if(commentingStrategySettings.CommentingStrategy == CommentingStrategy.Default)
				{
					var nominatedMedia = new HolderComment();
					var commentingActionSelected = CommentingActionType.CommentingViaTopic;
					if (commentingActionOptions != null && commentingActionOptions.CommentingActionType == CommentingActionType.Any)
					{
						var commentingActionChances = new List<Chance<CommentingActionType>>
						{
							new Chance<CommentingActionType>{Object = CommentingActionType.CommentingViaTopic, Probability = 0.25},
							new Chance<CommentingActionType>{Object = CommentingActionType.CommentingViaLikersPosts, Probability = 0.40},
						};
						if (user.Profile.LocationTargetList != null)
							if (user.Profile.LocationTargetList.Count > 0)
								commentingActionChances.Add(new Chance<CommentingActionType> { Object = CommentingActionType.CommentingViaLocation, Probability = 0.35 });

						commentingActionSelected = SecureRandom.ProbabilityRoll(commentingActionChances);
					}
					else
					{
						if (commentingActionOptions != null)
							commentingActionSelected = commentingActionOptions.CommentingActionType;
					}

					switch (commentingActionSelected)
					{
						case CommentingActionType.CommentingViaTopic:
							nominatedMedia = CommentingByTopic();
							break;
						case CommentingActionType.CommentingViaLikersPosts:
							nominatedMedia = CommentingByLikers();
							break;
						case CommentingActionType.CommentingViaLocation:
							nominatedMedia = CommentingByLocation();
							break;
					}
					if(string.IsNullOrEmpty(nominatedMedia?.MediaId))
					{
						results.IsSuccessful = false;
						results.Info = new ErrorResponse
						{
							Message = $"No nominated media found, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
							StatusCode = System.Net.HttpStatusCode.NotFound
						};
						return results;
					}
					var createComment = new CreateCommentRequest
					{
						Text = _builder.GenerateComment(nominatedMedia.Topic)
					};
					var restModel = new RestModel
					{
						BaseUrl = string.Format(_urlReader.CreateComment, nominatedMedia.MediaId),
						RequestType = RequestType.Post,
						JsonBody = JsonConvert.SerializeObject(createComment),
						User = user
					};
					results.IsSuccessful = true;
					if (commentingActionOptions != null)
						results.Results = new List<TimelineEventModel>
						{
							new TimelineEventModel
							{
								ActionName =
									$"Comment_{commentingStrategySettings.CommentingStrategy.ToString()}_{commentingActionSelected.ToString()}",
								Data = restModel,
								ExecutionTime = commentingActionOptions.ExecutionTime
							}
						};
					return results;
				}
				results.IsSuccessful = false;
				results.Info = new ErrorResponse
				{
					Message = $"wrong strategy used, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.NotFound
				};
				return results;
			}
			catch(Exception ee)
			{
				results.IsSuccessful = false;
				results.Info = new ErrorResponse
				{
					Message = $"{ee.Message}, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.InternalServerError,
					Exception = ee
				};
				return results;
			}
		}
		public IActionCommit IncludeUser(UserStoreDetails userStoreDetails)
		{
			user = userStoreDetails;
			return this;
		}

		public IActionCommit IncludeStorage(IStorage storage)
		{
			throw new NotImplementedException();
		}
	}
}
