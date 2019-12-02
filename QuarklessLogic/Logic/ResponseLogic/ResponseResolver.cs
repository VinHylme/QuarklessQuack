using System;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using Newtonsoft.Json;
using Quarkless.Interfacing;
using QuarklessContexts.Enums;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessContexts.Models.LookupModels;
using QuarklessContexts.Models.MessagingModels;
using QuarklessContexts.Models.Sections;
using QuarklessContexts.Models.TimelineLoggingRepository;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Logic.InstagramAccountLogic;
using QuarklessLogic.Logic.LookupLogic;
using QuarklessLogic.Logic.TimelineEventLogLogic;
using QuarklessRepositories.RedisRepository.LoggerStoring;

namespace QuarklessLogic.Logic.ResponseLogic
{
	public class ResponseResolver : CommonInterface, IResponseResolver
	{
		private IAPIClientContainer _client;
		private readonly ILookupLogic _lookupLogic;
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private readonly ITimelineEventLogLogic _timelineEventLogLogic;
		private string AccountId { get; set; }
		private string InstagramAccountId { get; set; }
		public ResponseResolver(IAPIClientContainer client, IInstagramAccountLogic instagramAccountLogic, 
			ITimelineEventLogLogic timelineEventLogLogic, ILoggerStore loggerStore, ILookupLogic lookup)
			: base(loggerStore, Sections.ResponseLogic.GetDescription())
		{
			_client = client;
			_instagramAccountLogic = instagramAccountLogic;
			_timelineEventLogLogic = timelineEventLogLogic;
			_lookupLogic = lookup;
		}

