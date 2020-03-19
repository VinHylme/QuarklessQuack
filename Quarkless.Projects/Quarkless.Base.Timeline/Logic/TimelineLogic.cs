using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quarkless.Analyser;
using Quarkless.Base.Lookup.Models;
using Quarkless.Base.Lookup.Models.Enums;
using Quarkless.Base.Lookup.Models.Interfaces;
using Quarkless.Base.Media.Models;
using Quarkless.Base.Messaging.Models;
using Quarkless.Base.Messaging.Models.Interfaces;
using Quarkless.Base.RequestBuilder.Models.Interfaces;
using Quarkless.Base.Storage.Models.Interfaces;
using Quarkless.Base.Timeline.Models;
using Quarkless.Base.Timeline.Models.Enums;
using Quarkless.Base.Timeline.Models.Interfaces;
using Quarkless.Base.Timeline.Models.Interfaces.TaskScheduler;
using Quarkless.Base.Timeline.Models.TaskScheduler;
using Quarkless.Common.Timeline.Models;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models;

namespace Quarkless.Base.Timeline.Logic
{
	public class TimelineLogic : ITimelineLogic
	{
		private readonly ITaskService _taskService;
		private readonly IRequestBuilder _requestBuilder;
		private readonly ILookupCache _lookupCache;
		private readonly IS3BucketLogic _s3BucketLogic;
		private readonly IPostAnalyser _postAnalyser;
		private readonly IUrlReader _urlReader;
		public TimelineLogic(ITaskService taskService, IRequestBuilder requestBuilder, 
			ILookupCache lookupCache, IS3BucketLogic bucketLogic, IPostAnalyser postAnalyser, IUrlReader urlReader)
		{
			_taskService = taskService;
			_requestBuilder = requestBuilder;
			_lookupCache = lookupCache;
			_s3BucketLogic = bucketLogic;
			_postAnalyser = postAnalyser;
			_urlReader = urlReader;
		}
		#region Add Event To Queue
		public string AddEventToTimeline(EventActionOptions eventAction)
		{
			var eventId = _taskService.ScheduleEvent(eventAction);
			return eventId;
		}
		public T ParseJsonObject<T>(string json) where T : class, new()
		{
			var jObject = JObject.Parse(json);
			return JsonConvert.DeserializeObject<T>(jObject.ToString());
		}
		private async Task<string> UploadToS3(byte[] media, string keyName)
		{
			await using var mediaStream = new MemoryStream(media);
			return await _s3BucketLogic.UploadStreamFile(mediaStream, keyName);
		}

