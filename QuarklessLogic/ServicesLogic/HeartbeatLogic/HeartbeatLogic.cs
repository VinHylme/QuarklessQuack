using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContentSearcher;
using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.ServicesModels.Corpus;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessContexts.Models.ServicesModels.HeartbeatModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessLogic.Handlers.Util;
using QuarklessRepositories.RedisRepository.HeartBeaterRedis;
using QuarklessRepositories.Repository.CorpusRepositories.Comments;
using QuarklessRepositories.Repository.CorpusRepositories.Medias;
using QuarklessRepositories.Repository.ServicesRepositories.HashtagsRepository;

namespace QuarklessLogic.ServicesLogic.HeartbeatLogic
{
	public class HeartbeatLogic : IHeartbeatLogic
	{
		private readonly IHeartbeatRepository _heartbeatRepository;
		private readonly ICommentCorpusRepository _commentsRepository;
		private readonly IMediaCorpusRepository _mediaRepository;
		private readonly IHashtagsRepository _hashtagsRepository;
		private readonly IUtilProviders _utilProviders;
		public HeartbeatLogic(IHeartbeatRepository heartbeatRepository, IUtilProviders utilProviders,
			ICommentCorpusRepository comment, IMediaCorpusRepository media, IHashtagsRepository hashtagsRepository)
		{
			_hashtagsRepository = hashtagsRepository;
			_commentsRepository = comment;
			_mediaRepository = media;
			_heartbeatRepository = heartbeatRepository;
			_utilProviders = utilProviders;
		}
		public async Task AddMetaData<T>(MetaDataType metaDataType, string topic, __Meta__<T> data, string userId = null)
		{
			await _heartbeatRepository.AddMetaData(metaDataType,topic,data,userId);
		}
		public async Task UpdateMetaData<T>(MetaDataType metaDataType, string topic, __Meta__<T> data, string userId = null)
		{
			//if typeof media
			//else if
			var dataToRemove = (await _heartbeatRepository.GetMetaData<T>(metaDataType,topic,userId));
			var dto = dataToRemove.Where(_ => JsonConvert.SerializeObject(_.Value.ObjectItem) == JsonConvert.SerializeObject(data.ObjectItem)).FirstOrDefault();
			if (dto.HasValue) { 
				await _heartbeatRepository.DeleteMetaDataFromSet(metaDataType, topic, dto.Value, userId);
				await _heartbeatRepository.AddMetaData(metaDataType, topic, data, userId);
			}
		}
		public async Task<__Meta__<Media>?> GetMediaMetaData(MetaDataType metaDataType, string topic)
		{
			return await _heartbeatRepository.GetMediaMetaData(metaDataType,topic);
		}
		public async Task<IEnumerable<__Meta__<T>?>> GetMetaData<T>(MetaDataType metaDataType, string topic, string userId = null)
		{
			return await _heartbeatRepository.GetMetaData<T>(metaDataType,topic, userId);
		}
		public async Task<__Meta__<List<UserResponse<string>>>?> GetUserFromLikers(MetaDataType metaDataType, string topic)
		{
			return await _heartbeatRepository.GetUserFromLikers(metaDataType, topic);
		}
		public async Task RefreshMetaData(MetaDataType metaDataType, string topic, string userId = null)
		{
			try {
				var datas = (await GetMetaData<object>(metaDataType,topic,userId)).ToList();
				if(metaDataType == MetaDataType.FetchMediaByTopic)
				{
					//first check limit
					var mediasfe = await _mediaRepository.GetMediasCount(topic);
					if (mediasfe <= 5000) { 
						ConcurrentBag<MediaCorpus> mediaCorpus = new ConcurrentBag<MediaCorpus>();
						ConcurrentBag<HashtagsModel> hashtags = new ConcurrentBag<HashtagsModel>();
						datas.ForEach(data => {
							var medias = JsonConvert.DeserializeObject<Media>(JsonConvert.SerializeObject(data.Value.ObjectItem));
							Parallel.ForEach(medias.Medias.TakeAny(10), media =>
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
							_mediaRepository.AddMedias(mediaCorpus).GetAwaiter().GetResult();
						}
						if (hashtags.Count > 0)
					{
						_hashtagsRepository.AddHashtags(hashtags).GetAwaiter().GetResult();
					}
					}
				}
				else if(metaDataType == MetaDataType.FetchUsersViaPostCommented)
				{
					var commentsfe = await _commentsRepository.GetCommentsCount(topic);
					if (commentsfe <= 5000) { 
						ConcurrentBag<CommentCorpus> commentCorpus = new ConcurrentBag<CommentCorpus>();
						ConcurrentBag<HashtagsModel> hashtagsComments = new ConcurrentBag<HashtagsModel>();
						datas.ForEach(data => {
							var comments = JsonConvert.DeserializeObject<List<UserResponse<InstaComment>>>(JsonConvert.SerializeObject(data.Value.ObjectItem));
							Parallel.ForEach(comments.TakeAny(20), comment =>
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
							_commentsRepository.AddComments(commentCorpus);
						}
						if (hashtagsComments.Count > 0)
					{
						_hashtagsRepository.AddHashtags(hashtagsComments).GetAwaiter().GetResult();
					}
					}
				}
				await _heartbeatRepository.RefreshMetaData(metaDataType,topic,userId);
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
		}
	}
}
