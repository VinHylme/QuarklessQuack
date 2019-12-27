using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;
using QuarklessContexts.Models.ServicesModels.HeartbeatModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessRepositories.RedisRepository.HeartBeaterRedis;
using MoreLinq;
using QuarklessContexts.Models.ServicesModels.FetcherModels;
using QuarklessLogic.Handlers.EventHandlers;

namespace QuarklessLogic.ServicesLogic.HeartbeatLogic
{
	public class HeartbeatLogic : IHeartbeatLogic
	{
		private readonly IHeartbeatRepository _heartbeatRepository;
		private readonly IEventPublisher _eventPublisher;
		public HeartbeatLogic(IHeartbeatRepository heartbeatRepository, IEventPublisher eventPublisher)
		{
			_heartbeatRepository = heartbeatRepository;
			_eventPublisher = eventPublisher;
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
		public async Task RefreshMetaData(MetaDataType metaDataType, string topic, string userId = null)
		{
			try {

				#region Old
				//var datas = (await GetMetaData<object>(metaDataType,topic,userId)).ToList();
				//if (datas.Count <= 0) return;
				//var by = new By { ActionType = 101, User = "Refreshed" };
				//var datass = new List<__Meta__<object>>();
				//foreach(var data in datas.DistinctBy(_=>_.ObjectItem).TakeAny(20))
				//{
				//	if(data.SeenBy is null)
				//	{
				//		datass.Add(data);
				//	}
				//	else
				//	{
				//		if (!data.SeenBy.Any(sb => sb.User == by.User && sb.ActionType == by.ActionType))
				//		{
				//			datass.Add(data);
				//		}
				//	}
				
				//}
				//switch (metaDataType)
				//{
				//	case MetaDataType.FetchMediaByTopic:
				//	case MetaDataType.FetchMediaByTopicRecent:
				//	{
				//		//var mediasfe = await _mediaCorpusLogic.MediasCount(topic);
				//		//if (mediasfe <= 80000)
				//		//{
				//		//	var totalMedias = new Media();
				//		//	datass.ForEach(data => {
				//		//		var medias = JsonConvert.DeserializeObject<Media>(JsonConvert.SerializeObject(data.ObjectItem));
				//		//		totalMedias.Medias.AddRange(medias.Medias);
				//		//	});
				//		//	if(totalMedias.Medias.Count > 0)
				//		//		PopulateCaption(totalMedias,topic);
				//		//}
				//		break;
				//	}
				//	case MetaDataType.FetchUsersViaPostCommented:
				//	{
				//		//var commentsfe = await _commentsLogic.CommentsCount(topic);
				//		//if (commentsfe <= 80000)
				//		//{
				//		//	var totalComments = new List<UserResponse<InstaComment>>();
				//		//	datass.ForEach(data => {
				//		//		var comments = JsonConvert.DeserializeObject<List<UserResponse<InstaComment>>>(JsonConvert.SerializeObject(data.ObjectItem));
				//		//		totalComments.AddRange(comments);
				//		//	});
				//		//	if(totalComments.Count > 0)
				//		//		PopulateComments(totalComments, topic);
				//		//}
				//		break;
				//	}
				//}
				//if(metaDataType == MetaDataType.FetchMediaByTopic || metaDataType == MetaDataType.FetchUsersViaPostCommented)
				//{
				//	foreach (var t in datass)
				//	{
				//		t.SeenBy = new List<By>();
				//	}

				//	foreach (var datatran in datass)
				//	{
				//		datatran.SeenBy = new List<By> {new By {ActionType = 101, User = "Refreshed"}};
				//	}
				//}
				#endregion

				switch (metaDataType)
				{
					case MetaDataType.FetchMediaByTopic:
					case MetaDataType.FetchMediaByTopicRecent:
					case MetaDataType.FetchMediaByLikers:
					case MetaDataType.FetchMediaByCommenters:
						var mediasObject = await GetMetaData<Media>(metaDataType, topic, userId);
						if (mediasObject == null || !mediasObject.Any())
							break;

						_eventPublisher.Publish(new MetaDataMediaRefresh{
							MetaDataType = metaDataType, 
							Topic =  topic, 
							UserId = userId,
							Medias = mediasObject.SelectMany(_=>_.ObjectItem.Medias).ToList()
						});
						break;
					case MetaDataType.FetchCommentsViaPostsLiked:
					case MetaDataType.FetchCommentsViaPostCommented:
						var commentObject =
							await GetMetaData<List<UserResponse<InstaComment>>>(metaDataType, topic, userId);

						if (commentObject == null || !commentObject.Any())
							break;

						_eventPublisher.Publish(new MetaDataCommentRefresh
						{
							MetaDataType = metaDataType,
							Topic = topic,
							UserId = userId,
							Comments = commentObject.SelectMany(_=>_.ObjectItem).ToList()
						});
						break;
				}

				await _heartbeatRepository.RefreshMetaData(metaDataType, topic, userId);
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
		}
	}
}
