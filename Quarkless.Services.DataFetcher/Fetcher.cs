using System;
using System.Linq;
using System.Threading.Tasks;
using QuarklessLogic.ServicesLogic;
using QuarklessContexts.Extensions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using InstagramApiSharp;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Enums;
using LanguageDetection;
using MongoDB.Bson;
using MoreLinq;
using QuarklessLogic.Logic.HashtagLogic;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessContexts.Models.ServicesModels.Corpus;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessContexts.Models.Topics;
using QuarklessLogic.ContentSearch.GoogleSearch;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Logic.InstagramAccountLogic;
using QuarklessLogic.Logic.ProfileLogic;
using QuarklessLogic.Logic.ResponseLogic;
using QuarklessLogic.Logic.TopicLookupLogic;
using QuarklessLogic.ServicesLogic.CorpusLogic;

namespace Quarkless.Services.DataFetcher
{
	public class Fetcher : IFetcher
	{
		#region Init
		private double _intervalWaitBetweenHashtagsAndMediaSearch;
		private int _mediaFetchAmount, _commentFetchAmount, _batchSize, _workerAccountType;

		private ConcurrentQueue<IEnumerable<ShortInstagramAccountModel>> _workerBatches;
		private readonly LanguageDetector _detector;
		private readonly ITopicServicesLogic _topicServicesLogic;
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private readonly IProfileLogic _profileLogic;
		private readonly IAPIClientContext _clientContext;
		private readonly IResponseResolver _responseResolver;
		private readonly IMediaCorpusLogic _mediaCorpusLogic;
		private readonly ICommentCorpusLogic _commentCorpusLogic;
		private readonly IHashtagLogic _hashtagCorpusLogic;
		private readonly IGoogleSearchLogic _googleSearchLogic;
		private readonly ITopicLookupLogic _topicLookup;
		public Fetcher(ITopicServicesLogic topicServicesLogic, IInstagramAccountLogic instagramAccountLogic, 
			IAPIClientContext clientContext, IResponseResolver responseResolver, IProfileLogic profileLogic,
			IMediaCorpusLogic mediaCorpusLogic, ICommentCorpusLogic commentCorpusLogic, IHashtagLogic hashtagCorpusLogic,
			IGoogleSearchLogic googleSearchLogic, ITopicLookupLogic topicLookup)
		{
			_topicServicesLogic = topicServicesLogic;
			_instagramAccountLogic = instagramAccountLogic;
			_profileLogic = profileLogic;
			_clientContext = clientContext;
			_responseResolver = responseResolver;

			_mediaCorpusLogic = mediaCorpusLogic;
			_commentCorpusLogic = commentCorpusLogic;
			_hashtagCorpusLogic = hashtagCorpusLogic;

			_googleSearchLogic = googleSearchLogic;
			_topicLookup = topicLookup;

			_detector = new LanguageDetector();
			_detector.AddAllLanguages();
		}
		#endregion

		/// <summary>
		/// wait until one batch is free
		/// upon free batch then send batch through function
		/// other function will perform the same thing here, (wait for single worker to be free)
		/// once done and batch is full --> batch complete
		/// rinse and repeat
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		public async Task Begin(Settings settings)
		{
			#region Initialise By Settings
			_intervalWaitBetweenHashtagsAndMediaSearch = settings.IntervalWaitBetweenHashtagsAndMediaSearch;
			_batchSize = settings.BatchSize;
			_workerAccountType = settings.WorkerAccountType;
			_mediaFetchAmount = settings.MediaFetchAmount;
			_commentFetchAmount = settings.CommentFetchAmount;

			_workerBatches = new ConcurrentQueue<IEnumerable<ShortInstagramAccountModel>>(
				(await _instagramAccountLogic.GetInstagramAccountsOfUser(settings.AccountId, _workerAccountType))
				.Batch(_batchSize));

			var initialWorkerAmount = _workerBatches.Count;
			#endregion
			
			if (settings.BuildInitialTopics)
			{
				var topicCategories = (await _topicServicesLogic.GetAllTopicCategories()).ToList();

				Console.WriteLine("Total Topics To Fetch {0}", topicCategories.Count);

				var categories = topicCategories.SelectMany(_ => _.SubCategories)
					.Distinct()
					.Shuffle()
					.ToList();
				foreach (var category in categories)
				{
					while (_workerBatches.IsEmpty)
						await Task.Delay(TimeSpan.FromSeconds(0.35));

					_workerBatches.TryDequeue(out var workers);
					_ = BatchBuildTopicIndex(category.RemoveSpaceAtEnd(), new ConcurrentQueue<ShortInstagramAccountModel>(workers.Shuffle()));

					if (!categories.Last().Equals(category)) continue;
					while (_workerBatches.Count != initialWorkerAmount)
						await Task.Delay(TimeSpan.FromSeconds(0.35));
				}
			}
			else
			{
				var accounts = await _instagramAccountLogic.GetInstagramAccounts(0);

				var customerTopics = accounts
					.Select(_ => _profileLogic.GetProfile(_.AccountId, _.Id).Result.Topics)
					.SelectMany(_ =>
					{
						var topics = new List<string> { _.TopicFriendlyName.ToLower() };
						topics.AddRange(_.SubTopics.Select(c => c.TopicName.ToLower()));
						return topics;
					}).Distinct().Shuffle().ToList();



				foreach (var topic in customerTopics)
				{
					Console.WriteLine("Started on category: {0}", topic);
					Console.WriteLine("----------------------------------------------------------------------");
					while (_workerBatches.IsEmpty) //wait for worker to be available before continuing;
						await Task.Delay(TimeSpan.FromSeconds(0.35));

					_workerBatches.TryDequeue(out var workers);
					_ = BatchOperation(topic, new ConcurrentQueue<ShortInstagramAccountModel>(workers));

					if (!customerTopics.Last().Equals(topic)) continue;
					while (_workerBatches.Count != initialWorkerAmount) //wait for all to finish;
						await Task.Delay(TimeSpan.FromSeconds(0.35));
				}
			}
		}

