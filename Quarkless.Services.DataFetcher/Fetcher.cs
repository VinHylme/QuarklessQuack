using System;
using System.Linq;
using System.Threading.Tasks;
using QuarklessLogic.ServicesLogic;
using QuarklessContexts.Extensions;
using System.Collections.Concurrent;
using Quarkless.Interfacing.Objects;
using System.Text.RegularExpressions;
using InstagramApiSharp;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using MoreLinq;
using QuarklessLogic.Logic.HashtagLogic;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Logic.InstagramAccountLogic;
using QuarklessLogic.Logic.ResponseLogic;
using QuarklessLogic.ServicesLogic.CorpusLogic;

namespace Quarkless.Services.DataFetcher
{
	public class Fetcher : IFetcher
	{
		#region Init
		private ConcurrentQueue<ShortInstagramAccountModel> _workers;
		private readonly ITopicServicesLogic _topicServicesLogic;
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private readonly IAPIClientContext _clientContext;
		private readonly IResponseResolver _responseResolver;
		private readonly IMediaCorpusLogic _mediaCorpusLogic;
		private readonly ICommentCorpusLogic _commentCorpusLogic;
		private readonly IHashtagLogic _hashtagCorpusLogic;
		public Fetcher(ITopicServicesLogic topicServicesLogic, IInstagramAccountLogic instagramAccountLogic, 
			IAPIClientContext clientContext, IResponseResolver responseResolver,
			IMediaCorpusLogic mediaCorpusLogic, ICommentCorpusLogic commentCorpusLogic, IHashtagLogic hashtagCorpusLogic)
		{
			_topicServicesLogic = topicServicesLogic;
			_instagramAccountLogic = instagramAccountLogic;
			_clientContext = clientContext;
			_responseResolver = responseResolver;

			_mediaCorpusLogic = mediaCorpusLogic;
			_commentCorpusLogic = commentCorpusLogic;
			_hashtagCorpusLogic = hashtagCorpusLogic;
		}
		#endregion

		/// <summary>
		/// Once have more accounts, could possibly batch each workers into groups of 2/3 
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		public async Task Begin(Settings settings)
		{
			//TODO: Need to link the original topic with the modified ones so that later they can be linked
			_workers = new ConcurrentQueue<ShortInstagramAccountModel>(await _instagramAccountLogic.GetInstagramAccountsOfUser(settings.AccountId, 1));
			var topicCategories = (await _topicServicesLogic.GetAllTopicCategories()).ToList();
			var initialWorkerAmount = _workers.Count;
			var pos = 0;

			Console.WriteLine("Total Topics To Fetch {0}", topicCategories.Count);
			var flattenCategories = topicCategories.SelectMany(_ => _.SubCategories).ToList();

			#region Make the list readable for instagram search
			flattenCategories.AddRange(topicCategories
				.Where(_=>!_.CategoryName.Contains("&"))
				.Select(_=>_.CategoryName));

			var uniqueCategories = flattenCategories.NormaliseStringList();

			uniqueCategories.ForEach(Console.WriteLine);

			#endregion

			foreach (var category in uniqueCategories)
			{
				Console.WriteLine(pos++);
				while (_workers.IsEmpty) //wait for worker to be available before continuing;
					await Task.Delay(TimeSpan.FromSeconds(0.35));
				
				_workers.TryDequeue(out var worker);
				_ = PerformFetch(category, worker);

				if (!topicCategories.Last().Equals(category)) continue;
				while (_workers.Count != initialWorkerAmount)	//wait for all to finish;
					await Task.Delay(TimeSpan.FromSeconds(0.35));
			}
		}

		private async Task<IResult<InstaSectionMedia>> SearchTopMedias(APIClientContainer container, string hashtag, int limit)
			=> await _responseResolver.WithResolverAsync(
				await container.Hashtag.GetTopHashtagMediaListAsync(hashtag,
					PaginationParameters.MaxPagesToLoad(limit)));
		private async Task<IResult<InstaSectionMedia>> SearchRecentMedias(APIClientContainer container, string hashtag, int limit)
			=> await _responseResolver.WithResolverAsync(
				await container.Hashtag.GetRecentHashtagMediaListAsync(hashtag,
					PaginationParameters.MaxPagesToLoad(limit)));
		private async Task<IResult<InstaSectionMedia>>PerformAction(Func<Task<IResult<InstaSectionMedia>>> @action, int attempts, TimeSpan delayPerRetry)
		{
			await Task.Delay(delayPerRetry);
			var results = await @action();
			if (results.Succeeded && results.Value.Medias.Count > 0) return results;
			attempts--;
			return await PerformAction(@action, attempts, delayPerRetry);
		}
		
		private async Task PerformFetch(SString topic, ShortInstagramAccountModel worker)
		{
			Console.WriteLine("Began Fetch for {0} using worker {1}", topic, worker.Username);
			await Task.Run(async () =>
			{
				// connect to a client in order to access instagram api
				var clientContainer = new APIClientContainer(_clientContext, worker.AccountId, worker.Id);
				
				// fetch data for this topic
				// first perform a hashtag search & related hashtags
				var hashtagResults = await _responseResolver
					.WithResolverAsync(await clientContainer.Hashtag.SearchHashtagAsync(topic));

				if (!hashtagResults.Succeeded && !hashtagResults.Value.Any())
					return;

				var hashtag = hashtagResults.Value.TakeAny(1).Single(); // take random one from list
				await Task.Delay(TimeSpan.FromSeconds(SecureRandom.NextDouble() + 1));
				
				// search medias for the hashtag
				var mediaResults = await PerformAction(() => SecureRandom.Next(0,1) == 0 ?
					SearchTopMedias(clientContainer, hashtag.Name, 1) : 
					SearchRecentMedias(clientContainer, hashtag.Name, 1), 
					3, TimeSpan.FromSeconds(0.85));
				
				if (!mediaResults.Succeeded)
					return;

				var currentlyStored = await _mediaCorpusLogic.GetMedias(topic, "en", 4000);

				foreach (var media in mediaResults.Value.Medias)
				{
					//clean the captions
				}

				// check for duplicates before storing
				// store caption, hashtags and comments
				await Task.Delay(TimeSpan.FromSeconds(SecureRandom.Next(5,7)));
			}).ContinueWith(t =>
			{
				Console.WriteLine("Finished Fetch for {0} using worker {1}", topic, worker.Username);
				_workers.Enqueue(worker);
			});
		}
	}
}
