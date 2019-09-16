using System;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using Newtonsoft.Json;
using QuarklessContexts.Enums;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessContexts.Models.TimelineLoggingRepository;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessLogic.Logic.InstagramAccountLogic;
using QuarklessLogic.Logic.TimelineEventLogLogic;

namespace QuarklessLogic.Logic.ResponseLogic
{
	public class ResponseResolver : IResponseResolver
	{
		private IAPIClientContainer _client;
		private readonly IReportHandler _reportHandler;
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private readonly ITimelineEventLogLogic _timelineEventLogLogic;
		public ResponseResolver(IAPIClientContainer client, IReportHandler reportHandler, 
			IInstagramAccountLogic instagramAccountLogic, ITimelineEventLogLogic timelineEventLogLogic)
		{
			_client = client;
			_reportHandler = reportHandler;
			_instagramAccountLogic = instagramAccountLogic;
			_timelineEventLogLogic = timelineEventLogLogic;
			_reportHandler.SetupReportHandler("ResponseResolver");
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
				default: return $"{actionType.ToString()} Made";
			}
		}
		public async Task<IResult<TInput>> WithResolverAsync<TInput>(IResult<TInput> response, ActionType actionType, string request)
		{
			var context = _client.GetContext.InstagramAccount;
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
					break;
				case ResponseType.MediaNotFound:
					break;
				case ResponseType.SomePagesSkipped:
					break;
				case ResponseType.SentryBlock:
					break;
				case ResponseType.Unknown:
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
		private async Task<bool> MarkAsRecentlyDone(params string[] id)
		{
			return false;
		}
		private async Task<IResult<bool>> AcceptConsent()
		{
			try
			{
				return await _client.GetContext.ActionClient.AcceptConsentAsync();
			}
			catch(Exception ee)
			{
				_reportHandler.MakeReport(ee);
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
				_reportHandler.MakeReport(ee);
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
				_reportHandler.MakeReport(ee);
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
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		private InstaChallengeLoginInfo GetChallangeInfo()
		{
			try
			{
				return _client.GetContext.ActionClient.GetChallengeLoginInfo;
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
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
				_reportHandler.MakeReport(ee);
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
				_reportHandler.MakeReport(ee);
				return null;
			}
		}

	}
}