		public IResponseResolver WithClient(IAPIClientContainer client)
		{
			_client = client;
			return this;
		}
		private string CreateMessage(ActionType actionType)
		{
			switch (actionType)
			{
				case ActionType.CreatePost:
					return "New Post Created";
				case ActionType.CreateCommentMedia:
					return "Comment Made";
				case ActionType.CreateBiography:
					return "Updated Biography";
				case ActionType.CreateCommentReply:
					return "Replied to comment";
				case ActionType.CreateStory:
					return "Added a new story";
				case ActionType.FollowHashtag:
					return "Followed Hashtag, based on your profile";
				case ActionType.FollowUser:
					return "Followed User who happens to follow the similar topics as you";
				case ActionType.UnFollowUser:
					return "Unfollowed User who is not engaging with you";
				case ActionType.LikeComment:
					return "Liked user's comment";
				case ActionType.LikePost:
					return "Liked a new post, based on your profile";
				case ActionType.SendDirectMessageVideo:
					return "Sent a video to a user via direct message";
				case ActionType.SendDirectMessagePhoto:
					return "Sent a video to a user via direct message";
				case ActionType.SendDirectMessageText:
					return "Sent a text to a user via direct message";
				case ActionType.SendDirectMessageLink:
					return "Sent a link to a user via direct message";
				case ActionType.SendDirectMessageProfile:
					return "Shared profile with a user via direct message";
				case ActionType.SendDirectMessageMedia:
					return "Shared media with a user via direct message";
				default: return $"{actionType.ToString()} Made";
			}
		}
		public async Task<IResult<TInput>> WithResolverAsync<TInput>(IResult<TInput> response, ActionType actionType, string request)
		{
			var context = _client.GetContext.InstagramAccount;
			AccountId = context.AccountId;
			InstagramAccountId = context.Id;

			if (response?.Info == null)
			{
				await _timelineEventLogLogic.AddTimelineLogFor(new TimelineEventLog
				{
					AccountID = context.AccountId,
					InstagramAccountID = context.Id,
					ActionType = actionType,
					DateAdded = DateTime.UtcNow,
					Level = 3,
					Request = request,
					Response = JsonConvert.SerializeObject(response),
					Status = TimelineEventStatus.ServerError,
					Message = "Something is not exactly happy dappy right now, we're sorry"
				});
				return response;
			};
			if (response.Succeeded)
			{
				await _timelineEventLogLogic.AddTimelineLogFor(new TimelineEventLog
				{
					_id = Guid.NewGuid().ToString(),
					AccountID = context.AccountId,
					InstagramAccountID = context.Id,
					ActionType = actionType,
					DateAdded = DateTime.UtcNow,
					Level = 1,
					Request = request,
					Response = JsonConvert.SerializeObject(response),
					Status = TimelineEventStatus.Success,
					Message = CreateMessage(actionType)
				});
			}
			else
			{
				await _timelineEventLogLogic.AddTimelineLogFor(new TimelineEventLog
				{
					AccountID = context.AccountId,
					InstagramAccountID = context.Id,
					ActionType = actionType,
					DateAdded = DateTime.UtcNow,
					Level = 2,
					Request = request,
					Response = JsonConvert.SerializeObject(response),
					Status = TimelineEventStatus.Failed,
					Message = $"Failed to carry out {actionType.ToString()} action"
				});
			}

			switch (response.Info.ResponseType)
			{
				case ResponseType.AlreadyLiked:
					break;
				case ResponseType.CantLike:
					break;
				case ResponseType.ChallengeRequired:
					var challenge = await GetChallengeRequireVerifyMethodAsync();
					if (challenge.Succeeded)
					{
						if (challenge.Value.SubmitPhoneRequired)
						{

						}
						else
						{
							if(challenge.Value.StepData!=null)
							{
								if (!string.IsNullOrEmpty(challenge.Value.StepData.PhoneNumber))
								{
									//verify phone
									var code = await RequestVerifyCodeToSmsForChallengeRequireAsync();
									if (code.Succeeded)
									{
										await _instagramAccountLogic.PartialUpdateInstagramAccount(context.AccountId,
											context.Id,
											new InstagramAccountModel
											{
												AgentState = (int)AgentState.Challenge,
												ChallengeInfo = new ChallengeCodeRequestResponse
												{
													Verify = "phone",
													Details = challenge.Value.StepData.PhoneNumber,
													ChallangePath = GetChallangeInfo()
												}
											});
									}
								}
								if (!string.IsNullOrEmpty(challenge.Value.StepData.Email))
								{
									var code = await RequestVerifyCodeToEmailForChallengeRequireAsync();
									if (code.Succeeded)
									{
										await _instagramAccountLogic.PartialUpdateInstagramAccount(context.AccountId,
											context.Id,
											new InstagramAccountModel
											{
												AgentState = (int) AgentState.Challenge,
												ChallengeInfo = new ChallengeCodeRequestResponse
												{
													Verify = "email",
													Details = challenge.Value.StepData.Email,
													ChallangePath = GetChallangeInfo()
												}
											});
									}
								}
							}
						}
					}
					await _client.GetContext.ActionClient.GetLoggedInChallengeDataInfoAsync();
					await AcceptChallenge();
					break;
				case ResponseType.CheckPointRequired:
					break;
				case ResponseType.CommentingIsDisabled:
					break;
				case ResponseType.ConsentRequired:
					await AcceptConsent();
					break;
				case ResponseType.DeletedPost:
					break;
				case ResponseType.InactiveUser:
					break;
				case ResponseType.LoginRequired:
					break;
				case ResponseType.InternalException:
					break;
				case ResponseType.ActionBlocked:
				case ResponseType.Spam:
				case ResponseType.RequestsLimit:
					await _instagramAccountLogic.PartialUpdateInstagramAccount(context.AccountId,
						context.Id,
						new InstagramAccountModel
						{
							AgentState = (int) AgentState.Blocked
						});
					break;
				case ResponseType.OK:
					switch (actionType)
					{
						case ActionType.SendDirectMessageMedia:
							await MarkAsComplete(actionType,
								JsonConvert.DeserializeObject<ShareDirectMediaModel>(request).Recipients.ToArray());
							break;
						case ActionType.SendDirectMessageAudio:
							await MarkAsComplete(actionType,
								JsonConvert.DeserializeObject<SendDirectPhotoModel>(request).Recipients.ToArray());
							break;
						case ActionType.SendDirectMessageLink:
							await MarkAsComplete(actionType,
								JsonConvert.DeserializeObject<SendDirectLinkModel>(request).Recipients.ToArray());
							break;
						case ActionType.SendDirectMessagePhoto:
							await MarkAsComplete(actionType,
								JsonConvert.DeserializeObject<SendDirectPhotoModel>(request).Recipients.ToArray());
							break;
						case ActionType.SendDirectMessageProfile:
							await MarkAsComplete(actionType,
								JsonConvert.DeserializeObject<SendDirectProfileModel>(request).Recipients.ToArray());
							break;
						case ActionType.SendDirectMessageText:
							await MarkAsComplete(actionType,
								JsonConvert.DeserializeObject<SendDirectTextModel>(request).Recipients.ToArray());
							break;
						case ActionType.SendDirectMessageVideo:
							await MarkAsComplete(actionType,
								JsonConvert.DeserializeObject<SendDirectVideoModel>(request).Recipients.ToArray());
							break;
					}
					break;
				case ResponseType.MediaNotFound:
					break;
				case ResponseType.SomePagesSkipped:
					break;
				case ResponseType.SentryBlock:
					break;
				case ResponseType.Unknown:
					await _client.GetContext.ActionClient.GetLoggedInChallengeDataInfoAsync();
					await _client.GetContext.ActionClient.AcceptChallengeAsync();
					break;
				case ResponseType.WrongRequest:
					break;
				case ResponseType.UnExpectedResponse:
					break;
				case ResponseType.NetworkProblem:
					break;
			}

			return response;
		}
		public async Task<IResult<TInput>> WithResolverAsync<TInput>(IResult<TInput> response)
		{
			var context = _client.GetContext.InstagramAccount;
			AccountId = context.AccountId;
			InstagramAccountId = context.Id;
			if (response?.Info == null) return response;
			switch (response.Info.ResponseType)
			{
				case ResponseType.AlreadyLiked:
					break;
				case ResponseType.CantLike:
					break;
				case ResponseType.ChallengeRequired:
					var challenge = await GetChallengeRequireVerifyMethodAsync();
					if (challenge.Succeeded)
					{
						if (challenge.Value.SubmitPhoneRequired)
						{

						}
						else
						{
							if(challenge.Value.StepData!=null)
							{
								if (!string.IsNullOrEmpty(challenge.Value.StepData.PhoneNumber))
								{
									//verify phone
									var code = await RequestVerifyCodeToSmsForChallengeRequireAsync();
									if (code.Succeeded)
									{
										await _instagramAccountLogic.PartialUpdateInstagramAccount(context.AccountId,
											context.Id,
											new InstagramAccountModel
											{
												AgentState = (int)AgentState.Challenge,
												ChallengeInfo = new ChallengeCodeRequestResponse
												{
													Verify = "phone",
													Details = challenge.Value.StepData.PhoneNumber,
													ChallangePath = GetChallangeInfo()
												}
											});
									}
								}
								if (!string.IsNullOrEmpty(challenge.Value.StepData.Email))
								{
									var code = await RequestVerifyCodeToEmailForChallengeRequireAsync();
									if (code.Succeeded)
									{
										await _instagramAccountLogic.PartialUpdateInstagramAccount(context.AccountId,
											context.Id,
											new InstagramAccountModel
											{
												AgentState = (int) AgentState.Challenge,
												ChallengeInfo = new ChallengeCodeRequestResponse
												{
													Verify = "email",
													Details = challenge.Value.StepData.Email,
													ChallangePath = GetChallangeInfo()
												}
											});
									}
								}
							}
						}
					}
					await _client.GetContext.ActionClient.GetLoggedInChallengeDataInfoAsync();
					await AcceptChallenge();
					break;
				case ResponseType.CheckPointRequired:
					break;
				case ResponseType.CommentingIsDisabled:
					break;
				case ResponseType.ConsentRequired:
					await AcceptConsent();
					break;
				case ResponseType.DeletedPost:
					break;
				case ResponseType.InactiveUser:
					break;
				case ResponseType.LoginRequired:
					break;
				case ResponseType.InternalException:
					break;
				case ResponseType.ActionBlocked:
				case ResponseType.Spam:
				case ResponseType.RequestsLimit:
					await _instagramAccountLogic.PartialUpdateInstagramAccount(context.AccountId,
						context.Id,
						new InstagramAccountModel
						{
							AgentState = (int) AgentState.Blocked
						});
					break;
				case ResponseType.OK:

					break;
				case ResponseType.MediaNotFound:
					break;
				case ResponseType.SomePagesSkipped:
					break;
				case ResponseType.SentryBlock:
					break;
				case ResponseType.Unknown:
					await _client.GetContext.ActionClient.GetLoggedInChallengeDataInfoAsync();
					await _client.GetContext.ActionClient.AcceptChallengeAsync();
					break;
				case ResponseType.WrongRequest:
					break;
				case ResponseType.UnExpectedResponse:
					break;
				case ResponseType.NetworkProblem:
					break;
			}

			return response;
		}
		private async Task MarkAsComplete(ActionType actionType, params string[] ids)
		{
			foreach(var id in ids)
			{
				try
				{
					var getObj = (await _lookupLogic.Get(AccountId, InstagramAccountId, id)).FirstOrDefault();
					if (getObj == null)
						continue;
					await _lookupLogic.UpdateObjectToLookup(AccountId, InstagramAccountId, id, getObj, new LookupModel
					{
						Id = Guid.NewGuid().ToString(),
						LastModified = DateTime.UtcNow,
						LookupStatus = LookupStatus.Completeted,
						ActionType = actionType
					});
				}
				catch(Exception ee)
				{
					await Expect(ee.Message, nameof(MarkAsComplete), AccountId, InstagramAccountId);
				}
			}
		}
		private async Task<IResult<bool>> AcceptConsent()
		{
			try
			{
				return await _client.GetContext.ActionClient.AcceptConsentAsync();
			}
			catch(Exception ee)
			{
				await Expect(ee.Message, nameof(AcceptConsent), AccountId, InstagramAccountId);
				return null;
			}
		}
		private async Task<IResult<bool>> AcceptChallenge()
		{
			try
			{
				return await _client.GetContext.ActionClient.AcceptChallengeAsync();
			}
			catch(Exception ee)
			{
				await Expect(ee.Message, nameof(AcceptChallenge), AccountId, InstagramAccountId);
				return null;
			}
		}
		private async Task<IResult<InstaChallengeRequireEmailVerify>> RequestVerifyCodeToEmailForChallengeRequireAsync()
		{
			try
			{
				return await _client.GetContext.ActionClient.RequestVerifyCodeToEmailForChallengeRequireAsync();
			}
			catch(Exception ee)
			{
				await Expect(ee.Message, nameof(RequestVerifyCodeToEmailForChallengeRequireAsync), AccountId, InstagramAccountId);
				return null;
			}
		}
		private async Task<IResult<InstaChallengeRequireSMSVerify>> RequestVerifyCodeToSmsForChallengeRequireAsync()
		{
			try
			{
				return await _client.GetContext.ActionClient.RequestVerifyCodeToSMSForChallengeRequireAsync();
			}
			catch (Exception ee)
			{
				await Expect(ee.Message, nameof(RequestVerifyCodeToSmsForChallengeRequireAsync), AccountId, InstagramAccountId);
				return null;
			}
		}
		private InstaChallengeLoginInfo GetChallangeInfo()
		{
			try
			{
				return _client.GetContext.ActionClient.ChallengeLoginInfo;
			}
			catch (Exception ee)
			{
				Expect(ee.Message, nameof(GetChallangeInfo), AccountId, InstagramAccountId).GetAwaiter().GetResult();
				return null;
			}
		}
		private async Task<IResult<InstaChallengeRequireVerifyMethod>> GetChallengeRequireVerifyMethodAsync()
		{
			try
			{
				return await _client.GetContext.ActionClient.GetChallengeRequireVerifyMethodAsync();
			}
			catch(Exception ee)
			{
				await Expect(ee.Message, nameof(GetChallengeRequireVerifyMethodAsync), AccountId, InstagramAccountId);
				return null;
			}
		}
		private async Task<IResult<InstaLoginResult>> VerifyCodeForChallengeRequireAsync(string code)
		{
			try
			{
				return await _client.GetContext.ActionClient.VerifyCodeForChallengeRequireAsync(code);
			}
			catch (Exception ee)
			{
				await Expect(ee.Message, nameof(VerifyCodeForChallengeRequireAsync), AccountId, InstagramAccountId);
				return null;
			}
		}

	}
}
