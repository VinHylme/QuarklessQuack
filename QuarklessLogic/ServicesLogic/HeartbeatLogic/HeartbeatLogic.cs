using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.Proxies;
using QuarklessContexts.Models.ServicesModels.Corpus;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessContexts.Models.ServicesModels.HeartbeatModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessLogic.Handlers.Util;
using QuarklessLogic.Logic.HashtagLogic;
using QuarklessLogic.ServicesLogic.CorpusLogic;
using QuarklessRepositories.RedisRepository.HeartBeaterRedis;
using MoreLinq;
namespace QuarklessLogic.ServicesLogic.HeartbeatLogic
{
	public class HeartbeatLogic : IHeartbeatLogic
	{
		private readonly IHeartbeatRepository _heartbeatRepository;
		private readonly ICommentCorpusLogic _commentsLogic;
		private readonly IMediaCorpusLogic _mediaCorpusLogic;
		private readonly IHashtagLogic _hashtagLogic;
		private readonly IUtilProviders _utilProviders;
		public HeartbeatLogic(IHeartbeatRepository heartbeatRepository, IUtilProviders utilProviders,
			ICommentCorpusLogic comment, IMediaCorpusLogic  media, IHashtagLogic hashtags)
		{
			_hashtagLogic = hashtags;
			_commentsLogic = comment;
			_mediaCorpusLogic = media;
			_heartbeatRepository = heartbeatRepository;
			_utilProviders = utilProviders;
		}
		public async Task AddMetaData<T>(MetaDataType metaDataType, string topic, __Meta__<T> data, string userId = null)
		{
			var contains = await _heartbeatRepository.MetaDataContains(metaDataType, topic, JsonConvert.SerializeObject(data), userId);
			if (!contains)
			{
				await _heartbeatRepository.AddMetaData(metaDataType, topic, data, userId);
			}
		}
		public async Task UpdateMetaData<T>(MetaDataType metaDataType, string topic, __Meta__<T> data, string userId = null)
		{
			//if typeof media
			//else if
			var dataToRemove = (await _heartbeatRepository.GetMetaData<T>(metaDataType,topic,userId));
			var dto = dataToRemove.Where(_ => JsonConvert.SerializeObject(_.ObjectItem) == JsonConvert.SerializeObject(data.ObjectItem)).FirstOrDefault();
			if (dto!=null) { 
				await _heartbeatRepository.DeleteMetaDataFromSet(metaDataType, topic, dto, userId);
				await _heartbeatRepository.AddMetaData(metaDataType, topic, data, userId);
			}
		}
		public async Task<__Meta__<Media>> GetMediaMetaData(MetaDataType metaDataType, string topic)
		{
			return await _heartbeatRepository.GetMediaMetaData(metaDataType,topic);
		}
		public async Task<IEnumerable<__Meta__<T>>> GetMetaData<T>(MetaDataType metaDataType, string topic, string userId = null)
		{
			return (await _heartbeatRepository.GetMetaData<T>(metaDataType,topic, userId)).DistinctBy(_ => _.ObjectItem);
		}
		public async Task<__Meta__<List<UserResponse<string>>>> GetUserFromLikers(MetaDataType metaDataType, string topic)
		{
			return await _heartbeatRepository.GetUserFromLikers(metaDataType, topic);
		}
		public async Task RefreshMetaData(MetaDataType metaDataType, string topic, string userId = null, ProxyModel proxy = null)
		{
			try {
				var datas = (await GetMetaData<object>(metaDataType,topic,userId)).ToList();
				if (datas == null || datas.Count() <= 0) return;
				By by = new By { ActionType = 101, User = "Refreshed" };
				List<__Meta__<object>> datass = new List<__Meta__<object>>();
				foreach(var data in datas.DistinctBy(_=>_.ObjectItem).TakeAny(20))
				{
					if(data.SeenBy is null)
					{
						datass.Add(data);
					}
					else
					{
						if (!data.SeenBy.Any(sb => sb.User == by.User && sb.ActionType == by.ActionType))
						{
							datass.Add(data);
						}
					}
				
				}
				if (metaDataType == MetaDataType.FetchMediaByTopic || metaDataType == MetaDataType.FetchMediaByTopicRecent)
				{
					//first check limit
					var mediasfe = await _mediaCorpusLogic.MediasCount(topic);
					if (mediasfe <= 15000) { 
						ConcurrentBag<MediaCorpus> mediaCorpus = new ConcurrentBag<MediaCorpus>();
						ConcurrentBag<HashtagsModel> hashtags = new ConcurrentBag<HashtagsModel>();
						datass.ForEach(data => {
							var medias = JsonConvert.DeserializeObject<Media>(JsonConvert.SerializeObject(data.ObjectItem));
							Parallel.ForEach(medias.Medias, media =>
							{
								if (!string.IsNullOrEmpty(media.Caption))
								{
									var detection = _utilProviders.TranslateService.DetectLanguageYandex(texts: media.Caption);
									if (detection != null && detection.Count() > 0)
									{
										if (!string.IsNullOrEmpty(media.Caption))
										{
											var tags = media.Caption.Split('#');
											var cap = tags.ElementAtOrDefault(0);
											mediaCorpus.Add(new MediaCorpus
											{
												TakenAt = media.TakenAt,
												OriginalCaption = media.Caption,
												Caption = cap,
												Language = detection.FirstOrDefault(),
												Location = media?.Location,
												MediaId = media.MediaId,
												Topic = topic,
												Username = media?.User?.Username,
												Usertags = media?.UserTags?.Select(s => s?.User?.UserName).ToList(),
												CommentsCount = media.CommentCount,
												LikesCount = media.LikesCount,
												ViewsCount = media.ViewCount
											});
											if (tags.Count() > 0)
											{
												hashtags.Add(new HashtagsModel
												{
													Hashtags = tags.Where((l, o) => o > 0 && l.Length <= 15).ToList(),
													Topic = topic,
													Language = detection.FirstOrDefault()
												});
											}
										}
									}
								}
							});
						});				
						if (mediaCorpus.Count > 0)
						{
							await _mediaCorpusLogic.AddMedias(mediaCorpus);
						}
						if (hashtags.Count > 0)
						{
							await _hashtagLogic.AddHashtagsToRepositoryAndCache(hashtags);
						}
					}
				}
				else if(metaDataType == MetaDataType.FetchUsersViaPostCommented)
				{
					var commentsfe = await _commentsLogic.CommentsCount(topic);
					if (commentsfe <= 15000) { 
						ConcurrentBag<CommentCorpus> commentCorpus = new ConcurrentBag<CommentCorpus>();
						ConcurrentBag<HashtagsModel> hashtagsComments = new ConcurrentBag<HashtagsModel>();
						datass.ForEach(data => {
							var comments = JsonConvert.DeserializeObject<List<UserResponse<InstaComment>>>(JsonConvert.SerializeObject(data.ObjectItem));
							Parallel.ForEach(comments, comment =>
							{
								if (!string.IsNullOrEmpty(comment.Object.Text))
								{
									var detection = _utilProviders.TranslateService.DetectLanguageYandex(texts: comment.Object.Text);
									if (detection != null && detection.Count() > 0)
									{
										if (comment.Object.Text.Count(s => s == '#') > 2)
										{
											hashtagsComments.Add(new HashtagsModel
											{
												Hashtags = comment.Object.Text.Split('#').Where(l => l.Length <= 15).ToList(),
												Language = detection.FirstOrDefault(),
												Topic = topic
											});
										}
										else
										{
											commentCorpus.Add(new CommentCorpus
											{
												Comment = comment.Object.Text,
												Created = comment.Object.CreatedAt,
												Language = detection.FirstOrDefault(),
												NumberOfLikes = comment.Object.LikesCount,
												NumberOfReplies = comment.Object.ChildCommentCount,
												Username = comment.Username,
												MediaId = comment.MediaId,
												Topic = topic
											});
										}
									}
								}
							});
						});					
						if (commentCorpus.Count > 0)
						{
							await _commentsLogic.AddComments(commentCorpus);
						}
						if (hashtagsComments.Count > 0)
						{
							await _hashtagLogic.AddHashtagsToRepositoryAndCache(hashtagsComments);
						}
					}
				}
				if(metaDataType == MetaDataType.FetchMediaByTopic || metaDataType == MetaDataType.FetchUsersViaPostCommented)
				{ 
					for(int i = 0 ; i < datass.Count; i++)
					{
						datass[i].SeenBy = new List<By>();

					}
					foreach (var datatran in datass)
					{
						datatran.SeenBy = new List<By>();
						datatran.SeenBy.Add(new By
						{
							ActionType = 101,
							User = "Refreshed"
						});
					}
				}
				//await _heartbeatRepository.RefreshMetaData(metaDataType,topic,userId);
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
		}
	}
}
