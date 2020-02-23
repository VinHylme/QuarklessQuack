using InstagramApiSharp.Classes;
using Quarkless.Models.InstagramAccounts.Interfaces;
using Quarkless.Models.InstagramClient.Interfaces;
using Quarkless.Models.ResponseResolver.Interfaces;
using Quarkless.Models.Timeline.Interfaces;
using System;
using System.Threading.Tasks;
using Quarkless.Models.InstagramAccounts;
using Quarkless.Models.InstagramAccounts.Enums;
using Newtonsoft.Json;
using Quarkless.Models.ReportHandler.Interfaces;
using Quarkless.Models.Lookup.Interfaces;
using Quarkless.Models.Lookup;
using Quarkless.Models.Lookup.Enums;
using System.Linq;
using InstagramApiSharp.Classes.Models;
using Quarkless.Base.InstagramUser.Models;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Interfaces;
using Quarkless.Models.InstagramClient;
using Quarkless.Models.Media;
using Quarkless.Models.Messaging;
using Quarkless.Models.Notification.Enums;
using Quarkless.Models.Notification.Extensions;
using Quarkless.Models.Notification.Interfaces;
using Quarkless.Models.Proxy.Interfaces;
using Quarkless.Models.ResponseResolver.Enums;
using Quarkless.Models.ResponseResolver.Models;
using Quarkless.Models.Stories;
using Quarkless.Models.Comments;
using Quarkless.Models.Common.Models.Resolver;

namespace Quarkless.Logic.ResponseResolver
{
	public class ResponseResolver : IResponseResolver
	{
		public struct MessageCreated
		{
			public string Message { get; set; }
			public string AssetUrl { get; set; }
			public MediaShort Media { get; set; }
			public MessageCreated(string message, MediaShort media, string assetUrl = "")
			{
				this.Message = message;
				this.AssetUrl = assetUrl;
				this.Media = media;
			}
		}

		#region Init
		private readonly ILookupLogic _lookupLogic;
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private readonly INotificationLogic _notificationLogic;
		private readonly IReportHandler _reportHandler;
		private readonly IProxyRequest _proxyRequest;

		private int _attempts = 0;
		private TimeSpan _intervalBetweenRequests;
		private IApiClientContainer _client;
		private IInstaClient _instaClient;
		private string AccountId { get; set; }
		private string InstagramAccountId { get; set; }