		private async Task BatchBuildTopicIndex(string category, ConcurrentQueue<ShortInstagramAccountModel> workers)
		{
			var initialWorkerAmount = workers.Count;
			workers.TryDequeue(out var worker);

			var id = await _topicLookup.AddTopic(new CTopic
			{
				Name = category,
				ParentTopicId = BsonObjectId.Empty.AsObjectId.ToString()
			});
			if (string.IsNullOrEmpty(id)) return;

			var clientContainer = new APIClientContainer(_clientContext, worker.AccountId, worker.Id);
			var hashtags =
				await clientContainer.Hashtag.SearchHashtagAsync(category);

			var totalTopics = new List<CTopic>();

			if (hashtags.Succeeded && hashtags.Value.Any())
			{
				totalTopics.AddRange(hashtags.Value.Select(_=>new CTopic
				{
					Name = _.Name,
					ParentTopicId = id
				}));
			}
			var related = await _googleSearchLogic.GetSuggestions(category);
			if(category.Any())
				totalTopics.AddRange(related.Select(_=>new CTopic
				{
					Name = _,
					ParentTopicId = id
				}));

			await _topicLookup.AddTopics(totalTopics);
			workers.Enqueue(worker);
			_workerBatches.Enqueue(workers);
		}

		private async Task GetBusinessCategoriesFromInstagram()
		{
			var pos = 0;
			var initialWorkerAmount = _workerBatches.Count;
			_workerBatches.TryPeek(out var workersPeek);

			var workers = new ConcurrentQueue<ShortInstagramAccountModel>(workersPeek);
			workers.TryPeek(out var workerp);

			var api = new APIClientContainer(_clientContext, workerp.AccountId, workerp.Id);
			var categories = await api.Business.GetCategoriesAsync();
			if (!categories.Succeeded)
				return;

			foreach (var category in categories.Value)
			{
				Console.WriteLine(pos++);
				while (workers.IsEmpty) //wait for worker to be available before continuing;
					await Task.Delay(TimeSpan.FromSeconds(0.35));

				workers.TryDequeue(out var worker);

				#region Category Stuff Function
				_ = Task.Run(async () =>
				{
					// connect to a client in order to access instagram api
					var clientContainer = new APIClientContainer(_clientContext, worker.AccountId, worker.Id);
					var results = await _responseResolver.WithClient(clientContainer).WithResolverAsync(
						await clientContainer.Business.GetSubCategoriesAsync(category.CategoryId));
					if (!results.Succeeded)
						return;

					var objectCategory = new TopicCategory
					{
						Category = new Category { CategoryName = category.CategoryName, CategoryId = category.CategoryId },
						SubCategories = results.Value.Select(_ => _.CategoryName).ToList().NormaliseStringList()
					};
					await _topicServicesLogic.AddTopicCategories(new List<TopicCategory> { objectCategory });
					await Task.Delay(TimeSpan.FromSeconds(SecureRandom.Next(5, 7)));
				}).ContinueWith(t =>
				{
					Console.WriteLine("Finished Fetch for {0} using worker {1}", category.CategoryName, worker.Username);
					workers.Enqueue(worker);
				});
				#endregion

				if (!categories.Value.Last().Equals(category)) continue;
				while (workers.Count != initialWorkerAmount)   //wait for all to finish;
					await Task.Delay(TimeSpan.FromSeconds(0.35));
			}
		}

