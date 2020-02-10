using InstagramApiSharp.Classes;
using Quarkless.Models.InstagramAccounts.Interfaces;
using Quarkless.Models.InstagramClient.Interfaces;
using Quarkless.Models.ResponseResolver.Interfaces;
using Quarkless.Models.Timeline.Interfaces;
using System;
using System.Threading.Tasks;
using Quarkless.Models.InstagramAccounts;
using Quarkless.Models.InstagramAccounts.Enums;
using Quarkless.Models.Timeline;
using Quarkless.Models.Timeline.Enums;
using Newtonsoft.Json;
using Quarkless.Models.ReportHandler.Interfaces;
using Quarkless.Models.Lookup.Interfaces;
using Quarkless.Models.Lookup;
using Quarkless.Models.Lookup.Enums;
using System.Linq;
using InstagramApiSharp.Classes.Models;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.InstagramClient;
using Quarkless.Models.Messaging;
using Quarkless.Models.Proxy.Interfaces;
using Quarkless.Models.ResponseResolver.Enums;
using Quarkless.Models.ResponseResolver.Models;

namespace Quarkless.Logic.ResponseResolver
{
	public class ResponseResolver : IResponseResolver
	{
		#region Init
		private readonly ILookupLogic _lookupLogic;
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private readonly ITimelineEventLogLogic _timelineEventLogLogic;
		private readonly IReportHandler _reportHandler;
		private readonly IProxyRequest _proxyRequest;

		private int _attempts = 0;
		private TimeSpan _intervalBetweenRequests;
		private IApiClientContainer _client;
		private IInstaClient _instaClient;
		private string AccountId { get; set; }
		private string InstagramAccountId { get; set; }

		public ResponseResolver(IApiClientContainer client, IInstagramAccountLogic instagramAccountLogic,
			ITimelineEventLogLogic timelineEventLogLogic, IReportHandler reportHandler, 
			ILookupLogic lookup, IProxyRequest proxyRequest)
		{
			_client = client;
			_instagramAccountLogic = instagramAccountLogic;
			_timelineEventLogLogic = timelineEventLogLogic;
			_reportHandler = reportHandler;
			_reportHandler.SetupReportHandler(nameof(ResponseResolver));
			_lookupLogic = lookup;
			_proxyRequest = proxyRequest;
			_intervalBetweenRequests = TimeSpan.FromSeconds(1.25);
		}
		#endregion

		#region With
		public IResponseResolver WithClient(IApiClientContainer client)
		{
			_client = client;
			return this;
		}
		public IResponseResolver WithInstaApiClient(IInstaClient client)
		{
			_instaClient = client;
			return this;
		}
		public IResponseResolver WithAttempts(int numberOfAttemptsPerRequest = 0, TimeSpan? intervalBetweenRequests = null)
		{
			_attempts = numberOfAttemptsPerRequest;

			if (intervalBetweenRequests.HasValue)
				_intervalBetweenRequests = intervalBetweenRequests.Value;

			return this;
		}
		#endregion

