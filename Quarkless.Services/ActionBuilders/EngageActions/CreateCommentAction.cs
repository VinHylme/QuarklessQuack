using Newtonsoft.Json;
using Quarkless.Services.Interfaces;
using Quarkless.Services.Interfaces.Actions;
using Quarkless.Services.StrategyBuilders;
using QuarklessContexts.Classes.Carriers;
using QuarklessContexts.Enums;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Requests;
using QuarklessContexts.Models.ServicesModels.HeartbeatModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.Handlers.RequestBuilder.Consts;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quarkless.Services.ActionBuilders.EngageActions
{
	public enum CommentingActionType
	{
		Any,
		CommentingViaLikersPosts, 
		CommentingViaLocation,
		CommentingViaTopic,
		CommentingUserReply,
		CommentingPostReply
	}
	public class HolderComment
	{
		public string Topic {get;set; }
		public string MediaId {get;set; }
	}
	public class CreateCommentAction : IActionCommit
	{
		private readonly IContentManager _builder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly ProfileModel _profile;
		private UserStoreDetails user;
		private CommentingStrategySettings commentingStrategySettings;
		public CreateCommentAction(IContentManager contentManager, IHeartbeatLogic heartbeatLogic, ProfileModel profile)
		{
			_heartbeatLogic = heartbeatLogic;
			_builder = contentManager;
			_profile = profile;
		}
		private HolderComment CommentingByLocation()
		{
			By by = new By
			{
				ActionType = (int)ActionType.CreateCommentMedia,
				User = _profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByUserLocationTargetList, _profile.Topics.TopicFriendlyName,_profile.InstagramAccountId)
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0);
			if (fetchMedias != null)
			{
				var select = fetchMedias.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count()));
				if (select != null)
				{
						select.SeenBy.Add(by);
						_heartbeatLogic.UpdateMetaData<Media>(MetaDataType.FetchMediaByUserLocationTargetList, _profile.Topics.TopicFriendlyName, select,_profile.InstagramAccountId).GetAwaiter().GetResult();
						return new HolderComment { Topic = _profile.Topics.TopicFriendlyName, MediaId = select.ObjectItem.Medias.FirstOrDefault().MediaId };
				}
			}
			return null;
		}
		private HolderComment CommentingByTopic()
		{
			By by = new By
			{
				ActionType = (int)ActionType.CreateCommentMedia,
				User = _profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByCommenters,_profile.Topics.TopicFriendlyName)
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0);
			if (fetchMedias!=null) { 
				var select = fetchMedias.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count()));
				if (select != null) {
					select.SeenBy.Add(by);
					_heartbeatLogic.UpdateMetaData<Media>(MetaDataType.FetchMediaByCommenters,_profile.Topics.TopicFriendlyName, select).GetAwaiter().GetResult();
					return new HolderComment { Topic = _profile.Topics.TopicFriendlyName, MediaId = select.ObjectItem.Medias.FirstOrDefault().MediaId};
				}
			}
			return null;
		}
		private HolderComment CommentingByLikers()
		{
			By by = new By
			{
				ActionType = (int)ActionType.CreateCommentMedia,
				User = _profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByLikers, _profile.Topics.TopicFriendlyName)
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0);
			if (fetchMedias != null)
			{
				var select = fetchMedias.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count()));
				if (select != null)
				{				
					select.SeenBy.Add(by);
					_heartbeatLogic.UpdateMetaData<Media>(MetaDataType.FetchMediaByCommenters, _profile.Topics.TopicFriendlyName, select).GetAwaiter().GetResult();
					return new HolderComment { Topic = _profile.Topics.TopicFriendlyName, MediaId = select.ObjectItem.Medias.FirstOrDefault().MediaId };
					
				}
			}
			return null;
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
			Console.WriteLine("Create Comment Action Started");
			CommentingActionOptions commentingActionOptions = actionOptions as CommentingActionOptions;
			ResultCarrier<IEnumerable<TimelineEventModel>> Results = new ResultCarrier<IEnumerable<TimelineEventModel>>();
			if(commentingActionOptions==null && user==null)
			{
				Results.IsSuccesful = false;
				Results.Info = new ErrorResponse
				{
					Message = $"commenting options is null, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.NotFound
				};
				return Results;
			}
			try
			{
				if(commentingStrategySettings.CommentingStrategy == CommentingStrategy.Default)
				{
					HolderComment nominatedMedia = new HolderComment();
					CommentingActionType commentingActionSelected = CommentingActionType.CommentingViaTopic;
					if (commentingActionOptions.CommentingActionType == CommentingActionType.Any)
					{
						List<Chance<CommentingActionType>> commentingActionChances = new List<Chance<CommentingActionType>>
						{
							new Chance<CommentingActionType>{Object = CommentingActionType.CommentingViaTopic, Probability = 0.25},
							new Chance<CommentingActionType>{Object = CommentingActionType.CommentingViaLikersPosts, Probability = 0.40},
						};
						if (_profile.LocationTargetList != null)
							if (_profile.LocationTargetList.Count > 0)
								commentingActionChances.Add(new Chance<CommentingActionType> { Object = CommentingActionType.CommentingViaLocation, Probability = 0.35 });

						commentingActionSelected = SecureRandom.ProbabilityRoll(commentingActionChances);
					}
					else
					{
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
					if(string.IsNullOrEmpty(nominatedMedia.MediaId))
					{
						Results.IsSuccesful = false;
						Results.Info = new ErrorResponse
						{
							Message = $"No nominated media found, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
							StatusCode = System.Net.HttpStatusCode.NotFound
						};
						return Results;
					}
					CreateCommentRequest createComment = new CreateCommentRequest
					{
						Text = _builder.GenerateComment(nominatedMedia.Topic, _profile.Language)
					};
					RestModel restModel = new RestModel
					{
						BaseUrl = string.Format(UrlConstants.CreateComment, nominatedMedia.MediaId),
						RequestType = RequestType.POST,
						JsonBody = JsonConvert.SerializeObject(createComment),
						User = user
					};
					Results.IsSuccesful = true;
					Results.Results = new List<TimelineEventModel>
					{
						new TimelineEventModel{
							ActionName = $"Comment_{commentingStrategySettings.CommentingStrategy.ToString()}_{commentingActionSelected.ToString()}",
							Data = restModel,
							ExecutionTime = commentingActionOptions.ExecutionTime
						}
					};
					return Results;
				}
				Results.IsSuccesful = false;
				Results.Info = new ErrorResponse
				{
					Message = $"wrong strategy used, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.NotFound
				};
				return Results;
			}
			catch(Exception ee)
			{
				Results.IsSuccesful = false;
				Results.Info = new ErrorResponse
				{
					Message = $"{ee.Message}, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.InternalServerError,
					Exception = ee
				};
				return Results;
			}
		}
		public IActionCommit IncludeUser(UserStoreDetails userStoreDetails)
		{
			user = userStoreDetails;
			return this;
		}
	}
}
