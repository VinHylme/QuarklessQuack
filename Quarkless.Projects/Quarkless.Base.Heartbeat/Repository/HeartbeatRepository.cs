using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;
using Quarkless.Base.Heartbeat.Models;
using Quarkless.Base.Heartbeat.Models.Enums;
using Quarkless.Base.Heartbeat.Models.Interfaces;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models;
using Quarkless.Models.SearchResponse;
using Quarkless.Repository.RedisContext;
using Quarkless.Repository.RedisContext.Models;

namespace Quarkless.Base.Heartbeat.Repository
{
	public class HeartbeatRepository : IHeartbeatRepository
	{
		private readonly IRedisClient _redis;
		public HeartbeatRepository(IRedisClient redis)
		{
			_redis = redis;
		}

		public async Task<bool> MetaDataContains(MetaDataContainsRequest request)
		{
			var contains = false;
			await WithExceptionLogAsync(async () =>
			{
				var key = $"{request.MetaDataType.ToString()}:{request.ProfileCategoryTopicId}:{request.InstagramId}";
				contains = await _redis.Database(0)
					.SetMemberExists(key, RedisKeys.HashtagGrowKeys.MetaData, request.JsonData);
			});
			return contains;
		}
		public async Task AddMetaData<TInput>(MetaDataCommitRequest<TInput> request)
		{
			await WithExceptionLogAsync(async () => {
				var key = $"{request.MetaDataType.ToString()}:{request.ProfileCategoryTopicId}:{request.InstagramId}";
				await _redis.Database(0).SetAdd(key, RedisKeys.HashtagGrowKeys.MetaData,
					request.Data.ToJsonString(), TimeSpan.FromHours(4));
			});
		}
		public async Task DeleteMetaDataFromSet<TInput>(MetaDataCommitRequest<TInput> request)
		{
			await WithExceptionLogAsync(async () => {
				var key = $"{request.MetaDataType.ToString()}:{request.ProfileCategoryTopicId}:{request.InstagramId}";
				await _redis.Database(0).SetRemove(key, RedisKeys.HashtagGrowKeys.MetaData,
					request.Data.ToJsonString());
			});
		}

		public async Task RefreshMetaData(MetaDataFetchRequest request)
		{
			await WithExceptionLogAsync(async () => {

				switch (request.MetaDataType)
				{
					case MetaDataType.FetchMediaByTopic:
					case MetaDataType.FetchMediaByTopicRecent:
					case MetaDataType.FetchMediaByLikers:
					case MetaDataType.FetchMediaByCommenters:
						var mediasObject = await GetMetaData<Media>(request);
						if (mediasObject == null || !mediasObject.Any())
							break;

						var dataMedia = new MetaDataMediaRefresh
						{
							Uuid = Guid.NewGuid().ToString(),
							MetaDataType = request.MetaDataType,
							ProfileCategoryTopicId = request.ProfileCategoryTopicId,
							InstagramId = request.InstagramId,
							AccountId = request.AccountId,
							Medias = mediasObject.SelectMany(_ => _.ObjectItem.Medias).ToList()
						};

						await _redis.Database(0).SetAdd(MetaDataTempType.Medias.GetDescription(), 
							RedisKeys.HashtagGrowKeys.MetaDataTemp,
							JsonConvert.SerializeObject(dataMedia), TimeSpan.FromHours(4));

						break;
					case MetaDataType.FetchCommentsViaPostsLiked:
					case MetaDataType.FetchCommentsViaPostCommented:
						var commentObject =
							await GetMetaData<List<UserResponse<InstaComment>>>(request);

						if (commentObject == null || !commentObject.Any())
							break;
						var dataComment = new MetaDataCommentRefresh
						{
							Uuid = Guid.NewGuid().ToString(),
							MetaDataType = request.MetaDataType,
							ProfileCategoryTopicId = request.ProfileCategoryTopicId,
							InstagramId = request.InstagramId,
							AccountId = request.AccountId,
							Comments = commentObject.SelectMany(_ => _.ObjectItem).ToList()
						};

						await _redis.Database(0).SetAdd(MetaDataTempType.Comments.GetDescription(),
							RedisKeys.HashtagGrowKeys.MetaDataTemp,
							JsonConvert.SerializeObject(dataComment), TimeSpan.FromHours(4));
						break;
				}

				var key = $"{request.MetaDataType.ToString()}:{request.ProfileCategoryTopicId}:{request.InstagramId}";
				await _redis.Database(0).DeleteKey(key, RedisKeys.HashtagGrowKeys.MetaData);
			});
		}

		public async Task<List<Meta<TResult>>> GetMetaData<TResult>(MetaDataFetchRequest request)
		{
			var response = new List<Meta<TResult>>();
			await WithExceptionLogAsync(async () => {
				var key = $"{request.MetaDataType.ToString()}:{request.ProfileCategoryTopicId}:{request.InstagramId}";
				var results = await _redis.Database(0).GetMembers<Meta<TResult>>(key, RedisKeys.HashtagGrowKeys.MetaData);
				if(results!=null && results.Any())
					response.AddRange(results);
			});
			return response;
		}
		public async Task<List<TResult>> GetTempMetaData<TResult>(MetaDataTempType type)
		{
			var response = new List<TResult>();
			await WithExceptionLogAsync(async () =>
			{
				var results = await _redis.Database(0)
					.GetMembers<TResult>(type.GetDescription(), RedisKeys.HashtagGrowKeys.MetaDataTemp);
				if(results!=null && results.Any())
					response.AddRange(results);
			});
			return response;
		}
		public async Task DeleteMetaDataTemp(MetaDataTempType type)
		{
			await WithExceptionLogAsync(async () =>
			{
				await _redis.Database(0).DeleteKey(type.GetDescription(), RedisKeys.HashtagGrowKeys.MetaDataTemp);
			});
		}
		private Task WithExceptionLogAsync(Func<Task> actionAsync)
		{
			try
			{
				return actionAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"{ex.Message}");
				throw;
			}
		}
	}
}