		#region Client Functions
		private async Task<IResult<bool>> AcceptConsent()
		{
			try
			{
				if (Context?.ActionClient == null)
					return await _instaClient.ReturnClient.AcceptConsentAsync();

				return await Context.ActionClient.AcceptConsentAsync();
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				await _reportHandler.MakeReport(err);
				return null;
			}
		}
		private async Task<IResult<bool>> AcceptChallenge()
		{
			try
			{
				if (Context?.ActionClient == null)
					return await _instaClient.ReturnClient.AcceptChallengeAsync();
				return await Context.ActionClient.AcceptChallengeAsync();
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				await _reportHandler.MakeReport(err);
				return null;
			}
		}
		private async Task<IResult<InstaChallengeRequireEmailVerify>> RequestVerifyCodeToEmailForChallengeRequireAsync()
		{
			try
			{
				if (Context?.ActionClient == null)
					return await _instaClient.ReturnClient.RequestVerifyCodeToEmailForChallengeRequireAsync();
				return await Context.ActionClient.RequestVerifyCodeToEmailForChallengeRequireAsync();
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				await _reportHandler.MakeReport(err);
				return null;
			}
		}
		private async Task<IResult<InstaChallengeRequireSMSVerify>> RequestVerifyCodeToSmsForChallengeRequireAsync()
		{
			try
			{
				if (Context?.ActionClient == null)
					return await _instaClient.ReturnClient.RequestVerifyCodeToSMSForChallengeRequireAsync();

				return await Context.ActionClient.RequestVerifyCodeToSMSForChallengeRequireAsync();
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				await _reportHandler.MakeReport(err);
				return null;
			}
		}
		private InstaChallengeLoginInfo GetChallengeInfo()
		{
			try
			{
				if (Context?.ActionClient == null)
					return _instaClient.ReturnClient.ChallengeLoginInfo;

				return Context.ActionClient.ChallengeLoginInfo;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				_reportHandler.MakeReport(err).GetAwaiter().GetResult();
				return null;
			}
		}
		private async Task<IResult<InstaLoggedInChallengeDataInfo>> GetLoggedInChallengeInfo()
		{
			try
			{
				if (Context?.ActionClient == null)
					return await _instaClient.ReturnClient.GetLoggedInChallengeDataInfoAsync();

				return await Context.ActionClient.GetLoggedInChallengeDataInfoAsync();
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				await _reportHandler.MakeReport(err);
				return null;
			}
		}
		private async Task<IResult<InstaChallengeRequireVerifyMethod>> ResetChallenge()
		{
			try
			{
				if (Context?.ActionClient == null)
				{
					return await _instaClient.ReturnClient.ResetChallengeRequireVerifyMethodAsync();
				}

				return await Context.ActionClient.ResetChallengeRequireVerifyMethodAsync();
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				await _reportHandler.MakeReport(err);
				return null;
			}
		}
		private async Task<IResult<InstaChallengeRequireVerifyMethod>> GetChallengeRequireVerifyMethodAsync()
		{
			try
			{
				if (Context?.ActionClient == null)
				{
					return await _instaClient.ReturnClient.GetChallengeRequireVerifyMethodAsync();
				}

				return await Context.ActionClient.GetChallengeRequireVerifyMethodAsync();
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				await _reportHandler.MakeReport(err);
				return null;
			}
		}
		private async Task<IResult<InstaLoginResult>> VerifyCodeForChallengeRequireAsync(string code)
		{
			try
			{
				if (Context?.ActionClient == null)
					return await _instaClient.ReturnClient.VerifyCodeForChallengeRequireAsync(code);
				return await Context.ActionClient.VerifyCodeForChallengeRequireAsync(code);
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				await _reportHandler.MakeReport(err);
				return null;
			}
		}
		private async Task<bool> IsUserLoggedIn()
		{
			try
			{
				return Context?.ActionClient?.IsUserAuthenticated 
				       ?? _instaClient.ReturnClient.IsUserAuthenticated;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				await _reportHandler.MakeReport(err);
				return false;
			}
		}
		#endregion

