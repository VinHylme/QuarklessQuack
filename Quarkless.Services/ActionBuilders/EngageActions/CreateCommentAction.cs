using Newtonsoft.Json;
using Quarkless.Services.Interfaces;
using Quarkless.Services.Interfaces.Actions;
using Quarkless.Services.StrategyBuilders;
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
		
		private HolderComment CommentingByTopic()
		{
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByCommenters,_profile.Topic).GetAwaiter().GetResult();
			if (fetchMedias!=null) { 
				var select = fetchMedias.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count()));
				if (select.HasValue) {
					By by = new By
					{
						ActionType = (int)ActionType.CreateCommentMedia,
						User = _profile.InstagramAccountId
					};
					if (!select.Value.SeenBy.Contains(by)) { 
						select.Value.SeenBy.Add(by);
						_heartbeatLogic.UpdateMetaData<Media>(MetaDataType.FetchMediaByCommenters,_profile.Topic, select.Value).GetAwaiter().GetResult();
						return new HolderComment { Topic = _profile.Topic, MediaId = select.Value.ObjectItem.Medias.FirstOrDefault().MediaId};
					}
				}
			}
			return null;
		}
		private HolderComment CommentingByLikers()
		{
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByLikers, _profile.Topic).GetAwaiter().GetResult();
			if (fetchMedias != null)
			{
				var select = fetchMedias.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count()));
				if (select.HasValue)
				{
					By by = new By
					{
						ActionType = (int)ActionType.CreateCommentMedia,
						User = _profile.InstagramAccountId
					};
					if (!select.Value.SeenBy.Contains(by))
					{
						select.Value.SeenBy.Add(by);
						_heartbeatLogic.UpdateMetaData<Media>(MetaDataType.FetchMediaByCommenters, _profile.Topic, select.Value).GetAwaiter().GetResult();
						return new HolderComment { Topic = _profile.Topic, MediaId = select.Value.ObjectItem.Medias.FirstOrDefault().MediaId };
					}
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
		public IEnumerable<TimelineEventModel> Push(IActionOptions actionOptions)
		{
			CommentingActionOptions commentingActionOptions = actionOptions as CommentingActionOptions;
			
			if(commentingActionOptions==null && user==null) return null ;
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
							new Chance<CommentingActionType>{Object = CommentingActionType.CommentingViaTopic, Probability = 0.35},
							new Chance<CommentingActionType>{Object = CommentingActionType.CommentingViaLikersPosts, Probability = 0.35},
						};
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
					}
					if(string.IsNullOrEmpty(nominatedMedia.MediaId)) return null;
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

					return new List<TimelineEventModel>
					{
						new TimelineEventModel{
							ActionName = $"Comment_{commentingStrategySettings.CommentingStrategy.ToString()}_{commentingActionSelected.ToString()}",
							Data = restModel,
							ExecutionTime = commentingActionOptions.ExecutionTime 
						} 
					};
				}
				
				return null;
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}
		public IActionCommit IncludeUser(UserStoreDetails userStoreDetails)
		{
			user = userStoreDetails;
			return this;
		}
	}
}
