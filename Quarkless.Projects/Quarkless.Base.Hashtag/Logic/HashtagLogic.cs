using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramApiSharp;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using Quarkless.Base.Hashtag.Models;
using Quarkless.Base.Hashtag.Models.Interfaces;
using Quarkless.Base.InstagramClient.Models.Interfaces;
using Quarkless.Base.ReportHandler.Models.Interfaces;

namespace Quarkless.Base.Hashtag.Logic
{
	public class HashtagLogic : IHashtagLogic
	{
		private readonly IReportHandler _reportHandler;
		private readonly IApiClientContainer _client;
		private readonly IHashtagsRepository _hashtagsRepository;
		public HashtagLogic(IReportHandler reportHandler, IApiClientContainer clientContext,
			IHashtagsRepository hashtagsRepository)
		{
			_client = clientContext;
			_reportHandler = reportHandler;
			_reportHandler.SetupReportHandler(nameof(HashtagLogic));
			_hashtagsRepository = hashtagsRepository;
		}

		#region Instagram Functions
		public Task<IResult<bool>> FollowHashtagAsync(string tagName)
		{
			throw new NotImplementedException();
		}
		public Task<IResult<InstaHashtagSearch>> GetFollowingHashtagsInfoAsync(long userId)
		{
			throw new NotImplementedException();
		}
		public Task<IResult<InstaHashtag>> GetHashtagInfoAsync(string tagName)
		{
			throw new NotImplementedException();
		}
		public Task<IResult<InstaHashtagStory>> GetHashtagStoriesAsync(string tagName)
		{
			throw new NotImplementedException();
		}
		public Task<IResult<InstaSectionMedia>> GetRecentHashtagMediaListAsync(string tagName, PaginationParameters paginationParameters)
		{
			throw new NotImplementedException();
		}
		public Task<IResult<InstaHashtagSearch>> GetSuggestedHashtagsAsync()
		{
			throw new NotImplementedException();
		}
		public async Task<IResult<InstaSectionMedia>> GetTopHashtagMediaListAsync(string topic, int limit)
		{
			try
			{
				return await _client.Hashtag.GetTopHashtagMediaListAsync(topic, PaginationParameters.MaxPagesToLoad(limit));
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaHashtagSearch>> SearchHashtagAsync(string query, IEnumerable<long> excludeList = null)
		{
			try
			{
				return await _client.Hashtag.SearchHashtagAsync(query, excludeList);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaSectionMedia>> SearchRelatedHashtagAsync(string query, int limit = 1)
		{
			try
			{
				return await _client.Hashtag.GetHashtagsSectionsAsync(query, PaginationParameters.MaxPagesToLoad(limit));
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public Task<IResult<bool>> UnFollowHashtagAsync(string tagName)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region Hashtag Database Logic
		public async Task AddHashtagsToRepository(IEnumerable<HashtagsModel> hashtags)
			=> await _hashtagsRepository.AddHashtags(hashtags);
		public async Task<List<HashtagsModel>> GetHashtagsFromRepositoryByTopic(string topic, int limit = 1)
			=> await _hashtagsRepository.GetHashtagsByTopic(topic, limit);

		public async Task<IEnumerable<HashtagsModel>> GetHashtags(int topicHashCode, int limit = -1, bool skip = true)
		{
			return await _hashtagsRepository.GetHashtags(topicHashCode, limit, skip);
		}
		#endregion
	}
}