		public async Task<TimelineScheduleResponse<RawMediaSubmit>> SchedulePostsByUser(UserStoreDetails userStoreDetails,
			RawMediaSubmit dataMediaSubmit)
		{
			var response = new TimelineScheduleResponse<RawMediaSubmit>
			{
				RequestData = dataMediaSubmit,
				IsSuccessful = false
			};

			if (string.IsNullOrEmpty(userStoreDetails.AccountId) ||
				string.IsNullOrEmpty(userStoreDetails.InstagramAccountUser))
				return response;
			try
			{
				var options = new EventActionOptions
				{
					ActionType = (int) ActionType.CreatePost,
					ExecutionTime = dataMediaSubmit.ExecuteTime,
					User = new UserStore
					{
						AccountId = userStoreDetails.AccountId,
						InstagramAccountUser = userStoreDetails.InstagramAccountUser,
						InstagramAccountUsername = userStoreDetails.InstagramAccountUsername
					}
				};
					
				var mediaInfo = new MediaInfo
				{
					Caption = dataMediaSubmit.Caption,
					Hashtags = dataMediaSubmit.Hashtags,
					MediaType = (InstaMediaType) (int) dataMediaSubmit.OptionSelected
				};

				foreach (var media in dataMediaSubmit.RawMediaDatas)
				{
					if (string.IsNullOrEmpty(media.Media64BaseData))
						continue;

					media.UrlToSend = !media.Media64BaseData.IsBase64String()
						? media.Media64BaseData
						: await UploadToS3(Convert.FromBase64String(media.Media64BaseData.Split(',')[1]),
							$"media_self_user{Guid.NewGuid()}");
				}

				switch (dataMediaSubmit.OptionSelected)
				{
					case MediaSelectionType.Image:
						options.ActionDescription += $"_{MediaSelectionType.Image.ToString()}_userPosted";
						options.DataObject = new EventActionOptions.EventBody
						{
							Body = new UploadPhotoModel
							{
								MediaTopic = null,
								MediaInfo = mediaInfo,
								Image = new InstaImageUpload()
								{
									Uri = dataMediaSubmit.RawMediaDatas.FirstOrDefault()?.UrlToSend
								},
								Location = dataMediaSubmit.Location != null
									? new InstaLocationShort
									{
										Address = dataMediaSubmit.Location.Address,
										Name = dataMediaSubmit.Location.City
									}
									: null
							},
							BodyType = typeof(UploadPhotoModel)
						};
						break;
					case MediaSelectionType.Video:
						options.ActionDescription += $"_{MediaSelectionType.Video.ToString()}_userPosted";

						var urlToSend = dataMediaSubmit.RawMediaDatas
							.FirstOrDefault()?.UrlToSend;
						if (urlToSend != null)
							options.DataObject = new EventActionOptions.EventBody
							{
								Body = new UploadVideoModel
								{
									MediaTopic = null,
									MediaInfo = mediaInfo,
									Video = new InstaVideoUpload
									{
										Video = new InstaVideo
										{
											Uri = urlToSend
										},
										VideoThumbnail = new InstaImage
										{
											Uri = await UploadToS3(
												_postAnalyser.Manipulation.VideoEditor.GenerateVideoThumbnail(
													_postAnalyser.Manager.DownloadMedia(urlToSend)),
												$"user_self_videoThumb_{Guid.NewGuid()}")
										}
									},
									Location = dataMediaSubmit.Location != null
										? new InstaLocationShort
										{
											Address = dataMediaSubmit.Location.Address,
											Name = dataMediaSubmit.Location.City
										}
										: null
								},
								BodyType = typeof(UploadVideoModel)
							};
						break;
					case MediaSelectionType.Carousel:
						options.ActionDescription += $"_{MediaSelectionType.Carousel.ToString()}_userPosted";
						options.DataObject = new EventActionOptions.EventBody
						{
							Body = new UploadAlbumModel
							{
								MediaTopic = null,
								Album = dataMediaSubmit.RawMediaDatas.Select(x => new InstaAlbumUpload
								{
									ImageToUpload = x.MediaType == MediaSelectionType.Image
										? new InstaImageUpload
										{
											Uri = x.UrlToSend
										}
										: null,
									VideoToUpload = x.MediaType == MediaSelectionType.Video
										? new InstaVideoUpload
										{
											Video = new InstaVideo
											{
												Uri = x.UrlToSend
											},
											VideoThumbnail = new InstaImage
											{
												Uri = UploadToS3(
													_postAnalyser.Manipulation.VideoEditor.GenerateVideoThumbnail(
														_postAnalyser.Manager.DownloadMedia(x.UrlToSend)),
													$"user_self_videoThumb_{Guid.NewGuid()}").GetAwaiter().GetResult()
											}
										}
										: null
								}).ToArray(),
								Location = dataMediaSubmit.Location != null
									? new InstaLocationShort
									{
										Address = dataMediaSubmit.Location.Address,
										Name = dataMediaSubmit.Location.City
									}
									: null,
								MediaInfo = mediaInfo
							},
							BodyType = typeof(UploadAlbumModel)
						};
						break;
					default:
						return response;
				}

				if (options.DataObject?.Body == null)
					return response;

				AddEventToTimeline(options);
				response.IsSuccessful = true;
				return response;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				response.IsSuccessful = false;
				return response;
			}
		}

