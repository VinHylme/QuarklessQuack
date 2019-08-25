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
			var dto = dataToRemove.FirstOrDefault(_ => JsonConvert.SerializeObject(_.ObjectItem) == JsonConvert.SerializeObject(data.ObjectItem));
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

		public async void PopulateCaption(Media item, string topic)
		{
			var filtered = item.Medias.Select(o =>
			{
				if (string.IsNullOrEmpty(o.Caption) || string.IsNullOrWhiteSpace(o.Caption)) return null;
				var sepaStrings = o.Caption.ToLower().Split('#');
				var caption = sepaStrings.ElementAtOrDefault(0);
				if (string.IsNullOrEmpty(caption) || string.IsNullOrWhiteSpace(caption)) return null;

				if (caption.ContainsMentions()
				    || caption.ContainsHashtags()
				    || caption.ContainsPhoneNumber()
				    || caption.ContainsWebAddress()
				    || caption.Contains("check out")
				    || caption.Contains("website")
				    || caption.Contains("phone")
				    || caption.Contains("blog")) return null;
				var tags = sepaStrings.Skip(1);
				var tagsEnumerator = tags as string[] ?? tags.ToArray();
				if (!tagsEnumerator.Any()) return null;
				return new
				{
					Media = new MediaCorpus
					{
						TakenAt = o.TakenAt,
						OriginalCaption = o.Caption,
						Caption = caption,
						Language = null,
						Location = o?.Location,
						MediaId = o.MediaId,
						Topic = topic,
						Username = o?.User?.Username,
						Usertags = o?.UserTags?.Select(s => s?.User?.UserName).ToList(),
						CommentsCount = o.CommentCount,
						LikesCount = o.LikesCount,
						ViewsCount = o.ViewCount
					},
					Tags = new HashtagsModel
					{
						Hashtags = tagsEnumerator.Where((l, h) => h > 0 && l.Length <= 15).ToList(),
						Topic = topic,
						Language = null
					}
				};
			}).Where(otem=>otem!=null).ToArray();

			if (filtered.Any())
			{
				var words = filtered.Select(s => s.Media.Caption);
				var detections = _utilProviders.TranslateService.DetectLanguageYandex(words.ToArray()).ToList();
				for (var i = 0; i < detections.Count; i++)
				{
					filtered[i].Media.Language = detections[i];
					filtered[i].Tags.Language = detections[i];
				}
			}

			var mediaCorpus = filtered.Select(x => x.Media).ToList();
			var hashtags = filtered.Select(x => x.Tags).ToList();

			if (mediaCorpus.Count > 0)
			{
				await _mediaCorpusLogic.AddMedias(mediaCorpus);
				Console.WriteLine($"Added captions for topic: {topic}, Captions Captured: {mediaCorpus.Count}");
			}
			else
				Console.WriteLine($"Failed to add captions for topic: {topic}, captions Captured: {mediaCorpus.Count}");

			if (hashtags.Count <= 0) return;
			await _hashtagLogic.AddHashtagsToRepositoryAndCache(hashtags);
			Console.WriteLine($"Added hashtags of caption for topic:{topic}, Hashtags Captured: {hashtags.Count}");
		}

		public async void PopulateComments(List<UserResponse<InstaComment>> comments, string topic)
		{
			var filteredComments = comments.Select(o =>
			{
				if (string.IsNullOrEmpty(o.Object.Text)) return null;
				var tags = new List<HashtagsModel>();
				if (o.Object.Text.Count(s => s == '#') > 2)
					return null;
				if (o.Object.Text.ContainsMentions()
				    || o.Object.Text.ContainsHashtags()
				    || o.Object.Text.ContainsPhoneNumber()
				    || o.Object.Text.ContainsWebAddress()
				    || o.Object.Text.Contains("check out")
				    || o.Object.Text.Contains("website")
				    || o.Object.Text.Contains("phone")
				    || o.Object.Text.Contains("blog")) return null;
				return new CommentCorpus
				{
					Comment = o.Object.Text,
					Created = o.Object.CreatedAt,
					Language = null,
					NumberOfLikes = o.Object.LikesCount,
					NumberOfReplies = o.Object.ChildCommentCount,
					Username = o.Username,
					MediaId = o.MediaId,
					Topic = topic
				};
			}).Where(comment => comment != null).ToArray();
			var filteredTags = comments.Select(o =>
			{
				if (string.IsNullOrEmpty(o.Object.Text)) return null;
				var tags = new List<HashtagsModel>();
				if (o.Object.Text.Count(s => s == '#') > 2)
					return new HashtagsModel
					{
						Hashtags = o.Object.Text.Split('#').Where(l => l.Length <= 15).ToList(),
						Language = null,
						Topic = topic
					};

				return null;
			}).Where(tag => tag != null).ToArray();
			if (filteredComments.Any())
			{
				var words = filteredComments.Select(s => s.Comment);
				var detections = _utilProviders.TranslateService.DetectLanguageYandex(words.ToArray()).ToList();

				for (var i = 0; i < detections.Count; i++)
				{
					filteredComments[i].Language = detections[i];
				}

				var mostFrequentLang = string.Empty;
				if(detections.Count > 0)
					mostFrequentLang =
						detections.GroupBy(x => x).OrderBy(g => g.Count()).Last().Key;

				foreach (var t in filteredTags)
				{
					t.Language = mostFrequentLang;
				}
			}

			var commentCorpus = filteredComments.ToList();
			var hashtagsComments = filteredTags.ToList();

			if (commentCorpus.Count > 0)
			{
				await _commentsLogic.AddComments(commentCorpus);
				Console.WriteLine($"Added comments for topic: {topic}, comments Captured: {commentCorpus.Count}");
			}
			else
				Console.WriteLine($"Failed to add comments for topic: {topic}, comments Captured: {commentCorpus.Count}");

			if (hashtagsComments.Count <= 0) return;
			await _hashtagLogic.AddHashtagsToRepositoryAndCache(hashtagsComments);
			Console.WriteLine($"Added hashtags of comments for topic:{topic}, Hashtags Captured: {hashtagsComments.Count}");
		}
		public async Task RefreshMetaData(MetaDataType metaDataType, string topic, string userId = null, ProxyModel proxy = null)
		{
			try {
				var datas = (await GetMetaData<object>(metaDataType,topic,userId)).ToList();
				if (datas.Count <= 0) return;
				var by = new By { ActionType = 101, User = "Refreshed" };
				var datass = new List<__Meta__<object>>();
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
				switch (metaDataType)
				{
					case MetaDataType.FetchMediaByTopic:
					case MetaDataType.FetchMediaByTopicRecent:
					{
						//var mediasfe = await _mediaCorpusLogic.MediasCount(topic);
						//if (mediasfe <= 80000)
						//{
						//	var totalMedias = new Media();
						//	datass.ForEach(data => {
						//		var medias = JsonConvert.DeserializeObject<Media>(JsonConvert.SerializeObject(data.ObjectItem));
						//		totalMedias.Medias.AddRange(medias.Medias);
						//	});
						//	if(totalMedias.Medias.Count > 0)
						//		PopulateCaption(totalMedias,topic);
						//}
						break;
					}
					case MetaDataType.FetchUsersViaPostCommented:
					{
						//var commentsfe = await _commentsLogic.CommentsCount(topic);
						//if (commentsfe <= 80000)
						//{
						//	var totalComments = new List<UserResponse<InstaComment>>();
						//	datass.ForEach(data => {
						//		var comments = JsonConvert.DeserializeObject<List<UserResponse<InstaComment>>>(JsonConvert.SerializeObject(data.ObjectItem));
						//		totalComments.AddRange(comments);
						//	});
						//	if(totalComments.Count > 0)
						//		PopulateComments(totalComments, topic);
						//}
						break;
					}
				}
				if(metaDataType == MetaDataType.FetchMediaByTopic || metaDataType == MetaDataType.FetchUsersViaPostCommented)
				{
					foreach (var t in datass)
					{
						t.SeenBy = new List<By>();
					}

					foreach (var datatran in datass)
					{
						datatran.SeenBy = new List<By> {new By {ActionType = 101, User = "Refreshed"}};
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
