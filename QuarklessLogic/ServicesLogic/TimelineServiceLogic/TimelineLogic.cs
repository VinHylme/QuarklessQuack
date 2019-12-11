using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuarklessContexts.Enums;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.MediaModels;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.Handlers.RequestBuilder.RequestBuilder;
using QuarklessLogic.QueueLogic.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using Quarkless.Analyser;
using QuarklessContexts.Models.Library;
using QuarklessContexts.Models.LookupModels;
using QuarklessContexts.Models.MessagingModels;
using QuarklessLogic.Handlers.RequestBuilder.Constants;
using QuarklessLogic.Logic.StorageLogic;
using QuarklessLogic.QueueLogic.Jobs.JobOptions;
using QuarklessRepositories.RedisRepository.LookupCache;

namespace QuarklessLogic.ServicesLogic.TimelineServiceLogic.TimelineLogic
{
	public enum TimelineDateType
	{
		Backwards,
		Forward	
	}
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
		public string AddEventToTimeline(string actionName, RestModel restBody, DateTimeOffset executeTime)
		{
			restBody.RequestHeaders = _requestBuilder.DefaultHeaders(restBody.User.OInstagramAccountUser).ToList();

			var eventId = _taskService.ScheduleEvent(actionName, restBody, executeTime);
			return eventId;
		}
		public T ParseJsonObject<T>(string json) where T : class, new()
		{
			var jObject = JObject.Parse(json);
			return JsonConvert.DeserializeObject<T>(jObject.ToString());
		}
		private async Task<string> UploadToS3(byte[] media, string keyName)
		{
			using (var mediaStream = new MemoryStream(media))
			{
				return await _s3BucketLogic.UploadStreamFile(mediaStream, keyName);
			}
		}