		public async Task<TimelineScheduleResponse<IEnumerable<IDirectMessageModel>>> ScheduleMessage(UserStoreDetails userStoreDetails, IEnumerable<IDirectMessageModel> messages)
		{
			var directMessages= messages as IDirectMessageModel[] ?? messages.ToArray();
			var response = new TimelineScheduleResponse<IEnumerable<IDirectMessageModel>>
			{
				RequestData = directMessages
			};

			if (userStoreDetails == null)
				return response;
			
			if (string.IsNullOrEmpty(userStoreDetails.AccountId) ||
			string.IsNullOrEmpty(userStoreDetails.InstagramAccountUser))
				return response;

			foreach (var message in directMessages)
			{
				foreach (var messageRecipient in message.Recipients)
				{
					var localCopy = message.CloneObject();
					localCopy.Recipients = new[] { messageRecipient };

					var availableTime = PickAGoodTime(userStoreDetails.AccountId, userStoreDetails.InstagramAccountUser, 
						ActionType.SendDirectMessage) ?? DateTime.UtcNow;

					var options = new EventActionOptions
					{
						ExecutionTime = availableTime.AddMinutes(.9),
						User = new UserStore
						{
							AccountId = userStoreDetails.AccountId,
							InstagramAccountUser = userStoreDetails.InstagramAccountUser,
							InstagramAccountUsername = userStoreDetails.InstagramAccountUsername
						}
					};

					switch (localCopy)
					{
						case ShareDirectMediaModel model:
							if (string.IsNullOrEmpty(model.MediaId))
							{
								response.NumberOfFails++;
								continue;;
							}

							options.DataObject = new EventActionOptions.EventBody
							{
								Body = model,
								BodyType = model.GetType()
							};
							options.ActionType = (int) ActionType.SendDirectMessageMedia;
							options.ActionDescription = ActionType.SendDirectMessageMedia.GetDescription();
							break;
						case SendDirectTextModel model:
							if (string.IsNullOrEmpty(model.TextMessage))
							{
								response.NumberOfFails++;
								continue;
							}

							options.DataObject = new EventActionOptions.EventBody
							{
								Body = model,
								BodyType = model.GetType()
							};
							options.ActionType = (int)ActionType.SendDirectMessageText;
							options.ActionDescription = ActionType.SendDirectMessageText.GetDescription();
							break;
						case SendDirectLinkModel model:
							if (string.IsNullOrEmpty(model.TextMessage) || string.IsNullOrEmpty(model.Link))
							{
								response.NumberOfFails++;
								continue;
							}

							if (!model.Link.IsValidUrl())
							{
								response.NumberOfFails++;
								continue;
							}

							options.DataObject = new EventActionOptions.EventBody
							{
								Body = model,
								BodyType = model.GetType()
							};
							options.ActionType = (int)ActionType.SendDirectMessageLink;
							options.ActionDescription = ActionType.SendDirectMessageLink.GetDescription();
							break;
						case SendDirectPhotoModel model:
							if (string.IsNullOrEmpty(model.Image.Uri))
							{
								response.NumberOfFails++;
								continue;
							}
							if (model.Image.Uri.IsBase64String())
							{
								model.Image.ImageBytes = Convert.FromBase64String(model.Image.Uri.Split(',')[1]);
								model.Image.Uri = null;
							}
							else
							{
								model.Image.ImageBytes = _postAnalyser.Manager.DownloadMedia(model.Image.Uri);
								model.Image.Uri = null;
							}

							options.DataObject = new EventActionOptions.EventBody
							{
								Body = model,
								BodyType = model.GetType()
							};
							options.ActionType = (int)ActionType.SendDirectMessagePhoto;
							options.ActionDescription = ActionType.SendDirectMessagePhoto.GetDescription();
							break;
						case SendDirectVideoModel model:
							if (string.IsNullOrEmpty(model.Video.Video.Uri))
							{
								response.NumberOfFails++;
								continue;
							}
							if (model.Video.Video.Uri.IsBase64String())
							{
								model.Video.Video.VideoBytes = Convert.FromBase64String(model.Video.Video.Uri.Split(',')[1]);
								model.Video.Video.Uri = null;
							}
							else
							{
								model.Video.Video.VideoBytes = _postAnalyser.Manager.DownloadMedia(model.Video.Video.Uri);
								model.Video.Video.Uri = null;
							}
							model.Video.VideoThumbnail = new InstaImage
							{
								ImageBytes = _postAnalyser.Manipulation.VideoEditor.GenerateVideoThumbnail(model.Video.Video.VideoBytes)
							};
							options.DataObject = new EventActionOptions.EventBody
							{
								Body = model,
								BodyType = model.GetType()
							};
							options.ActionType = (int)ActionType.SendDirectMessageVideo;
							options.ActionDescription = ActionType.SendDirectMessageVideo.GetDescription();
							break;
						case SendDirectProfileModel model:
							options.DataObject = new EventActionOptions.EventBody
							{
								Body = model,
								BodyType = model.GetType()
							};
							options.ActionType = (int)ActionType.SendDirectMessageProfile;
							options.ActionDescription = ActionType.SendDirectMessageProfile.GetDescription();
							break;
						default:
							response.NumberOfFails++;
							continue;
					}

					if (options.DataObject?.Body == null)
						continue;
					
					AddEventToTimeline(options);
					
					await _lookupCache.AddObjectToLookup(userStoreDetails.AccountId,
						userStoreDetails.InstagramAccountUser,
						new LookupModel(localCopy.Recipients.FirstOrDefault()) {
							Id = Guid.NewGuid().ToString(),
							LastModified = DateTime.UtcNow,
							LookupStatus = LookupStatus.Pending,
							ActionType = (ActionType) options.ActionType
						});
				}
			}

			//if all fails
			if (response.NumberOfFails >= directMessages.Sum(_ => _.Recipients.Count()))
			{
				return response;
			}

			response.IsSuccessful = true;
			return response;
		}