		#region Search
		private async Task<IResult<InstaSectionMedia>> SearchTopMedias(APIClientContainer container, IEnumerable<InstaHashtag> hashtags,
			int limit)
		{
			var hashtag = hashtags.Where(_ => _.NonViolating && _.MediaCount > 10).TakeAny(1).Single(); // take random one from list
			Console.WriteLine("Picked hashtag {0}", hashtag.Name);
			return await _responseResolver.WithResolverAsync(
				await container.Hashtag.GetTopHashtagMediaListAsync(hashtag.Name,
					PaginationParameters.MaxPagesToLoad(limit)));
		}
		private async Task<IResult<InstaSectionMedia>> SearchRecentMedias(APIClientContainer container, IEnumerable<InstaHashtag> hashtags,
			int limit)
		{
			var hashtag = hashtags.Where(_ => _.NonViolating && _.MediaCount > 10).TakeAny(1).Single(); // take random one from list
			Console.WriteLine("Picked hashtag {0}", hashtag.Name);
			return await _responseResolver.WithResolverAsync(
				await container.Hashtag.GetRecentHashtagMediaListAsync(hashtag.Name,
					PaginationParameters.MaxPagesToLoad(limit)));
		}

		private async Task<IResult<InstaSectionMedia>>PerformAction(Func<Task<IResult<InstaSectionMedia>>> @action, int attempts, TimeSpan delayPerRetry)
		{
			await Task.Delay(delayPerRetry);
			var results = await @action();
			if (results.Succeeded && results.Value.Medias.Count > 0) return results;
			attempts--;
			return await PerformAction(@action, attempts, delayPerRetry);
		}
		private async Task<IResult<TResponse>> PerformAction<TResponse>(Func<Task<IResult<TResponse>>> @action, int attempts, TimeSpan delayPerRetry)
		{
			await Task.Delay(delayPerRetry);
			var results = await @action();
			if (results.Succeeded) return results;
			attempts--;
			return await PerformAction(@action, attempts, delayPerRetry);
		}
		#endregion

