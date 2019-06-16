using Newtonsoft.Json;
using Quarkless.Queue.Jobs.Interfaces;
using Quarkless.Services.CommentsServices;
using Quarkless.Services.ContentBuilder;
using Quarkless.Services.Extensions;
using Quarkless.Services.PostServices;
using Quarkless.Services.RequestBuilder;
using Quarkless.Services.RequestBuilder.Consts;
using QuarklessContexts.Models.Requests;
using QuarklessLogic.Handlers.ClientProvider;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quarkless.Services
{
	public class AgentServicer : IAgentServicer
	{
		private readonly IPostServices _postServices;
		private readonly ISearchServices _searchServices;
		private readonly ICommentsServices _commentsServices;
		private readonly IAPIClientContext _aPIClientContext;
		private readonly ITaskService _taskService;
		private readonly IRequestBuilder _requestBuilder;
		private APIClientContainer _clientContexter;
		public string GetName => _clientContexter.GetContext.InstagramAccount.Username;

		public AgentServicer(IPostServices postServices,ISearchServices searchServices,
			ICommentsServices commentsServices, IAPIClientContext aPIClientContext,
			ITaskService taskService,IRequestBuilder requestBuilder)
		{
			_postServices = postServices;
			_searchServices = searchServices;
			_commentsServices = commentsServices;
			_aPIClientContext = aPIClientContext;
			_taskService = taskService;
			_requestBuilder = requestBuilder;
		}

		public IAgentServicer Create(string userId, string instaId)
		{
			_clientContexter = new APIClientContainer(_aPIClientContext, userId, instaId);
			return this;
		}

		private async Task<bool> BeginFetchingMediaWork(string tag, int limit)
		{
			return await _postServices.FetchMedias(tag, limit);
		}
		public class Relevancy
		{
			public string SearchResult { get; set; }
			public int Score { get; set; }
		}

		public async Task<bool> GatherComments()
		{
			var userSetts = _clientContexter.GetContext.Profile;
			var res= await _commentsServices.FetchComments(userSetts.TopicList.First());
			if(res)
				return true;
			return false;
		}

		public async Task<bool> FetchMedia()
		{
			try
			{
				var userSetts = _clientContexter.GetContext.Profile;
				string prevSearch = string.Empty;
				int position = 0;
				foreach (var topic in userSetts.TopicList)
				{
					var searchqueryResults = (await _searchServices.SearchTag(topic)).ToList();

					if (searchqueryResults.FirstOrDefault() == null) { return false; }

					Relevancy[] relevanSearch = new Relevancy[searchqueryResults.Count];

					for (int x = 0; x < searchqueryResults.Count; x++)
					{
						relevanSearch[x] = new Relevancy
						{
							Score = searchqueryResults[x].Similarity(topic),
							SearchResult = searchqueryResults[x]
						};
					}

					var picked = relevanSearch.OrderBy(_ => _.Score).ToList();
					if (prevSearch.Equals(picked[position]) && picked[position + 1] != null)
					{
						position++;
					}

					prevSearch = picked[position].SearchResult;
					var res = await BeginFetchingMediaWork(picked[position].SearchResult, 4);
					if (res)
					{
						await Task.Delay(1000);
						continue;
					}
					break;
				}
				return true;
			}
			catch (Exception ee)
			{
				return false;
			}
		}

		public async Task<bool> Start(string user, string instauser, string accessToken)
		{
			try { 
				Create(user,instauser);
				
				//_clientContexter.Initialize(user,instauser);
				//await GatherComments();
				return true;
			}
			catch(Exception ee)
			{
				return false;
			}
		}

	}
}