		public DateTime? PickAGoodTime(string accountId, string instagramAccountId,
			ActionType actionName = ActionType.All)
		{
			List<TimelineItem> timelineItems;

			switch (actionName)
			{
				case ActionType.None:
					timelineItems = GetScheduledEventsForUserForActionByDate(accountId, ActionType.CreateCommentMedia, DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward)?.ToList();
					if (timelineItems == null)
						break;
					timelineItems.AddRange(GetScheduledEventsForUserForActionByDate(accountId, ActionType.FollowUser, DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward)?.ToList());
					timelineItems.AddRange(GetScheduledEventsForUserForActionByDate(accountId, ActionType.WatchStory, DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward)?.ToList());
					timelineItems.AddRange(GetScheduledEventsForUserForActionByDate(accountId, ActionType.ReactStory, DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward)?.ToList());
					timelineItems.AddRange(GetScheduledEventsForUserForActionByDate(accountId, ActionType.LikePost, DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward)?.ToList());
					timelineItems.AddRange(GetScheduledEventsForUserForActionByDate(accountId, ActionType.LikeComment, DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward)?.ToList());
					timelineItems.AddRange(GetScheduledEventsForUserForActionByDate(accountId, ActionType.UnFollowUser, DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward)?.ToList());
					break;
				case ActionType.All:
					timelineItems = GetScheduledEventsForUserByDate(accountId, DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward)?.ToList();
					break;
				case ActionType.SendDirectMessage:
					timelineItems = GetScheduledEventsForUserForActionByDate(accountId, actionName, DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward)?.ToList();
					break;
				case ActionType.CreatePost:
					timelineItems = GetScheduledEventsForUserForActionByDate(accountId, actionName, DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward)?.ToList();
					break;
				default:
					timelineItems = GetScheduledEventsForUserForActionByDate(accountId, ActionType.CreateCommentMedia, DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward)?.ToList();
					if (timelineItems == null)
						break;
					timelineItems.AddRange(GetScheduledEventsForUserForActionByDate(accountId, ActionType.FollowUser, DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward)?.ToList());
					timelineItems.AddRange(GetScheduledEventsForUserForActionByDate(accountId, ActionType.WatchStory, DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward)?.ToList());
					timelineItems.AddRange(GetScheduledEventsForUserForActionByDate(accountId, ActionType.ReactStory, DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward)?.ToList());
					timelineItems.AddRange(GetScheduledEventsForUserForActionByDate(accountId, ActionType.LikePost, DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward)?.ToList());
					timelineItems.AddRange(GetScheduledEventsForUserForActionByDate(accountId, ActionType.LikeComment, DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward)?.ToList());
					timelineItems.AddRange(GetScheduledEventsForUserForActionByDate(accountId, ActionType.UnFollowUser, DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward)?.ToList());
					break;
			}

			if (timelineItems == null || !timelineItems.Any())
				return null;

			var datesPlanned = timelineItems.Select(_ => _.EnqueueTime);
			var dateTimes = datesPlanned as DateTime?[] ?? datesPlanned.ToArray();

			if (!dateTimes.Any()) return DateTime.UtcNow;
			{
				var current = DateTime.UtcNow;
				var difference = dateTimes.Where(_ => _ != null).Max(_ => _ - current);

				var position = dateTimes.ToList().FindIndex(n => n - current == difference);
				var datePick = dateTimes.ElementAt(position);

				return datePick;
			}
		}
	