		public async Task<TimelineScheduleResponse<RawMediaSubmit>> SchedulePostsByUser(UserStoreDetails userStoreDetails,
			RawMediaSubmit dataMediaSubmit)
		{
			var response = new TimelineScheduleResponse<RawMediaSubmit>
			{
				RequestData = dataMediaSubmit,
				IsSuccessful = false
			};
			if (string.IsNullOrEmpty(userStoreDetails.OAccountId) ||
				string.IsNullOrEmpty(userStoreDetails.OAccessToken) ||
				string.IsNullOrEmpty(userStoreDetails.OInstagramAccountUser))
				return response;

			try
			{
				var options = new LongRunningJobOptions
				{
					ActionName = "createPost",
					ExecutionTime = dataMediaSubmit.ExecuteTime,
					Rest = new RestModel {User = userStoreDetails, RequestType = RequestType.POST}
				};
				var mediaInfo = new MediaInfo
				{
					Caption = dataMediaSubmit.Caption,
					Hashtags = dataMediaSubmit.Hashtags
				};

				foreach (var media in dataMediaSubmit.RawMediaDatas)
				{
					if (string.IsNullOrEmpty(media.Media64BaseData))
						continue;

					media.UrlToSend = !media.Media64BaseData.IsBase64String()
						? media.Media64BaseData
						: await UploadToS3(Convert.FromBase64String(media.Media64BaseData.Split(',')[1]),
							$"media_self_user{Guid.NewGuid()}");

//					media.MediaByteArray = !media.Media64BaseData.IsBase64String() 
//						? media.Media64BaseData.DownloadMedia() 
//						: Convert.FromBase64String(media.Media64BaseData.Split(',')[1]);
				}

				switch (dataMediaSubmit.OptionSelected)
				{
					case MediaSelectionType.Image:
						options.ActionName += $"_{MediaSelectionType.Image.ToString()}_userPosted";
						options.Rest.BaseUrl = _urlReader.UploadPhoto;
						options.Rest.JsonBody = JsonConvert.SerializeObject(new UploadPhotoModel
						{
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
						});
						break;
					case MediaSelectionType.Video:
						options.ActionName += $"_{MediaSelectionType.Video.ToString()}_userPosted";
						options.Rest.BaseUrl = _urlReader.UploadVideo;
						var urlToSend = dataMediaSubmit.RawMediaDatas
							.FirstOrDefault()?.UrlToSend;
						if (urlToSend != null)
							options.Rest.JsonBody = JsonConvert.SerializeObject(new UploadVideoModel
							{
								MediaInfo = mediaInfo,
								Video = new InstaVideoUpload
								{
									Video = new InstaVideo
									{
										Uri = urlToSend
									},
									VideoThumbnail = new InstaImage
									{
										Uri = await UploadToS3(_postAnalyser.Manipulation.VideoEditor.GenerateVideoThumbnail(_postAnalyser.Manager.DownloadMedia(urlToSend)), $"user_self_videoThumb_{Guid.NewGuid()}")
									}
								},
								Location = dataMediaSubmit.Location != null
									? new InstaLocationShort
									{
										Address = dataMediaSubmit.Location.Address,
										Name = dataMediaSubmit.Location.City
									}
									: null
							});
						break;
					case MediaSelectionType.Carousel:
						options.ActionName += $"_{MediaSelectionType.Carousel.ToString()}_userPosted";
						options.Rest.BaseUrl = _urlReader.UploadCarousel;
						options.Rest.JsonBody = JsonConvert.SerializeObject(new UploadAlbumModel
						{
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
											Uri = UploadToS3(_postAnalyser.Manipulation.VideoEditor.GenerateVideoThumbnail(_postAnalyser.Manager.DownloadMedia(x.UrlToSend)),
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
						});
						break;
					default:
						return response;
				}

				if (string.IsNullOrEmpty(options.Rest.JsonBody))
					return response;

				AddEventToTimeline(options.ActionName, options.Rest, options.ExecutionTime);
				response.IsSuccessful = true;
				return response;
			}
			catch (Exception ee)
			{
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
			
			if (string.IsNullOrEmpty(userStoreDetails.OAccountId) ||
			string.IsNullOrEmpty(userStoreDetails.OAccessToken) ||
			string.IsNullOrEmpty(userStoreDetails.OInstagramAccountUser))
				return response;

			foreach (var message in directMessages)
			{
				foreach (var messageRecipient in message.Recipients)
				{
					var localCopy = message.CloneObject();
					localCopy.Recipients = new[] { messageRecipient };
					var options = new LongRunningJobOptions
					{
						ExecutionTime = PickAGoodTime(userStoreDetails.OAccountId, userStoreDetails.OInstagramAccountUser, ActionType.SendDirectMessage).AddMinutes(.9),
						Rest = new RestModel
						{
							User = userStoreDetails,
							RequestType = RequestType.POST
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

							options.Rest.JsonBody = model.ToJsonString();
							options.Rest.BaseUrl = model.ThreadIds.Any() ? _urlReader.SendDirectMessageMediaWithThreads 
								: _urlReader.SendDirectMessageMedia;

							options.ActionName = ActionType.SendDirectMessageMedia.GetDescription();
							break;
						case SendDirectTextModel model:
							if (string.IsNullOrEmpty(model.TextMessage))
							{
								response.NumberOfFails++;
								continue;
							}

							options.Rest.JsonBody = model.ToJsonString();
							options.Rest.BaseUrl = _urlReader.SendDirectMessageText;
							options.ActionName = ActionType.SendDirectMessageText.GetDescription();
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

							options.Rest.JsonBody = model.ToJsonString();
							options.Rest.BaseUrl = _urlReader.SendDirectMessageLink;
							options.ActionName = ActionType.SendDirectMessageLink.GetDescription();
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

							options.Rest.JsonBody = model.ToJsonString();
							options.Rest.BaseUrl = _urlReader.SendDirectMessagePhoto;
							options.ActionName = ActionType.SendDirectMessagePhoto.GetDescription();
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
							options.ActionName = ActionType.SendDirectMessageVideo.GetDescription();
							options.Rest.JsonBody = model.ToJsonString();
							options.Rest.BaseUrl = _urlReader.SendDirectMessageVideo;
							break;
						case SendDirectProfileModel model:

							options.Rest.JsonBody = model.ToJsonString();
							options.ActionName = ActionType.SendDirectMessageProfile.GetDescription();
							options.Rest.BaseUrl = _urlReader.SendDirectMessageProfile;
							break;
						default:
							response.NumberOfFails++;
							continue;
					}

					if(string.IsNullOrEmpty(options.Rest.BaseUrl))
						continue;
					
					AddEventToTimeline(ActionType.SendDirectMessage.GetDescription()+"_"+"none", options.Rest, options.ExecutionTime);
					await _lookupCache.AddObjectToLookup(userStoreDetails.OAccountId, userStoreDetails.OInstagramAccountUser, localCopy.Recipients.FirstOrDefault(),
						new LookupModel {
							Id = Guid.NewGuid().ToString(),
							LastModified = DateTime.UtcNow,
							LookupStatus = LookupStatus.Pending,
							ActionType = options.ActionName.GetValueFromDescription<ActionType>()
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

		public DateTime PickAGoodTime(string accountId, string instagramAccountId, ActionType? actionName = null)
		{
			List<TimelineItem> sft;
				switch (actionName)
				{
					case null:
						sft = GetScheduledEventsForUserByDate(accountId, DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward).ToList();
						break;
					case ActionType.SendDirectMessage:
						sft = GetScheduledEventsForUserForActionByDate(accountId, actionName.Value.GetDescription(), DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward).ToList();
						break;
					case ActionType.CreatePost:
						sft = GetScheduledEventsForUserForActionByDate(accountId, actionName.Value.GetDescription(), DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward).ToList();
						break;
					default:
						sft = GetScheduledEventsForUserForActionByDate(accountId, ActionType.CreateCommentMedia.GetDescription(), DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward).ToList();
						sft.AddRange(GetScheduledEventsForUserForActionByDate(accountId, ActionType.FollowUser.GetDescription(), DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward).ToList());
						sft.AddRange(GetScheduledEventsForUserForActionByDate(accountId, ActionType.LikePost.GetDescription(), DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward).ToList());
						sft.AddRange(GetScheduledEventsForUserForActionByDate(accountId, ActionType.LikeComment.GetDescription(), DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward).ToList());
						sft.AddRange(GetScheduledEventsForUserForActionByDate(accountId, ActionType.UnFollowUser.GetDescription(), DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward).ToList());
						break;
				}
				var datesPlanned = sft.Select(_ => _.EnqueueTime);
				var dateTimes = datesPlanned as DateTime?[] ?? datesPlanned.ToArray();
				if (!dateTimes.Any()) return DateTime.UtcNow;
				{
					var current = DateTime.UtcNow;
					var difference = dateTimes.Where(_ => _ != null).Max(_ => _ - current);
					var position = dateTimes.ToList().FindIndex(n => n - current == difference);
					return dateTimes.ElementAt(position).Value;
				}
		}
		public string UpdateEvent(UpdateTimelineItemRequest updateTimelineItemRequest)
		{
			try { 
				var job = _taskService.GetEvent(updateTimelineItemRequest.EventId);
				job.ExecuteTime = updateTimelineItemRequest.Time;
			
				var object_ = JsonConvert.DeserializeObject(job.Rest.JsonBody, 
					updateTimelineItemRequest.MediaType == 1 ? typeof(UploadPhotoModel) 
					: updateTimelineItemRequest.MediaType == 2 ? typeof(UploadVideoModel) 
					: updateTimelineItemRequest.MediaType == 3 ? typeof(UploadAlbumModel) : typeof(object)
					);

				object_.GetProp("MediaInfo").SetValue(object_, new MediaInfo
				{
					Caption = updateTimelineItemRequest.Caption,
					Credit = updateTimelineItemRequest.Credit,
					Hashtags = updateTimelineItemRequest.Hashtags
				});
				object_.GetProp("Location").SetValue(object_, updateTimelineItemRequest.Location);

				job.Rest.JsonBody = JsonConvert.SerializeObject(object_);

				if (!DeleteEvent(updateTimelineItemRequest.EventId)) return null;
				var res = AddEventToTimeline(job.ActionName, job.Rest, job.ExecuteTime);
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
		public IEnumerable<TimelineItem> GetScheduledEventsForUserForAction(string actionName, string username, string instaId = null, int limit = 100)
		{
			return GetScheduledEventsForUser(username, instaId, limit: limit).Where(_ => _.ActionName.Split('_')[0].ToLower().Equals(actionName.ToLower()));
		}
		#endregion
		#region GET BY USING DATES
		public IEnumerable<ResultBase<TimelineItem>> GetAllEventsForUserByAction(string actionName, string userName,
			DateTime startDate, DateTime? endDate = null, string instaId = null, int limit = 1000, TimelineDateType timelineDateType = TimelineDateType.Backwards)
		{
			return GetAllEventsForUser(userName, startDate, endDate, instaId, limit, timelineDateType).Where(_ => _?.Response?.ActionName?.Split('_')?[0]?.ToLower() == (actionName.ToLower()));
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
			}
			return null;
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
					var eventsB = GetFinishedEventsForUser(username, instaId, limit: limit)
						.Where(_ => _?.SuccededAt <= date && _?.SuccededAt >= endDate);
					return eventsB;
				case TimelineDateType.Forward:
					if (endDate == null)
					{
						endDate = date.AddDays(1).AddTicks(-1);
					}
					var eventsF = GetFinishedEventsForUser(username, instaId, limit: limit);
						
					return eventsF.Where(_ => _?.SuccededAt >= date && _?.SuccededAt <= endDate);
			}
			return null;
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
			}
			return null;
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
			}
			return null;
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
		public IEnumerable<TimelineItem> GetScheduledEventsForUserForActionByDate(string username, string actionName, DateTime date,
			string instaId = null, DateTime? endDate = null, int limit = 30, TimelineDateType timelineDateType = TimelineDateType.Backwards)
		{
			switch (timelineDateType)
			{
				case TimelineDateType.Backwards:
					if (endDate == null)
					{
						endDate = date.AddDays(-1).AddTicks(1);
					}
					var eventsB = GetScheduledEventsForUser(username, instaId, limit: limit)
						.Where(_ => (_.EnqueueTime <= date && _.EnqueueTime >= endDate) && _?.ActionName?.Split('_')?[0]?.ToLower() == (actionName.ToLower()));
					return eventsB;
				case TimelineDateType.Forward:
					if (endDate == null)
					{
						endDate = date.AddDays(1).AddTicks(-1);
					}
					var eventsF = GetScheduledEventsForUser(username, instaId, limit: limit)
						.Where(_ => (_.EnqueueTime >= date && _.EnqueueTime <= endDate) && _?.ActionName?.Split('_')?[0]?.ToLower() == (actionName.ToLower()));
					return eventsF;
			}
			return null;
		}
		public IEnumerable<ResultBase<TimelineItemShort>> ShortGetAllEventsForUser(string userName, DateTime startDate, DateTime? endDate = null,
		
		string instaId = null, int limit = 1000, TimelineDateType timelineDateType = TimelineDateType.Backwards)
		{
			List<ResultBase<TimelineItemShort>> totalEvents = new List<ResultBase<TimelineItemShort>>();
			totalEvents.AddRange(GetScheduledEventsForUserByDate(userName, startDate, endDate, instaId, limit, timelineDateType).Select(_ => new ResultBase<TimelineItemShort>
			{
				Response = new TimelineItemShort
				{
					ActionName = _.ActionName,
					EnqueueTime = _.EnqueueTime,
					ItemId = _.ItemId,
					StartTime = _.StartTime,
					State = _.State,
					Body = _.Rest.JsonBody,
					TargetId = Regex.Match(_.Url.Split('/').Last(),@"\d+").Value
				},
				TimelineType = typeof(TimelineItemShort)
			}));
			return totalEvents;
		}
		public IEnumerable<TimelineItemShort> GetScheduledPosts(string username, string instagramId, int limit = 1000)
		{
			var res = GetScheduledEventsForUserForAction(ActionType.CreatePost.GetDescription(), username, instagramId, limit).ToList();

			return res.Select(s => new TimelineItemShort
			{
				ActionName = s.ActionName,
				StartTime = s.StartTime,
				State = s.State,
				Body = s.Rest.JsonBody,
				EnqueueTime = s.EnqueueTime,
				ItemId = s.ItemId,
				TargetId = Regex.Match(s.Url.Split('/').Last(), @"\d+").Value
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
					ActionName = _.ActionName,
					EnqueueTime = _.EnqueueTime,
					ItemId = _.ItemId,
					StartTime = _.StartTime,
					State = _.State,
					Url = _.Url,
					User = _.User,
					Rest = _.Rest
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
