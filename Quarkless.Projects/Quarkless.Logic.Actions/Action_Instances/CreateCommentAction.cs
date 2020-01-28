using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quarkless.Models.Actions;
using Quarkless.Models.Actions.Enums.ActionTypes;
using Quarkless.Models.Actions.Enums.StrategyType;
using Quarkless.Models.Actions.Factory.Action_Options;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Actions.Models;
using Quarkless.Models.Comments;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.ContentInfo.Interfaces;
using Quarkless.Models.Heartbeat;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.SearchResponse;
using Quarkless.Models.Timeline;
using ActionType = Quarkless.Models.Actions.Enums.ActionType;

namespace Quarkless.Logic.Actions.Action_Instances
{
	internal class CreateCommentAction : IActionCommit
	{
		private readonly IContentInfoBuilder _contentInfoBuilder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly UserStoreDetails _user;
		private CommentingActionOptions _actionOptions;

		internal CreateCommentAction(UserStoreDetails userStoreDetails, IContentInfoBuilder contentInfoBuilder,
			IHeartbeatLogic heartbeatLogic)
		{
			_user = userStoreDetails;
			_contentInfoBuilder = contentInfoBuilder;
			_heartbeatLogic = heartbeatLogic;
			_actionOptions = new CommentingActionOptions();
		}
		private async Task<HolderComment> CommentingByLocation()
		{
			var by = new By
			{
				ActionType = (int)ActionType.CreateCommentMedia,
				User = _user.Profile.InstagramAccountId
			};

			var fetchMedias = (await _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
			{
				MetaDataType = MetaDataType.FetchMediaByUserLocationTargetList,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id
			}))?.Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0).ToList();

			var select = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count));
			if (select == null) return null;
			select.SeenBy.Add(by);

			await _heartbeatLogic.UpdateMetaData<Media>(new MetaDataCommitRequest<Media>
			{
				MetaDataType = MetaDataType.FetchMediaByUserLocationTargetList,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				Data = select,
			});

			return new HolderComment
			{
				Topic = _user.Profile.ProfileTopic.Category,
				MediaId = select.ObjectItem.Medias.FirstOrDefault()?.MediaId
			};
		}
		private async Task<HolderComment> CommentingByTopic()
		{
			var by = new By
			{
				ActionType = (int)ActionType.CreateCommentMedia,
				User = _user.Profile.InstagramAccountId
			};

			var fetchMedias = (await _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
			{
				MetaDataType = MetaDataType.FetchMediaByCommenters,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id
			}))?.Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0).ToList();

			var select = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count));

			if (select == null) return null;

			select.SeenBy.Add(by);

			await _heartbeatLogic.UpdateMetaData<Media>(new MetaDataCommitRequest<Media>
			{
				MetaDataType = MetaDataType.FetchMediaByCommenters,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				Data = select
			});

			return new HolderComment
			{
				Topic = _user.Profile.ProfileTopic.Category, 
				MediaId = select.ObjectItem.Medias.FirstOrDefault()?.MediaId
			};
		}
		private async Task<HolderComment> CommentingByLikers()
		{
			var by = new By
			{
				ActionType = (int)ActionType.CreateCommentMedia,
				User = _user.ShortInstagram.Id
			};

			var fetchMedias = (await _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
			{
				MetaDataType = MetaDataType.FetchMediaByLikers,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id
			}))?.Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0).ToList();

			var select = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count));
			if (select == null) return null;

			select.SeenBy.Add(by);

			await _heartbeatLogic.UpdateMetaData<Media>(new MetaDataCommitRequest<Media>
			{
				MetaDataType = MetaDataType.FetchMediaByCommenters,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				Data = select
			});

			return new HolderComment
			{
				Topic = _user.Profile.ProfileTopic.Category, 
				MediaId = select.ObjectItem.Medias.FirstOrDefault()?.MediaId
			};
		}

		public async Task<ResultCarrier<EventActionModel>> PushAsync(DateTimeOffset executionTime)
		{
			Console.WriteLine($"Create Comment Action Started: {_user.OAccountId}, {_user.OInstagramAccountUsername}, {_user.OInstagramAccountUser}");
			var results = new ResultCarrier<EventActionModel>();

			try
			{
				switch (_actionOptions.StrategySettings.StrategyType)
				{
					case CommentingStrategy.Default:
						var nominatedMedia = new HolderComment();
						CommentingActionType commentingActionSelected;

						if (_actionOptions.CommentingActionType == CommentingActionType.Any)
						{
							var commentingActionChances = new List<Chance<CommentingActionType>>
							{
								new Chance<CommentingActionType>
								{
									Object = CommentingActionType.CommentingViaTopic, 
									Probability = 0.25
								},
								new Chance<CommentingActionType>
								{
									Object = CommentingActionType.CommentingViaLikersPosts, 
									Probability = 0.40
								},
							};

							if (_user.Profile.LocationTargetList != null)
								if (_user.Profile.LocationTargetList.Count > 0)
									commentingActionChances.Add(new Chance<CommentingActionType>
									{
										Object = CommentingActionType.CommentingViaLocation, 
										Probability = 0.35
									});

							commentingActionSelected = SecureRandom.ProbabilityRoll(commentingActionChances);
						}
						else
						{
								commentingActionSelected = _actionOptions.CommentingActionType;
						}

						switch (commentingActionSelected)
						{
							case CommentingActionType.CommentingViaTopic:
								nominatedMedia = await CommentingByTopic();
								break;
							case CommentingActionType.CommentingViaLikersPosts:
								nominatedMedia = await CommentingByLikers();
								break;
							case CommentingActionType.CommentingViaLocation:
								nominatedMedia = await CommentingByLocation();
								break;
						}

						if (string.IsNullOrEmpty(nominatedMedia?.MediaId))
						{
							results.IsSuccessful = false;
							results.Info = new ErrorResponse
							{
								Message = $"No nominated media found, user: {_user.OAccountId}, instaId: {_user.OInstagramAccountUsername}",
								StatusCode = System.Net.HttpStatusCode.NotFound
							};
							return results;
						}

						var @event = new EventActionModel($"Comment_{_actionOptions.StrategySettings.StrategyType.ToString()}_{commentingActionSelected.ToString()}")
						{
							ActionType = ActionType.CreateCommentMedia,
							User = new UserStore
							{
								OAccountId = _user.OAccountId,
								OInstagramAccountUsername = _user.OInstagramAccountUsername,
								OInstagramAccountUser = _user.OInstagramAccountUser
							}
						};

						var createComment = new CreateCommentRequest
						{
							MediaId = nominatedMedia.MediaId,
							Text = _contentInfoBuilder.GenerateComment(nominatedMedia.Topic)
						};

						@event.DataObjects.Add(new EventBody(createComment, createComment.GetType(), executionTime));

						results.IsSuccessful = true;
						results.Results = @event;
						return results;
					case CommentingStrategy.TopNth:
						throw new NotImplementedException();
					default: throw new Exception("Invalid Comment Strategy");
				}
			}
			catch(Exception err)
			{
				Console.WriteLine(err);
				results.IsSuccessful = false;
				results.Info = new ErrorResponse
				{
					Exception = err,
					Message = err.Message
				};
				return results;
			}
			finally
			{
				Console.WriteLine($"Send Comment Action Ended: { _user.OAccountId}, { _user.OInstagramAccountUsername}, { _user.OInstagramAccountUser}");
			}
		}

		public IActionCommit ModifyOptions(IActionOptions newOptions)
		{
			try
			{
				_actionOptions = newOptions as CommentingActionOptions;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
			}
			return this;
		}
	}
}
