using InstagramApiSharp;
using InstagramApiSharp.API.Processors;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessRepositories.Repository.ServicesRepositories.HashtagsRepository;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QuarklessLogic.Logic.HashtagLogic
{
	public class HashtagLogic : IHashtagLogic
	{
		private IReportHandler _reportHandler { get; set; }
		private readonly IAPIClientContainer Client;
		private readonly IHashtagsRepository _hashtagsRepository;
		public HashtagLogic(IReportHandler reportHandler, IAPIClientContainer clientContexter, IHashtagsRepository hashtagsRepository)
		{
			Client = clientContexter;
			_reportHandler = reportHandler;
			_hashtagsRepository = hashtagsRepository;
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

		public Task<IEnumerable<HashtagsModel>> GetHashtagsByTopic(string topic,int limit = 1)
		{
			try
			{
				return _hashtagsRepository.GetHashtagsByTopic(topic,limit);			
			}
			catch(Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
	}
}