		private async Task BatchOperation(string topic, ConcurrentQueue<ShortInstagramAccountModel> workers)
		{
			var initialWorkerAmount = workers.Count;
			workers.TryPeek(out var workerPeek);
			var clientContainer = new APIClientContainer(_clientContext, workerPeek.AccountId, workerPeek.Id);

			#region Pick one random Hashtag against the topic and then search for posts (limit=2)
			// first perform a hashtag search & related hashtags
			var hashtagResults = await _responseResolver.WithClient(clientContainer)
				.WithResolverAsync(await clientContainer.Hashtag.SearchHashtagAsync(topic));

			await Task.Delay(TimeSpan.FromSeconds(SecureRandom.NextDouble() + _intervalWaitBetweenHashtagsAndMediaSearch));

			if (!hashtagResults.Succeeded && !hashtagResults.Value.Any())
				return;

			var mediaResults = await PerformAction(() => SecureRandom.Next(0, 1) == 0 ?
					SearchTopMedias(clientContainer, hashtagResults.Value, _mediaFetchAmount) :
					SearchRecentMedias(clientContainer, hashtagResults.Value, _mediaFetchAmount),
				3, TimeSpan.FromSeconds(0.85));

			if (!mediaResults.Succeeded)
				return;
			var medias = mediaResults.Value.Medias;
			#endregion

			//remove duplicates from list (check against db)
			var currentMedias = await _mediaCorpusLogic.GetMedias(topic, null, 4000);
			var filteredMedias = medias.Where(_ => currentMedias.All(p => p.MediaId != _.Pk) 
			                                       && _.Caption!=null).ToList();

			if (!filteredMedias.Any()) return;

			foreach (var media in filteredMedias)
			{
				while (workers.IsEmpty) //wait for worker to be available before continuing;
					await Task.Delay(TimeSpan.FromSeconds(0.35));
				workers.TryDequeue(out var worker);

				#region Fetch Start Function
				_ = Task.Run(async () =>
				{
					// connect to a client in order to access instagram api
					var clientApi = new APIClientContainer(_clientContext, worker.AccountId, worker.Id);

					#region STORE POST CAPTION & HASHTAGS
					var hashtags = media.Caption.Text.FilterHashtags().ToList();
					var caption = media.Caption.Text
						.RemoveHashtagsFromText()
						.RemoveMentionsFromText()
						.RemoveCurrencyFromText()
						.RemovePhoneNumbersFromText()
						.RemoveWebAddressesFromText()
						.RemoveNonWordsFromText()
						.RemoveHorizontalSeparationFromText()
						.RemoveVerticalSeparationFromText()
						.RemoveLargeSpacesInText();

					if (!_detector.Detect(caption).Equals("en")) return;
					if (!string.IsNullOrEmpty(caption) || !string.IsNullOrWhiteSpace(caption))
					{
						await Task.Delay(TimeSpan.FromSeconds(SecureRandom.NextDouble() + 1));
						var mediaCorpus = new MediaCorpus
						{
							Caption = caption,
							CommentsCount = media.CommentsCount,
							LikesCount = media.LikesCount,
							Location = media.Location,
							MediaId = media.Pk,
							TakenAt = media.TakenAt,
							OriginalCaption = media.Caption.Text,
							Topic = topic,
							ViewsCount = media.ViewCount,
						};
						Console.WriteLine("Storing post...");
						await _mediaCorpusLogic.AddMedia(mediaCorpus);
					}

					if (hashtags.Any())
					{
						var hashtagCorpus = new HashtagsModel
						{
							Hashtags = hashtags.ToList(),
							Topic = topic
						};
						Console.WriteLine("Storing hashtags of post...");
						await _hashtagCorpusLogic.AddHashtagsToRepositoryAndCache(new[] {hashtagCorpus});
					}
					#endregion

					if (!int.TryParse(media.CommentsCount, out var commentCount) && commentCount <= 0)
						return;

					var mediaCommentsAsync =
						await clientApi.Comment.GetMediaCommentsAsync(media.Pk, PaginationParameters.MaxPagesToLoad(_commentFetchAmount));

					if (!mediaCommentsAsync.Succeeded)
						return;

					var comments = new List<CommentCorpus>();

					void AddInnerComments(IEnumerable<InstaComment> commentsIn)
					{
						foreach (var comment in commentsIn)
						{
							var commentTags = comment.Text.FilterHashtags().ToList();
							if (commentTags.Count > 3)
							{
								_hashtagCorpusLogic.AddHashtagsToRepositoryAndCache(new[]
								{
									new HashtagsModel
									{
										Hashtags = commentTags, 
										Topic = topic
									}
								});
							}
							if(comment.User.Pk == media.User.Pk) continue;
							var commentExtract = comment.Text
								.RemoveHashtagsFromText()
								.RemoveMentionsFromText()
								.RemoveCurrencyFromText()
								.RemovePhoneNumbersFromText()
								.RemoveWebAddressesFromText()
								.RemoveNonWordsFromText()
								.RemoveHorizontalSeparationFromText()
								.RemoveVerticalSeparationFromText()
								.RemoveLargeSpacesInText();

							if (!string.IsNullOrEmpty(commentExtract) || !string.IsNullOrWhiteSpace(commentExtract))
								comments.Add(new CommentCorpus
								{
									Comment = commentExtract,
									Created = comment.CreatedAtUtc,
									MediaId = media.Pk,
									NumberOfLikes = comment.LikesCount,
									NumberOfReplies = comment.ChildCommentCount,
									Topic = topic,
									Username = comment.User.UserName,
									IsReply = comment.ParentCommentId > 0
								});
							
							if (comment.ChildComments.Count > 0)
								AddInnerComments(comment.ChildComments);
						}
					}

					AddInnerComments(mediaCommentsAsync.Value.Comments);

					if (comments.Any())
					{
						Console.WriteLine("Adding Comments fetched {0}", comments.Count);
						await _commentCorpusLogic.AddComments(comments);
					}

					await Task.Delay(TimeSpan.FromSeconds(SecureRandom.Next(1, 2)));
					Console.WriteLine("Moving to next media...");
				}).ContinueWith(t =>
				{
					Console.WriteLine("Finished Fetch for {0} using worker {1}", media?.Pk, worker.Username);
					workers.Enqueue(worker);
				});
				#endregion 

				if (!filteredMedias.Last().Equals(media)) continue;
				while (workers.Count != initialWorkerAmount)   //wait for all to finish;
					await Task.Delay(TimeSpan.FromSeconds(0.35));
				_workerBatches.Enqueue(workers);
			}
		}
	}
}
