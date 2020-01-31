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
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Messaging;
using Quarkless.Models.Proxy.Interfaces;

namespace Quarkless.Logic.ResponseResolver
{
	public class ResponseResolver : IResponseResolver
	{
		private readonly ILookupLogic _lookupLogic;
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private readonly ITimelineEventLogLogic _timelineEventLogLogic;
		private readonly IReportHandler _reportHandler;
		private readonly IProxyRequest _proxyRequest;

		private int _attempts = 0;
		private TimeSpan _intervalBetweenRequests;
		private IApiClientContainer _client;
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

		public IResponseResolver WithClient(IApiClientContainer client)
		{
			_client = client;
			return this;
		}
		public IResponseResolver WithAttempts(int numberOfAttemptsPerRequest = 0, TimeSpan? intervalBetweenRequests = null)
		{
			_attempts = numberOfAttemptsPerRequest;

			if (intervalBetweenRequests.HasValue)
				_intervalBetweenRequests = intervalBetweenRequests.Value;

			return this;
		}

		private async Task UpdateAccountState(InstagramAccountModel newState)
		{
			var context = _client?.GetContext?.InstagramAccount;
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
				default:
					break;
			}
		}
		private async Task ResponseHandler(ResponseType type, ActionType actionType = ActionType.None,
			string request = null)
		{
			var context = _client.GetContext.InstagramAccount;
			AccountId = context.AccountId;
			InstagramAccountId = context.Id;
			switch (type)
			{
				case ResponseType.AlreadyLiked:
				{
					break;
				}
				case ResponseType.CantLike:
				{
					break;
				}
				case ResponseType.ChallengeRequired:
				{
					var challenge = await GetChallengeRequireVerifyMethodAsync();
					if (challenge.Succeeded)
					{
						if (challenge.Value.SubmitPhoneRequired)
						{

						}
						else
						{
							if (challenge.Value.StepData != null)
							{
								if (!string.IsNullOrEmpty(challenge.Value.StepData.PhoneNumber))
								{
									//verify phone
									var code = await RequestVerifyCodeToSmsForChallengeRequireAsync();
									if (code.Succeeded)
									{
										await UpdateAccountState(new InstagramAccountModel
										{
											AgentState = (int) AgentState.Challenge,
											ChallengeInfo = new ChallengeCodeRequestResponse
											{
												Verify = "phone",
												Details = challenge.Value.StepData.PhoneNumber,
												ChallengePath = GetChallengeInfo()
											}
										});
									}
								}

								if (!string.IsNullOrEmpty(challenge.Value.StepData.Email))
								{
									var code = await RequestVerifyCodeToEmailForChallengeRequireAsync();
									if (code.Succeeded)
									{
										await UpdateAccountState(new InstagramAccountModel
										{
											AgentState = (int) AgentState.Challenge,
											ChallengeInfo = new ChallengeCodeRequestResponse
											{
												Verify = "email",
												Details = challenge.Value.StepData.Email,
												ChallengePath = GetChallengeInfo()
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
				}
				case ResponseType.CheckPointRequired:
				{
					break;
				}
				case ResponseType.CommentingIsDisabled:
				{
					break;
				}
				case ResponseType.ConsentRequired:
				{
					await AcceptConsent();
					break;
				}
				case ResponseType.DeletedPost:
				{
					break;
				}
				case ResponseType.InactiveUser:
				{
					break;
				}
				case ResponseType.LoginRequired:
				{
					var results = await WithResolverAsync(await _client.GetContext.InstaClient.TryLogin());
					if (results.Succeeded)
					{
						var newState = JsonConvert.DeserializeObject<StateData>(results.Value);
						await UpdateAccountState(new InstagramAccountModel
						{
							State = newState
						});
					}

					break;
				}
				case ResponseType.InternalException:
				{
					break;
				}
				case ResponseType.ActionBlocked:
				case ResponseType.Spam:
				case ResponseType.RequestsLimit:
					await UpdateAccountState(new InstagramAccountModel
					{
						AgentState = (int) AgentState.Blocked
					});
					break;
				case ResponseType.OK:
				{
					await MessagingHandler(actionType, request);
					break;
				}
				case ResponseType.MediaNotFound:
				{
					break;
				}
				case ResponseType.SomePagesSkipped:
				{
					break;
				}
				case ResponseType.SentryBlock:
				{
					break;
				}
				case ResponseType.Unknown:
				{
					await _client.GetContext.ActionClient.GetLoggedInChallengeDataInfoAsync();
					await AcceptChallenge();
					break;
				}
				case ResponseType.WrongRequest:
					break;
				case ResponseType.UnExpectedResponse:
				{
					await TestUserProxy();
					break;
				}
				case ResponseType.NetworkProblem:
				{
					await TestUserProxy();
					break;
				}
			}
		}

		public async Task<IResult<TInput>> WithResolverAsync<TInput>(Func<Task<IResult<TInput>>> func)
		{
			IResult<TInput> results;
			var currentAttempts = 0;
			do
			{
				results = await WithResolverAsync(await func());
				if (results.Succeeded) return results;

				await Task.Delay(_intervalBetweenRequests);
				currentAttempts++;
			} while (currentAttempts < _attempts);

			return results;
		}
		public async Task<IResult<TInput>> WithResolverAsync<TInput>(Func<Task<IResult<TInput>>> func,
			ActionType actionType, string request)
		{
			IResult<TInput> results;
			var currentAttempts = 0;
			do
			{
				results = await WithResolverAsync(await func(), actionType, request);
				if (results.Succeeded) return results;

				await Task.Delay(_intervalBetweenRequests);
				currentAttempts++;
			} while (currentAttempts < _attempts);

			return results;
		}

		private async Task<IResult<TInput>> WithResolverAsync<TInput>(IResult<TInput> response, ActionType actionType,
			string request)
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

			await ResponseHandler(response.Info.ResponseType, actionType, request);
			return response;

			#region OldHandler
			// switch (response.Info.ResponseType)
			// {
			// 	case ResponseType.AlreadyLiked:
			// 		break;
			// 	case ResponseType.CantLike:
			// 		break;
			// 	case ResponseType.ChallengeRequired:
			// 		var challenge = await GetChallengeRequireVerifyMethodAsync();
			// 		if (challenge.Succeeded)
			// 		{
			// 			if (challenge.Value.SubmitPhoneRequired)
			// 			{
			//
			// 			}
			// 			else
			// 			{
			// 				if (challenge.Value.StepData != null)
			// 				{
			// 					if (!string.IsNullOrEmpty(challenge.Value.StepData.PhoneNumber))
			// 					{
			// 						//verify phone
			// 						var code = await RequestVerifyCodeToSmsForChallengeRequireAsync();
			// 						if (code.Succeeded)
			// 						{
			// 							await _instagramAccountLogic.PartialUpdateInstagramAccount(context.AccountId,
			// 								context.Id,
			// 								new InstagramAccountModel
			// 								{
			// 									AgentState = (int)AgentState.Challenge,
			// 									ChallengeInfo = new ChallengeCodeRequestResponse
			// 									{
			// 										Verify = "phone",
			// 										Details = challenge.Value.StepData.PhoneNumber,
			// 										ChallengePath = GetChallengeInfo()
			// 									}
			// 								});
			// 						}
			// 					}
			// 					if (!string.IsNullOrEmpty(challenge.Value.StepData.Email))
			// 					{
			// 						var code = await RequestVerifyCodeToEmailForChallengeRequireAsync();
			// 						if (code.Succeeded)
			// 						{
			// 							await _instagramAccountLogic.PartialUpdateInstagramAccount(context.AccountId,
			// 								context.Id,
			// 								new InstagramAccountModel
			// 								{
			// 									AgentState = (int)AgentState.Challenge,
			// 									ChallengeInfo = new ChallengeCodeRequestResponse
			// 									{
			// 										Verify = "email",
			// 										Details = challenge.Value.StepData.Email,
			// 										ChallengePath = GetChallengeInfo()
			// 									}
			// 								});
			// 						}
			// 					}
			// 				}
			// 			}
			// 		}
			// 		await _client.GetContext.ActionClient.GetLoggedInChallengeDataInfoAsync();
			// 		await AcceptChallenge();
			// 		break;
			// 	case ResponseType.CheckPointRequired:
			// 		break;
			// 	case ResponseType.CommentingIsDisabled:
			// 		break;
			// 	case ResponseType.ConsentRequired:
			// 		await AcceptConsent();
			// 		break;
			// 	case ResponseType.DeletedPost:
			// 		break;
			// 	case ResponseType.InactiveUser:
			// 		break;
			// 	case ResponseType.LoginRequired:
			// 		var results = await WithResolverAsync(await _client.GetContext.InstaClient.TryLogin());
			// 		if (results.Succeeded)
			// 		{
			// 			var newState = JsonConvert.DeserializeObject<StateData>(results.Value);
			// 			await _instagramAccountLogic.PartialUpdateInstagramAccount(context.AccountId, context.Id,
			// 				new InstagramAccountModel
			// 				{
			// 					State = newState
			// 				});
			// 		}
			// 		break;
			// 	case ResponseType.InternalException:
			// 		break;
			// 	case ResponseType.ActionBlocked:
			// 	case ResponseType.Spam:
			// 	case ResponseType.RequestsLimit:
			// 		await _instagramAccountLogic.PartialUpdateInstagramAccount(context.AccountId,
			// 			context.Id,
			// 			new InstagramAccountModel
			// 			{
			// 				AgentState = (int)AgentState.Blocked
			// 			});
			// 		break;
			// 	case ResponseType.OK:
			// 		switch (actionType)
			// 		{
			// 			case ActionType.SendDirectMessageMedia:
			// 				await MarkAsComplete(actionType,
			// 					JsonConvert.DeserializeObject<ShareDirectMediaModel>(request).Recipients.ToArray());
			// 				break;
			// 			case ActionType.SendDirectMessageAudio:
			// 				await MarkAsComplete(actionType,
			// 					JsonConvert.DeserializeObject<SendDirectPhotoModel>(request).Recipients.ToArray());
			// 				break;
			// 			case ActionType.SendDirectMessageLink:
			// 				await MarkAsComplete(actionType,
			// 					JsonConvert.DeserializeObject<SendDirectLinkModel>(request).Recipients.ToArray());
			// 				break;
			// 			case ActionType.SendDirectMessagePhoto:
			// 				await MarkAsComplete(actionType,
			// 					JsonConvert.DeserializeObject<SendDirectPhotoModel>(request).Recipients.ToArray());
			// 				break;
			// 			case ActionType.SendDirectMessageProfile:
			// 				await MarkAsComplete(actionType,
			// 					JsonConvert.DeserializeObject<SendDirectProfileModel>(request).Recipients.ToArray());
			// 				break;
			// 			case ActionType.SendDirectMessageText:
			// 				await MarkAsComplete(actionType,
			// 					JsonConvert.DeserializeObject<SendDirectTextModel>(request).Recipients.ToArray());
			// 				break;
			// 			case ActionType.SendDirectMessageVideo:
			// 				await MarkAsComplete(actionType,
			// 					JsonConvert.DeserializeObject<SendDirectVideoModel>(request).Recipients.ToArray());
			// 				break;
			// 		}
			// 		break;
			// 	case ResponseType.MediaNotFound:
			// 		break;
			// 	case ResponseType.SomePagesSkipped:
			// 		break;
			// 	case ResponseType.SentryBlock:
			// 		break;
			// 	case ResponseType.Unknown:
			// 		await _client.GetContext.ActionClient.GetLoggedInChallengeDataInfoAsync();
			// 		await _client.GetContext.ActionClient.AcceptChallengeAsync();
			// 		break;
			// 	case ResponseType.WrongRequest:
			// 		break;
			// 	case ResponseType.UnExpectedResponse:
			// 		break;
			// 	case ResponseType.NetworkProblem:
			// 		break;
			// }
			#endregion
		}
		private async Task<IResult<TInput>> WithResolverAsync<TInput>(IResult<TInput> response)
		{
			var context = _client.GetContext.InstagramAccount;
			AccountId = context.AccountId;
			InstagramAccountId = context.Id;

			if (response?.Info == null) return response;
			await ResponseHandler(response.Info.ResponseType);
			return response;
			#region Old Handle
			/*
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
							if (challenge.Value.StepData != null)
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
													ChallengePath = GetChallengeInfo()
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
												AgentState = (int)AgentState.Challenge,
												ChallengeInfo = new ChallengeCodeRequestResponse
												{
													Verify = "email",
													Details = challenge.Value.StepData.Email,
													ChallengePath = GetChallengeInfo()
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
					var results = await WithResolverAsync(await _client.GetContext.InstaClient.TryLogin());
					if (results.Succeeded)
					{
						var newState = JsonConvert.DeserializeObject<StateData>(results.Value);
						await _instagramAccountLogic.PartialUpdateInstagramAccount(context.AccountId, context.Id,
							new InstagramAccountModel
							{
								State = newState
							});
					}
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
							AgentState = (int)AgentState.Blocked
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
			*/
			#endregion
		}

		#region Other Functionality

		private async Task TestUserProxy()
		{
			var context = _client.GetContext.InstagramAccount;
			try
			{
				var testProxyConnectivity = await _proxyRequest.TestConnectivity(_client.GetContext.Proxy);
				if (!testProxyConnectivity)
				{
					await _proxyRequest.AssignProxy(context.AccountId, context.Id,
						_client.GetContext.Proxy.Location.LocationQuery);
				}
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return;
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
		private async Task<IResult<bool>> AcceptConsent()
		{
			try
			{
				return await _client.GetContext.ActionClient.AcceptConsentAsync();
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
				return await _client.GetContext.ActionClient.AcceptChallengeAsync();
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
				return await _client.GetContext.ActionClient.RequestVerifyCodeToEmailForChallengeRequireAsync();
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
				return await _client.GetContext.ActionClient.RequestVerifyCodeToSMSForChallengeRequireAsync();
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
				return _client.GetContext.ActionClient.ChallengeLoginInfo;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				_reportHandler.MakeReport(err).GetAwaiter().GetResult();
				return null;
			}
		}
		private async Task<IResult<InstaChallengeRequireVerifyMethod>> GetChallengeRequireVerifyMethodAsync()
		{
			try
			{
				return await _client.GetContext.ActionClient.GetChallengeRequireVerifyMethodAsync();
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
				return await _client.GetContext.ActionClient.VerifyCodeForChallengeRequireAsync(code);
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				await _reportHandler.MakeReport(err);
				return null;
			}
		}
		#endregion
	}
}