		public ResponseResolver(IApiClientContainer client, IInstagramAccountLogic instagramAccountLogic, 
			IReportHandler reportHandler, ILookupLogic lookup, IProxyRequest proxyRequest,
			INotificationLogic notificationLogic)
		{
			_client = client;
			_instagramAccountLogic = instagramAccountLogic;
			_reportHandler = reportHandler;
			_reportHandler.SetupReportHandler(nameof(ResponseResolver));
			_lookupLogic = lookup;
			_proxyRequest = proxyRequest;
			_notificationLogic = notificationLogic;
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

		private async Task CreateNotification(string assetUrl, MediaShort media, string message, ResponseType responseType,
			ActionType actionType, string responseMessage, TimelineEventItemStatus status)
		{
			var context = Context.InstagramAccount;
			if (context == null)
				return;

			await _notificationLogic
				.AddTimelineActionNotification(NotificationExtensions
					.CreateTimelineNotificationObject(context.AccountId, context.Id, assetUrl, media,
						message, actionType, (int) responseType, responseMessage, status));
		}

		private MessageCreated CreateMessage(ActionType actionType, IExec executedRequest)
		{
			switch (actionType)
			{
				case ActionType.RefreshLogin:
				{
					return new MessageCreated("Refreshed Account and cleared cookies", null);
				}
				case ActionType.CreatePost:
				{
					MessageCreated messageCreated;
					var message = "Created a new {0} post";
					switch (executedRequest)
					{
						case UploadPhotoModel model:
							messageCreated = 
								new MessageCreated(string.Format(message, "Photo"), new MediaShort
									{
										Caption = model.MediaInfo.Caption,
										Hashtags = model.MediaInfo.Hashtags,
										MediaType = (int) model.MediaInfo.MediaType,
										MediaUrl = model.Image.Uri
									}, Context?.InstagramAccount?.ProfilePicture);
							break;
						case UploadAlbumModel model:
							messageCreated =
								new MessageCreated(string.Format(message, "Album"),
									new MediaShort
									{
										Caption = model.MediaInfo.Caption,
										Hashtags = model.MediaInfo.Hashtags,
										MediaType = (int) model.MediaInfo.MediaType,
										MediaUrl = model.Album[0].ImageToUpload != null
											? model.Album[0].ImageToUpload.Uri
											: model.Album[0].VideoToUpload.VideoThumbnail.Uri
									},
									Context?.InstagramAccount?.ProfilePicture);
							break;
						case UploadVideoModel model:
							messageCreated =
								new MessageCreated(string.Format(message, "Video"),
									new MediaShort
									{
										Caption = model.MediaInfo.Caption,
										Hashtags = model.MediaInfo.Hashtags,
										MediaType = (int) model.MediaInfo.MediaType,
										MediaUrl = model.Video.VideoThumbnail.Uri
									},
									Context?.InstagramAccount?.ProfilePicture);
							break;
						default: throw new ArgumentOutOfRangeException();
					}

					return messageCreated;
				}
				case ActionType.WatchStory:
				{
					if(!(executedRequest is StoryRequest exec)) throw new Exception();
					return new MessageCreated($"Watched story from {exec.User.Username} who happens to matches your interests",
						exec.Media, exec.User.ProfilePicture);
				}
				case ActionType.ReactStory:
				{
					if (!(executedRequest is StoryRequest exec)) throw new Exception();
					return new MessageCreated($"Reacted {exec.User.Username}'s story",
						exec.Media, exec.User.ProfilePicture);
				}
				case ActionType.CreateCommentMedia:
				{
					if (!(executedRequest is CreateCommentRequest exec)) throw new Exception();
					return new MessageCreated($"Posted Comment: {exec.Text} on {exec.User.Username}'s post",
						exec.Media,
						exec.User.ProfilePicture);
				}
				case ActionType.CreateBiography:
				{
					return new MessageCreated("You have updated your biography",
						null, Context?.InstagramAccount?.ProfilePicture);
				}
				case ActionType.CreateCommentReply:
				{
					if (!(executedRequest is CreateCommentRequest exec)) throw new Exception();
					return new MessageCreated($"Replied to {exec.User.Username}'s comment",
						exec.Media,
						exec.User.ProfilePicture);
				}
				case ActionType.CreateStory:
				{
					return new MessageCreated("Added a new story", null, Context?.InstagramAccount?.ProfilePicture);
				}
				case ActionType.FollowHashtag:
				{
					return new MessageCreated("Followed Hashtag, based on your profile", null);
				}
				case ActionType.FollowUser:
				{
					if (!(executedRequest is FollowAndUnFollowUserRequest exec)) throw new Exception();
					return new MessageCreated($"Followed {exec.User.Username} who matches your profile",
						null, exec.User.ProfilePicture);
				}
				case ActionType.UnFollowUser:
				{
					if (!(executedRequest is FollowAndUnFollowUserRequest exec)) throw new Exception();
					return new MessageCreated($"UnFollowed {exec.User.Username}", null, exec.User.ProfilePicture);
				}
				case ActionType.LikeComment:
				{
					if(!(executedRequest is LikeCommentRequest exec)) throw new Exception();
					return new MessageCreated($"Liked {exec.User.Username}'s comment", null, exec.User.ProfilePicture);
				}
				case ActionType.LikePost:
				{
					if (!(executedRequest is LikeMediaModel exec)) throw new Exception();
					return new MessageCreated($"Liked {exec.User.Username}'s Post",
						exec.Media,
						Context?.InstagramAccount?.ProfilePicture);
				}
				case ActionType.SendDirectMessageVideo:
				{
					if (!(executedRequest is SendDirectVideoModel exec)) throw new Exception();
					return new MessageCreated($"Sent a direct video to {string.Join(",", exec.Recipients)}", null);
				}
				case ActionType.SendDirectMessagePhoto:
				{
					if (!(executedRequest is SendDirectPhotoModel exec)) throw new Exception();
					return new MessageCreated($"Sent a direct photo to {string.Join(",", exec.Recipients)}", null);
				}
				case ActionType.SendDirectMessageText:
				{
					if (!(executedRequest is SendDirectTextModel exec)) throw new Exception();
					return new MessageCreated($"Sent a direct text message to {string.Join(",", exec.Recipients)}", null);
				}
				case ActionType.SendDirectMessageLink:
				{
					if (!(executedRequest is SendDirectLinkModel exec)) throw new Exception();
					return new MessageCreated($"Sent a direct link to {string.Join(",", exec.Recipients)}", null);
				}
				case ActionType.SendDirectMessageProfile:
				{
					if (!(executedRequest is SendDirectVideoModel exec)) throw new Exception();
					return new MessageCreated($"Shared profile to {string.Join(",", exec.Recipients)}", null);
				}
				default: return new MessageCreated($"{actionType.ToString()} Made", null);
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
		private ContextContainer Context => _client?.GetContext?.Container;
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
					var resultFromProxy = await TestUserProxy();

					if (!resultFromProxy.IsSuccessful)
					{
						var events = await _notificationLogic
							.OccurrencesByResponseType(context.AccountId, context.Id, 15,
								(int) ResponseType.UnExpectedResponse,
								(int) ResponseType.NetworkProblem);

						if (events > 3)
						{
							await UpdateAccountState(new InstagramAccountModel
							{
								AgentState = (int)AgentState.BadProxy
							});
						}
					}
					response.Resolved = true;
					break;
				}
				case ResponseType.NetworkProblem:
				{
					var resultFromProxy = await TestUserProxy();
					if (!resultFromProxy.IsSuccessful)
					{
						var events = await _notificationLogic
							.OccurrencesByResponseType(context.AccountId, context.Id, 15,
								(int)ResponseType.UnExpectedResponse,
								(int)ResponseType.NetworkProblem);

						if (events > 3)
						{
							await UpdateAccountState(new InstagramAccountModel
							{
								AgentState = (int)AgentState.BadProxy
							});
						}
					}
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
		public async Task<ResolverResponse<TInput>> WithResolverAsync<TInput>(Func<Task<IResult<TInput>>> func, 
			ActionType actionType, IExec request)
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

		private async Task<ResolverResponse<TInput>> WithResolverAsync<TInput>(IResult<TInput> response,
			ActionType actionType, IExec request)
		{
			var account = Context.InstagramAccount;
			AccountId = account.AccountId;
			InstagramAccountId = account.Id;
			var resolverResponse = new ResolverResponse<TInput>
			{
				Response = response,
			};

			var actionMessage = CreateMessage(actionType, request);

			if (response?.Info == null)
			{
				await CreateNotification(actionMessage.AssetUrl, actionMessage.Media, "Something is not going as expected right now, we're sorry",
					ResponseType.InternalException, actionType, "response is null",
					TimelineEventItemStatus.ServerError);

				return resolverResponse;
			};

			if (response.Succeeded)
			{
				await CreateNotification(actionMessage.AssetUrl, actionMessage.Media, actionMessage.Message,
					response.Info.ResponseType, actionType, response.Info.Message,
					TimelineEventItemStatus.Success);
			}
			else
			{
				await CreateNotification(actionMessage.AssetUrl, actionMessage.Media, $"Failed to carry out {actionType.ToString()} action",
					response.Info.ResponseType, actionType, response.Info.Message,
					TimelineEventItemStatus.Failed);
			}

			var results = await ResponseHandler(response.Info, actionType, request.ToJsonString());
			resolverResponse.HandlerResults = results;
			return resolverResponse;
		}

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

		private async Task<ProxyTestResponse> TestUserProxy()
		{
			var response = new ProxyTestResponse();

			var account = Context?.InstagramAccount;

			if (account == null)
			{
				response.Reason = "Failed to retrieve Account";
				return response;
			}
			if (Context.Proxy == null)
			{
				response.Reason = "Failed to retrieve user proxy";
				return response;
			}

			try
			{
				var testProxyConnectivity = await _proxyRequest.TestConnectivity(Context.Proxy);
				if (!testProxyConnectivity)
				{
					if (Context.Proxy.FromUser)
					{
						response.IsSuccessful = false;
						response.AttemptedToReAssign = false;
						response.ProxyIsFromUser = true;
						response.Reason = "User's Proxy Failed to Connect";
						return response;
					}

					var assignedResult = await _proxyRequest.AssignProxy(account.AccountId, account.Id,
						Context.Proxy.Location.LocationQuery);

					if (assignedResult == null)
					{
						response.IsSuccessful = false;
						response.AttemptedToReAssign = true;
						response.Reason = "Failed to assign proxy";
						response.ProxyIsFromUser = false;
						return response;
					}

					response.IsSuccessful = true;
					response.AttemptedToReAssign = true;
					response.ProxyIsFromUser = false;
					return response;
				}

				response.IsSuccessful = true;
				response.AttemptedToReAssign = false;
				response.Reason = "Connectivity Good";
				return response;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				response.Reason = err.Message;
				return response;
			}
		}
		private async Task MarkAsComplete(ActionType actionType, params string[] ids)
		{
			foreach (var id in ids)
			{
				try
				{
					var getObj = (await _lookupLogic.Get(AccountId, InstagramAccountId))
						.FirstOrDefault(_=>_.ObjectId == id);

					if (getObj == null)
						continue;

					await _lookupLogic.UpdateObjectToLookup(AccountId, InstagramAccountId, getObj,
						new LookupModel(id)
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
