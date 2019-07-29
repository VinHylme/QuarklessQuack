using InstagramApiSharp;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessRepositories.RedisRepository.CorpusCache.HashtagCorpusCache;
using QuarklessRepositories.Repository.ServicesRepositories.HashtagsRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuarklessLogic.Logic.HashtagLogic
{
	public class HashtagLogic : IHashtagLogic
	{
		private IReportHandler _reportHandler { get; set; }
		private readonly IAPIClientContainer Client;
		private readonly IHashtagsRepository _hashtagsRepository;
		private readonly IHashtagCoprusCache _hashtagCoprusCache;
		public HashtagLogic(IReportHandler reportHandler, IAPIClientContainer clientContexter, 
			IHashtagsRepository hashtagsRepository, IHashtagCoprusCache hashtagCoprusCache)
		{
			Client = clientContexter;
			_reportHandler = reportHandler;
			_hashtagsRepository = hashtagsRepository;
			_hashtagCoprusCache = hashtagCoprusCache;
		}
		public Task<IResult<bool>> FollowHashtagAsync(string tagname)
		{
			throw new NotImplementedException();
		}

		public Task<IResult<InstaHashtagSearch>> GetFollowingHashtagsInfoAsync(long userId)
		{
			throw new NotImplementedException();
		}

		public Task<IResult<InstaHashtag>> GetHashtagInfoAsync(string tagname)
		{
			throw new NotImplementedException();
		}

		public Task<IResult<InstaHashtagStory>> GetHashtagStoriesAsync(string tagname)
		{
			throw new NotImplementedException();
		}

		public Task<IResult<InstaSectionMedia>> GetRecentHashtagMediaListAsync(string tagname, PaginationParameters paginationParameters)
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
				return await Client.Hashtag.GetTopHashtagMediaListAsync(topic, PaginationParameters.MaxPagesToLoad(limit));
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}

		public async Task<IResult<InstaHashtagSearch>> SearchHashtagAsync(string query, IEnumerable<long> excludeList = null)
		{
			try
			{
				return await Client.Hashtag.SearchHashtagAsync(query,excludeList);
			}
			catch(Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}

		public Task<IResult<bool>> UnFollowHashtagAsync(string tagname)
		{
			throw new NotImplementedException();
		}
		public async Task AddHashtagsToRepositoryAndCache(IEnumerable<HashtagsModel> hashtags)
		{
			await _hashtagsRepository.AddHashtags(hashtags);
			await _hashtagCoprusCache.AddHashtags(hashtags);
		}
		public async Task<IEnumerable<HashtagsModel>> GetHashtagsByTopicAndLanguage(string topic, string lang, string langmapped, int limit = 1)
		{
			try
			{
				var cacheRes = await _hashtagCoprusCache.GetHashtags(topic, lang, limit);
				if(cacheRes!=null && cacheRes.Count()>0)
					return cacheRes;
				else
				{
					return await _hashtagsRepository.GetHashtags(topic, lang, langmapped, limit);
				}
			}
			catch(Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
	}
}