		public string UpdateEvent(UpdateTimelineMediaItemRequest updateTimelineMediaItemRequest)
		{
			try 
			{ 
				var job = _taskService.GetEvent(updateTimelineMediaItemRequest.Id);
				if (job == null)
					return null;

				if (job.EventBody.BodyType == typeof(UploadPhotoModel))
				{
					var model = JsonConvert.DeserializeObject<UploadPhotoModel>(job.EventBody.Body.ToJsonString());
					model.MediaInfo = new MediaInfo
					{
						Caption = updateTimelineMediaItemRequest.Caption,
						Credit = updateTimelineMediaItemRequest.Credit,
						Hashtags = updateTimelineMediaItemRequest.Hashtags,
						MediaType = (InstaMediaType)updateTimelineMediaItemRequest.Type
					};
					model.Location = updateTimelineMediaItemRequest.Location;
					job.EventBody.Body = model;
				}
				else if (job.EventBody.BodyType == typeof(UploadVideoModel))
				{
					var model = JsonConvert.DeserializeObject<UploadVideoModel>(job.EventBody.Body.ToJsonString());
					model.MediaInfo = new MediaInfo
					{
						Caption = updateTimelineMediaItemRequest.Caption,
						Credit = updateTimelineMediaItemRequest.Credit,
						Hashtags = updateTimelineMediaItemRequest.Hashtags,
						MediaType = (InstaMediaType)updateTimelineMediaItemRequest.Type
					};
					model.Location = updateTimelineMediaItemRequest.Location;
					job.EventBody.Body = model;
				}
				else if (job.EventBody.BodyType == typeof(UploadAlbumModel))
				{
					var model = JsonConvert.DeserializeObject<UploadAlbumModel>(job.EventBody.Body.ToJsonString());
					model.MediaInfo = new MediaInfo
					{
						Caption = updateTimelineMediaItemRequest.Caption,
						Credit = updateTimelineMediaItemRequest.Credit,
						Hashtags = updateTimelineMediaItemRequest.Hashtags,
						MediaType = (InstaMediaType) updateTimelineMediaItemRequest.Type
					};

					foreach (var toDeletePosition in updateTimelineMediaItemRequest.Deleted)
					{
						model.Album = model.Album.RemoveAt(toDeletePosition);
					}

					model.Location = updateTimelineMediaItemRequest.Location;
					job.EventBody.Body = model;
				}

				if (!DeleteEvent(updateTimelineMediaItemRequest.Id)) return null;

				job.ExecuteTime = updateTimelineMediaItemRequest.Time;
				var res = AddEventToTimeline(new EventActionOptions
				{
					ActionType = (int) job.ActionType,
					ActionDescription = job.ActionDescription,
					DataObject = job.EventBody,
					ExecutionTime = job.ExecuteTime,
					User = job.User
				});

				return !string.IsNullOrEmpty(res) ? res : null;
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}
		public void ExecuteNow(string eventId)
		{
			_taskService.ExecuteNow(eventId);
		}

		#endregion

		#region Get Specified UserAction
		public IEnumerable<TimelineItem> GetScheduledEventsForUserForAction(ActionType actionType, string username, string instaId = null, int limit = 100)
		{
			return GetScheduledEventsForUser(username, instaId, limit: limit).Where(_ => _.ActionType == actionType);
		}
		#endregion

		#region GET BY USING DATES

		public IEnumerable<ResultBase<TimelineItem>> GetAllEventsForUserByAction(ActionType actionType, string userName,
			DateTime startDate, DateTime? endDate = null, string instaId = null, int limit = 1000, TimelineDateType timelineDateType = TimelineDateType.Backwards)
		{
			return GetAllEventsForUser(userName, startDate, endDate, instaId, limit, timelineDateType).Where(_ => _.Response.ActionType==actionType);
		}

		public IEnumerable<TimelineItem> GetScheduledEventsForUserByDate(string username, DateTime date, DateTime? endDate = null,
			string instaId = null, int limit = 100, TimelineDateType timelineDateType = TimelineDateType.Backwards)
		{
			switch (timelineDateType)
			{
				case TimelineDateType.Backwards:
					if (endDate == null)
					{
						endDate = date.AddDays(-1).AddTicks(1);
					}
					var eventsB = GetScheduledEventsForUser(username, instaId, limit: limit)
						.Where(_ => _.EnqueueTime <= date && _.EnqueueTime >= endDate);
					return eventsB;
				case TimelineDateType.Forward:
					if (endDate == null)
					{
						endDate = date.AddDays(1).AddTicks(-1);
					}
					var eventsF = GetScheduledEventsForUser(username, instaId, limit: limit)
						.Where(_ => _.EnqueueTime >= date && _.EnqueueTime <= endDate);
					return eventsF;
				default: throw new NotImplementedException();
			}
		}

		public IEnumerable<TimelineFinishedItem> GetFinishedEventsForUserByDate(string username, DateTime date, DateTime? endDate = null,
			string instaId = null, int limit = 100, TimelineDateType timelineDateType = TimelineDateType.Backwards)
		{
			switch (timelineDateType)
			{
				case TimelineDateType.Backwards:
					if (endDate == null)
					{
						endDate = date.AddDays(-1).AddTicks(1);
					}
					var eventsB = GetFinishedEventsForUser(username, instaId, limit)
						.Where(_ => _?.SuccessAt <= date && _?.SuccessAt >= endDate);
					return eventsB;
				case TimelineDateType.Forward:
					if (endDate == null)
					{
						endDate = date.AddDays(1).AddTicks(-1);
					}
					var eventsF = GetFinishedEventsForUser(username, instaId, limit);
						
					return eventsF.Where(_ => _?.SuccessAt >= date && _?.SuccessAt <= endDate);
				default: throw new NotImplementedException();
			}
		}

		public IEnumerable<TimelineInProgressItem> GetCurrentlyRunningEventsForUserByDate(string username, DateTime date, DateTime? endDate = null,
			int limit = 100, string instaid = null, TimelineDateType timelineDateType = TimelineDateType.Backwards)
		{
			switch (timelineDateType)
			{
				case TimelineDateType.Backwards:
					if (endDate == null)
					{
						endDate = date.AddDays(-1).AddTicks(1);
					}
					var eventsB = GetCurrentlyRunningEventsForUser(username, instaid, limit: limit)
						.Where(_ => _.StartedAt <= date && _.StartedAt >= endDate);
					return eventsB;
				case TimelineDateType.Forward:
					if (endDate == null)
					{
						endDate = date.AddDays(1).AddTicks(-1);
					}
					var eventsF = GetCurrentlyRunningEventsForUser(username, instaid, limit: limit)
						.Where(_ => _.StartedAt >= date && _.StartedAt <= endDate);
					return eventsF;
				default:throw new NotImplementedException();
			}
		}

		public IEnumerable<TimelineDeletedItem> GetDeletedEventsForUserByDate(string username, DateTime date, DateTime? endDate = null,
			string instaId = null, int limit = 100, TimelineDateType timelineDateType = TimelineDateType.Backwards)
		{
			switch (timelineDateType)
			{
				case TimelineDateType.Backwards:
					if (endDate == null)
					{
						endDate = date.AddDays(-1).AddTicks(1);
					}
					var eventsB = GetDeletedEventsForUser(username, instaId, limit: limit)
						.Where(_ => _.DeletedAt <= date && _.DeletedAt >= endDate);
					return eventsB;
				case TimelineDateType.Forward:
					if (endDate == null)
					{
						endDate = date.AddDays(1).AddTicks(-1);
					}
					var eventsF = GetDeletedEventsForUser(username, instaId, limit: limit)
						.Where(_ => _.DeletedAt >= date && _.DeletedAt <= endDate);
					return eventsF;
				default: throw new NotImplementedException();
			}
		}

		public IEnumerable<TimelineFailedItem> GetFailedEventsForUserByDate(string username, DateTime date, DateTime? endDate = null,
			string instaId = null, int limit = 100, TimelineDateType timelineDateType = TimelineDateType.Backwards)
		{
			switch (timelineDateType)
			{
				case TimelineDateType.Backwards:
					if (endDate == null)
					{
						endDate = date.AddDays(-1).AddTicks(1);
					}
					var eventsB = GetFailedEventsForUser(username, instaId, limit: limit)
						.Where(_ => _.FailedAt <= date && _.FailedAt >= endDate);
					return eventsB;
				case TimelineDateType.Forward:
					if (endDate == null)
					{
						endDate = date.AddDays(1).AddTicks(-1);
					}
					var eventsF = GetFailedEventsForUser(username, instaId, limit: limit)
						.Where(_ => _.FailedAt >= date && _.FailedAt <= endDate);
					return eventsF;
			}
			return null;
		}

		public IEnumerable<TimelineItem> GetScheduledEventsForUserForActionByDate(string username, 
			ActionType actionType, DateTime date, string instaId = null, DateTime? endDate = null, int limit = 30, 
			TimelineDateType timelineDateType = TimelineDateType.Backwards)
		{
			switch (timelineDateType)
			{
				case TimelineDateType.Backwards:
					if (endDate == null)
					{
						endDate = date.AddDays(-1).AddTicks(1);
					}
					var eventsB = GetScheduledEventsForUser(username, instaId, limit: limit)
						.Where(_ => (_.EnqueueTime <= date && _.EnqueueTime >= endDate) && _.ActionType == actionType);
					return eventsB;
				case TimelineDateType.Forward:
					if (endDate == null)
					{
						endDate = date.AddDays(1).AddTicks(-1);
					}
					var eventsF = GetScheduledEventsForUser(username, instaId, limit: limit)
						.Where(_ => (_.EnqueueTime >= date && _.EnqueueTime <= endDate) && _.ActionType == actionType);
					return eventsF;
			}
			return null;
		}

		public IEnumerable<ResultBase<TimelineItemShort>> ShortGetAllEventsForUser(string userName, DateTime startDate, 
			DateTime? endDate = null, string instaId = null, int limit = 1000, 
			TimelineDateType timelineDateType = TimelineDateType.Backwards)
		{
			var totalEvents = new List<ResultBase<TimelineItemShort>>();

			totalEvents.AddRange(GetScheduledEventsForUserByDate(userName, startDate, endDate, instaId, limit, 
				timelineDateType).Select(_ => new ResultBase<TimelineItemShort>
			{
				Response = new TimelineItemShort
				{
					ActionType = _.ActionType,
					ActionDescription = _.ActionDescription,
					EnqueueTime = _.EnqueueTime,
					ItemId = _.ItemId,
					StartTime = _.StartTime,
					State = _.State,
					Body = _.EventBody.ToJsonString(),
				},
				TimelineType = typeof(TimelineItemShort)
			}));

			return totalEvents;
		}

		public IEnumerable<TimelineItemShort> GetScheduledPosts(string username, string instagramId, int limit = 1000)
		{
			var res = GetScheduledEventsForUserForAction(ActionType.CreatePost, username, instagramId,
				limit).ToList();

			return res.Select(_ => new TimelineItemShort
			{
				ActionType = _.ActionType,
				ActionDescription = _.ActionDescription,
				EnqueueTime = _.EnqueueTime,
				ItemId = _.ItemId,
				StartTime = _.StartTime,
				State = _.State,
				Body = _.EventBody.ToJsonString(),
			}).Distinct();
		}
		
		public IEnumerable<ResultBase<TimelineItem>> GetAllEventsForUser(string userName, DateTime startDate, DateTime? endDate = null,
		string instaId = null, int limit = 1000, TimelineDateType timelineDateType = TimelineDateType.Backwards)
		{
			var totalEvents = new List<ResultBase<TimelineItem>>();
			totalEvents.AddRange(GetScheduledEventsForUserByDate(userName, startDate, endDate, instaId, limit, timelineDateType).Select(_ => new ResultBase<TimelineItem>
			{
				Response = new TimelineItem
				{
					ActionType = _.ActionType,
					ActionDescription = _.ActionDescription,
					User = _.User,
					EventBody = _.EventBody,
					EnqueueTime = _.EnqueueTime,
					ItemId = _.ItemId,
					StartTime = _.StartTime,
					State = _.State
				},
				TimelineType = typeof(TimelineItem)
			}));
			
			/*totalEvents.AddRange(GetFinishedEventsForUserByDate(userName, startDate, endDate, instaId, limit, timelineDateType).Select(_ => new ResultBase<TimelineItem>
			{
				Response = new TimelineItem
				{
					ActionName = _.ActionName,
					EnqueueTime = _.SuccededAt,
					ItemId = _.ItemId,
					State = _.State,
					Url = _.Url,
					User = _.User
				},
				Message = _.Results,
				TimelineType = typeof(TimelineFinishedItem)
			}));*/
			/*
			totalEvents.AddRange(GetCurrentlyRunningEventsForUserByDate(userName, startDate, endDate, limit, instaId, timelineDateType).Select(_ => new ResultBase<TimelineItem>
			{
				Response = new TimelineItem
				{
					ActionName = _.ActionName,
					EnqueueTime = _.StartedAt,
					ItemId = _.ItemId,
					State = _.State,
					Url = _.Url,
					User = _.User
				},
				TimelineType = typeof(TimelineInProgressItem)
			}));
			*/
			/*
			totalEvents.AddRange(GetDeletedEventsForUserByDate(userName, startDate, endDate, instaId, limit, timelineDateType).Select(_ => new ResultBase<TimelineItem>
			{
				Response = new TimelineItem
				{
					ActionName = _.ActionName,
					EnqueueTime = _.DeletedAt,
					ItemId = _.ItemId,
					State = _.State,
					Url = _.Url,
					User = _.User
				},
				TimelineType = typeof(TimelineDeletedItem)
			}));
			totalEvents.AddRange(GetFailedEventsForUserByDate(userName, startDate, endDate, instaId, limit, timelineDateType).Select(_ => new ResultBase<TimelineItem>
			{
				Response = new TimelineItem
				{
					ActionName = _.ActionName,
					EnqueueTime = _.FailedAt,
					ItemId = _.ItemId,
					State = _.State,
					Url = _.Url,
					User = _.User
				},
				Message = _.Error,
				TimelineType = typeof(TimelineFailedItem)
			}));
			*/
			return totalEvents;
		}

		#endregion

		#region GETTING EVENT DETAILS FOR THE USER
		public IEnumerable<TimelineItem> GetScheduledEventsForUser(string username, string instagramId = null, int limit = 30)
		{
			return _taskService.GetScheduledItemsForUser(username, instagramId, limit);
		}
		public IEnumerable<TimelineFinishedItem> GetFinishedEventsForUser(string username, string instagramId = null, int limit = 30)
		{
			return _taskService.GetFinishedItemsForUser(username, instagramId, limit);
		}
		public IEnumerable<TimelineInProgressItem> GetCurrentlyRunningEventsForUser(string username, string instagramId = null, int limit = 30)
		{
			return _taskService.GetCurrentlyRunningItemsForUser(username, instagramId, limit);
		}
		public IEnumerable<TimelineFailedItem> GetFailedEventsForUser(string username, string instagramId = null, int limit = 30)
		{
			return _taskService.GetFailedItemsForUser(username, instagramId, limit);
		}
		public IEnumerable<TimelineDeletedItem> GetDeletedEventsForUser(string username, string instagramId = null, int limit = 30)
		{
			return _taskService.GetDeletedItemsForUser(username, instagramId, limit);
		}

		#endregion

		#region GET BY SPECIFIED JOB ID FOR ANY USER (ADMIN) BUT CAN BE USED FOR USER LEVEL TOO (BECAREFUL)
		public TimelineItemDetail GetEventDetail(string eventId)
		{
			return _taskService.GetEvent(eventId);
		}
		#endregion

		#region ADMIN TIMELINE OPTIONS
		public TimelineStatistics GetTimelineStatistics()
		{
			return _taskService.GetStatistics();
		}
		public IEnumerable<TimelineInProgressItem> GetTotalCurrentlyRunningEvents(int from, int limit)
		{
			return _taskService.GetTotalCurrentlyRunningJobs(from, limit);
		}
		public IEnumerable<TimelineFinishedItem> GetTotalFinishedEvents(int from, int limit)
		{
			return _taskService.GetTotalFinishedJobs(from, limit);
		}
		public IEnumerable<TimelineDeletedItem> GetTotalDeletedEvents(int from, int limit)
		{
			return _taskService.GetTotalDeletedEvents(from, limit);
		}
		public IEnumerable<TimelineFailedItem> GetTotalFailedEvents(int from, int limit)
		{
			return _taskService.GetTotalFailedEvents(from, limit);
		}
		public IEnumerable<TimelineItem> GetTotalScheduledEvents(int from, int limit)
		{
			return _taskService.GetTotalScheduledEvents(from, limit);
		}
		public bool IsAnyEventsCurrentlyRunning()
		{
			return _taskService.IsAnyJobsCurrentlyRunning();
		}

		public bool DeleteEvent(string eventId)
		{
			return _taskService.DeleteEvent(eventId);
		}
		#endregion
	}
}