		#region Account Functions
		private async Task UpdateAccountState(InstagramAccountModel newState)
		{
			var context = Context.InstagramAccount;
			if (context == null)
				return;

			await _instagramAccountLogic.PartialUpdateInstagramAccount(context.AccountId, context.Id, newState);
		}
		private string CreateMessage(ActionType actionType)
		{
			switch (actionType)
			{
				case ActionType.CreatePost:
					return "New Post Created";
				case ActionType.WatchStory:
					return "Watched story of a user who matches your interests";
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
		private async Task MessagingHandler(ActionType actionType, string request)
		{
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
				case ActionType.None:
					break;
			}
		}
		private async Task<bool> AddBlockedAction(ActionType actionType)
		{
			if (actionType == ActionType.None)
				return false;

			if (Context?.InstagramAccount == null)
				return false;

			return await _instagramAccountLogic.AddBlockedAction(Context.InstagramAccount.Id, actionType);
		}
		private async Task<bool> RemoveBlockedAction(ActionType actionType)
		{
			if (actionType == ActionType.None)
				return false;

			if (Context?.InstagramAccount == null)
				return false;

			return await _instagramAccountLogic.RemoveBlockedAction(Context.InstagramAccount.Id, actionType);
		}
		private async Task BlockedActionHandle(ActionType actionType)
		{
			if (Context?.InstagramAccount == null)
				return;

			var context = Context.InstagramAccount;

			if (context.AgentState == (int)AgentState.Blocked
				|| context.AgentState == (int)AgentState.AwaitingActionFromUser
				|| context.AgentState == (int)AgentState.Challenge)
			{
				await UpdateAccountState(new InstagramAccountModel
				{
					AgentState = (int)AgentState.Running
				});
				await RemoveBlockedAction(actionType);
			}
		}
		private ContextContainer Context
			=> _client?.GetContext?.Container;
		#endregion


		#region Static Functions

		public async Task CheckBlockStates(ShortInstagramAccountModel account)
		{
			if (account.BlockedActions != null && account.BlockedActions.Any())
			{
				foreach (var blockedAction in account.BlockedActions)
				{
					if (DateTime.UtcNow > blockedAction.DateBlocked)
					{
						await _instagramAccountLogic.RemoveBlockedAction(account.Id, blockedAction.ActionType);
					}
				}
			}

			if (account.AgentState == (int)AgentState.Blocked)
			{
				await UpdateAccountState(new InstagramAccountModel
				{
					AgentState = (int)AgentState.DeepSleep,
					SleepTimeRemaining = DateTime.UtcNow.AddHours(2)
				});
			}
		}

		#endregion


		private async Task<ResponseHandlerResults> ResponseHandler(ResultInfo info,
			ActionType actionType = ActionType.None, string request = null)
		{
			var context = _client.GetContext.Container.InstagramAccount;
			AccountId = context.AccountId;
			InstagramAccountId = context.Id;

			var response = new ResponseHandlerResults
			{
				ResponseType = info.ResponseType
			};

			await CheckBlockStates(context);

			switch (info.ResponseType)
			{
				case ResponseType.AlreadyLiked:
				{
					response.Resolved = true;
					await RemoveBlockedAction(actionType);
					break;
				}
				case ResponseType.CantLike:
				{
					response.Resolved = true;
					await RemoveBlockedAction(actionType);
					break;
				}
				case ResponseType.ChallengeRequired:
				{
					var challengeInfo = await ChallengeHandle(info);
					response.ChallengeResponse = challengeInfo;
					switch (challengeInfo.ResponseCode)
					{
						case ChallengeResponse.Ok:
							response.Resolved = true;
							break;
						case ChallengeResponse.Unknown:
							await UpdateAccountState(new InstagramAccountModel
							{
								AgentState = (int) AgentState.AwaitingActionFromUser
							});
							break;
						case ChallengeResponse.Captcha:
						case ChallengeResponse.RequireSmsCode:
						case ChallengeResponse.RequireEmailCode:
						case ChallengeResponse.RequirePhoneNumber:
							await UpdateAccountState(new InstagramAccountModel
							{
								AgentState = (int) AgentState.AwaitingActionFromUser,
								ChallengeInfo = new ChallengeCodeRequestResponse
								{
									Verify = challengeInfo.VerifyMethod.GetDescription(),
									Details = challengeInfo.StepDataEmailOrPhone,
									ChallengePath = challengeInfo.ChallengeData
								}
							});
							response.UpdatedAccountState = true;
							break;
					}
					break;
				}
				case ResponseType.CheckPointRequired:
				{
					break;
				}
				case ResponseType.CommentingIsDisabled:
				{
					response.Resolved = true;
					await RemoveBlockedAction(actionType);
					break;
				}
				case ResponseType.ConsentRequired:
				{
					await AcceptConsent();
					response.Resolved = true;
					break;
				}
				case ResponseType.DeletedPost:
				{
					response.Resolved = true;
					await RemoveBlockedAction(actionType);
					break;
				}
				case ResponseType.InactiveUser:
				{
					response.Resolved = true;
					await RemoveBlockedAction(actionType);
					break;
				}
				case ResponseType.LoginRequired:
				{
					var results = await WithResolverAsync(await Context.InstaClient.TryLogin());
					if (results.Response.Succeeded)
					{
						var newState = JsonConvert.DeserializeObject<StateData>(results.Response.Value);
						await UpdateAccountState(new InstagramAccountModel
						{
							State = newState
						});
						response.UpdatedAccountState = true;
					}
					response.Resolved = true;
					break;
				}
				case ResponseType.InternalException:
				{
					break;
				}
				case ResponseType.ActionBlocked:
				case ResponseType.Spam:
				case ResponseType.RequestsLimit:
					response.UpdatedAccountState = true;
					var result = await AddBlockedAction(actionType);
					response.Resolved = result;
					break;
				case ResponseType.OK:
				{
					await BlockedActionHandle(actionType);
					await MessagingHandler(actionType, request);
					response.Resolved = true;
					break;
				}
				case ResponseType.MediaNotFound:
				{
					response.Resolved = true;
					await RemoveBlockedAction(actionType);
					break;
				}
				case ResponseType.SomePagesSkipped:
				{
					response.Resolved = true;
					await RemoveBlockedAction(actionType);
					break;
				}
				case ResponseType.SentryBlock:
				{
					break;
				}
				case ResponseType.Unknown:
				{
					await RemoveBlockedAction(actionType);
					if (info.NeedsChallenge)
					{
						var challengeInfo = await ChallengeHandle(info);
						response.ChallengeResponse = challengeInfo;
						switch (challengeInfo.ResponseCode)
						{
							case ChallengeResponse.Ok:
								response.Resolved = true;
								break;
							case ChallengeResponse.Unknown:
								break;
							case ChallengeResponse.Captcha:
							case ChallengeResponse.RequireSmsCode:
							case ChallengeResponse.RequireEmailCode:
							case ChallengeResponse.RequirePhoneNumber:
								await UpdateAccountState(new InstagramAccountModel
								{
									AgentState = (int)AgentState.Challenge,
									ChallengeInfo = new ChallengeCodeRequestResponse
									{
										Verify = challengeInfo.VerifyMethod.GetDescription(),
										Details = challengeInfo.StepDataEmailOrPhone,
										ChallengePath = challengeInfo.ChallengeData
									}
								});
								response.UpdatedAccountState = true;
								break;
						}
					}
					break;
				}
				case ResponseType.WrongRequest:
					response.Resolved = true;
					break;
				case ResponseType.UnExpectedResponse:
				{
					await TestUserProxy();
					response.Resolved = true;
					break;
				}
				case ResponseType.NetworkProblem:
				{
					await TestUserProxy();
					response.Resolved = true;
					break;
				}
			}
			return response;
		}
		private async Task<ChallengeHandleResponse> ChallengeHandle(ResultInfo info)
		{
			var isLoggedIn = await IsUserLoggedIn();
			if (isLoggedIn)
			{
				var method = await GetLoggedInChallengeInfo();
				if (!method.Succeeded)
				{
					return new ChallengeHandleResponse(ChallengeResponse.Captcha, info)
					{
						IsUserLoggedIn = true,
						VerifyMethod = VerifyMethod.Captcha,
						ChallengeData = GetChallengeInfo()
					};
				}

				// this is me feature
				var challengeAccept = await AcceptChallenge();
				if (challengeAccept.Succeeded)
				{
					return new ChallengeHandleResponse(ChallengeResponse.Ok, info)
					{
						IsUserLoggedIn = true
					};
				}

				return challengeAccept.Info.NeedsChallenge
					? new ChallengeHandleResponse(ChallengeResponse.Captcha, info)
					{
						VerifyMethod = VerifyMethod.Captcha,
						IsUserLoggedIn = true,
						ChallengeData = GetChallengeInfo()
					}
					: new ChallengeHandleResponse(ChallengeResponse.Unknown, info)
					{
						IsUserLoggedIn = true
					};

				// most likely captcha
			}
			else
			{
				var method = await GetChallengeRequireVerifyMethodAsync();

				if (!method.Succeeded)
				{
					var reset = await ResetChallenge();
					if (!reset.Succeeded)
					{
						return reset.Info.NeedsChallenge
							? new ChallengeHandleResponse(ChallengeResponse.Captcha, info)
							{
								VerifyMethod = VerifyMethod.Captcha,
								ChallengeData = GetChallengeInfo()
							}
							: new ChallengeHandleResponse(ChallengeResponse.Unknown, info);
					}
					method = reset;
				}

				if (method.Value.SubmitPhoneRequired)
				{
					return new ChallengeHandleResponse(ChallengeResponse.RequirePhoneNumber, info)
					{
						ChallengeData = GetChallengeInfo(),
						VerifyMethod = VerifyMethod.Phone
					};
				}

				if (method.Value.StepData == null)
				{
					if (method.Value.StepName == "verify_code")
					{
						return new ChallengeHandleResponse(ChallengeResponse.RequireSmsCode, info)
						{
							StepDataEmailOrPhone = "phone number",
							ChallengeData = GetChallengeInfo(),
							VerifyMethod = VerifyMethod.Sms
						};
					}
					return new ChallengeHandleResponse(ChallengeResponse.Unknown, info);
				}

				if (!string.IsNullOrEmpty(method.Value.StepData.PhoneNumber))
				{
					//send code to phone via sms
					var reset = await ResetChallenge();
					if (!reset.Succeeded)
					{
						return reset.Info.NeedsChallenge
							? new ChallengeHandleResponse(ChallengeResponse.Captcha, info)
							{
								VerifyMethod = VerifyMethod.Captcha,
								ChallengeData = GetChallengeInfo()
							}
							: new ChallengeHandleResponse(ChallengeResponse.Unknown, info);
					}
					var request = await RequestVerifyCodeToSmsForChallengeRequireAsync();
					if (request.Succeeded)
					{
						return new ChallengeHandleResponse(ChallengeResponse.RequireSmsCode, info)
						{
							StepDataEmailOrPhone = request.Value.StepData.PhoneNumberPreview,
							ChallengeData = GetChallengeInfo(),
							VerifyMethod = VerifyMethod.Sms
						};
					}

					return request.Info.NeedsChallenge 
						? new ChallengeHandleResponse(ChallengeResponse.Captcha, info)
						{
							VerifyMethod = VerifyMethod.Captcha,
							ChallengeData = GetChallengeInfo()
						}
						: new ChallengeHandleResponse(ChallengeResponse.Unknown, info);
				}
				if (string.IsNullOrEmpty(method.Value.StepData.Email)) 
					return new ChallengeHandleResponse(ChallengeResponse.Captcha, info)
					{
						VerifyMethod = VerifyMethod.Captcha,
						ChallengeData = GetChallengeInfo()
					};
				{
					//send code to email
					var reset = await ResetChallenge();
					if (!reset.Succeeded)
					{
						return reset.Info.NeedsChallenge
							? new ChallengeHandleResponse(ChallengeResponse.Captcha, info)
							{
								VerifyMethod = VerifyMethod.Captcha,
								ChallengeData = GetChallengeInfo()
							}
							: new ChallengeHandleResponse(ChallengeResponse.Unknown, info);
					}
					var request = await RequestVerifyCodeToEmailForChallengeRequireAsync();
					if (request.Succeeded)
					{ 
						return new ChallengeHandleResponse(ChallengeResponse.RequireEmailCode, info)
						{
							StepDataEmailOrPhone = request.Value.StepData.ContactPoint,
							ChallengeData = GetChallengeInfo(),
							VerifyMethod = VerifyMethod.Email
						};
					}

					return request.Info.NeedsChallenge
						? new ChallengeHandleResponse(ChallengeResponse.Captcha, info)
						{
							VerifyMethod = VerifyMethod.Captcha,
							ChallengeData = GetChallengeInfo()
						}
						: new ChallengeHandleResponse(ChallengeResponse.Unknown, info);
				}
			}
		}
		
		#region Resolver Implementation
		public async Task<ResolverResponse<TInput>> WithResolverAsync<TInput>(Func<Task<IResult<TInput>>> func)
		{
			ResolverResponse<TInput> results;
			var currentAttempts = 0;
			do
			{
				results = await WithResolverAsync(await func());
				if (results.Response.Succeeded) return results;

				await Task.Delay(_intervalBetweenRequests);
				currentAttempts++;
			} while (currentAttempts <= _attempts);

			return results;
		}

		public async Task<ResolverResponse<TInput>> WithResolverAsync<TInput>(Func<Task<IResult<TInput>>> func,
			ActionType actionType, string request)
		{
			ResolverResponse<TInput> results;
			var currentAttempts = 0;
			do
			{
				results = await WithResolverAsync(await func(), actionType, request);
				if (results.Response.Succeeded) return results;

				await Task.Delay(_intervalBetweenRequests);
				currentAttempts++;
			} while (currentAttempts <= _attempts);

			return results;
		}

		private async Task<ResolverResponse<TInput>> WithResolverAsync<TInput>(IResult<TInput> response, ActionType actionType,
			string request)
		{
			var account = Context.InstagramAccount;
			AccountId = account.AccountId;
			InstagramAccountId = account.Id;

			var resolverResponse = new ResolverResponse<TInput>
			{
				Response = response,
			};

			if (response?.Info == null)
			{
				await _timelineEventLogLogic.AddTimelineLogFor(new TimelineEventLog
				{
					_id = Guid.NewGuid().ToString(),
					AccountID = account.AccountId,
					InstagramAccountID = account.Id,
					ActionType = actionType,
					DateAdded = DateTime.UtcNow,
					Level = 3,
					Request = request,
					Response = JsonConvert.SerializeObject(response),
					Status = TimelineEventStatus.ServerError,
					Message = "Something is not going as expected right now, we're sorry"
				});
				return resolverResponse;
			};

			if (response.Succeeded)
			{
				await _timelineEventLogLogic.AddTimelineLogFor(new TimelineEventLog
				{
					_id = Guid.NewGuid().ToString(),
					AccountID = account.AccountId,
					InstagramAccountID = account.Id,
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
					_id = Guid.NewGuid().ToString(),
					AccountID = account.AccountId,
					InstagramAccountID = account.Id,
					ActionType = actionType,
					DateAdded = DateTime.UtcNow,
					Level = 2,
					Request = request,
					Response = JsonConvert.SerializeObject(response),
					Status = TimelineEventStatus.Failed,
					Message = $"Failed to carry out {actionType.ToString()} action"
				});
			}
			var results = await ResponseHandler(response.Info, actionType, request);
			resolverResponse.HandlerResults = results;
			return resolverResponse;
		}

		/// <summary>
		/// Please use 'WithClient' before this
		/// </summary>
		/// <typeparam name="TInput"></typeparam>
		/// <param name="response"></param>
		/// <returns></returns>
		public async Task<ResolverResponse<TInput>> WithResolverAsync<TInput>(IResult<TInput> response)
		{
			var account = Context.InstagramAccount;
			AccountId = account.AccountId;
			InstagramAccountId = account.Id;

			var resolverResponse = new ResolverResponse<TInput>
			{
				Response = response
			};

			if (response?.Info == null) return resolverResponse;
			var results = await ResponseHandler(response.Info);
			resolverResponse.HandlerResults = results;
			return resolverResponse;
		}
		#endregion

		#region Other Functionality

		private async Task TestUserProxy()
		{
			var account = Context?.InstagramAccount;
			if (account == null)
				return;
			if (Context.Proxy == null)
				return;
			try
			{
				var testProxyConnectivity = await _proxyRequest.TestConnectivity(Context.Proxy);
				if (!testProxyConnectivity)
				{
					await _proxyRequest.AssignProxy(account.AccountId, account.Id,
						Context.Proxy.Location.LocationQuery);
				}
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
			}
		}
		private async Task MarkAsComplete(ActionType actionType, params string[] ids)
		{
			foreach (var id in ids)
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
						LookupStatus = LookupStatus.Completed,
						ActionType = actionType
					});
				}
				catch (Exception err)
				{
					Console.WriteLine(err);
					await _reportHandler.MakeReport(err);
				}
			}
		}
		#endregion
	}
}
