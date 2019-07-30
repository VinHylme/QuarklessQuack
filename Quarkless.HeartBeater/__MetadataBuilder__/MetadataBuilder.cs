using InstagramApiSharp.Classes.Models;
using MoreLinq;
using Quarkless.HeartBeater.__Init__;
using Quarkless.HeartBeater.ContentSearch;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.ResponseModels;
using QuarklessContexts.Models.ServicesModels.HeartbeatModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Logic.InstagramAccountLogic;
using QuarklessLogic.Logic.ProxyLogic;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;
using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Quarkless.HeartBeater.__MetadataBuilder__
{
	public static class MetadataExtensions
	{
		public static IEnumerable<MediaResponse> RandomAny(this Media medias, int amount)
		{
			return medias.Medias.TakeAny(amount);
		}
		public static IEnumerable<TA> Between<TA>(this IEnumerable<TA> obj, int start, int limit)
		{
			for(int j = start; j < limit + start; j++)
			{
				yield return obj.ElementAtOrDefault(j);
			}
		}
		public static List<List<TCut>> CutObject<TCut>(this List<TCut> @item, int cutAmount) where TCut: new()
		{
			int pos = 0;
			List<List<TCut>> objects = new List<List<TCut>>();
			for(int x = 0 ; x <= @item.Count(); x++)
			{
				if(x%cutAmount==0 && x != 0)
				{
					objects.Add(@item.Between(pos,cutAmount).ToList());
				}
				pos = x;
			}
			return objects;
		}
		public static IEnumerable<Media> CutObjects(this Media medias, int amount)
		{
			if (medias == null) return null;
			int pos = 0;
			List<Media> media_ = new List<Media>();
			for(int x = 0 ; x <= medias.Medias.Count; x++)
			{
				if (x % amount == 0 && x!=0)
				{
					media_.Add(new Media
					{
						Medias = medias.Medias.Between(pos,amount).ToList(),
						errors = medias.errors
					});
					pos = x;
				}
			}
			return media_;
		}
		public static T RandomAny<T>(this IEnumerable<T> @ts)
		{
			return ts.ElementAtOrDefault(SecureRandom.Next(@ts.Count() - 1));
		}
	}
	public class MetadataBuilder
	{
		private readonly List<Assignments> _assignments;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly IProxyLogic _proxyLogic;
		private readonly IAPIClientContext _context;
		public MetadataBuilder(List<Assignments> assignments, IAPIClientContext aPIClient, IHeartbeatLogic heartbeatLogic, IProxyLogic proxyLogic)
		{
			_proxyLogic = proxyLogic;
			_assignments = assignments;
			_heartbeatLogic = heartbeatLogic;
			_context = aPIClient;
		}
		private SearchImageModel GoogleQueryBuilder(string color, string topics, List<string> sites, int limit = 10,
		string imageType = null, string exactSize = null, string similarImage = null)
		{
			var query = new SearchImageModel
			{
				color = color,
				prefix_keywords = string.Join(", ", sites),
				keywords = topics,
				limit = limit,
				no_download = true,
				print_urls = true,
				type = imageType,
				exact_size = exactSize,
				proxy = null,
				similar_images = similarImage,
				size = string.IsNullOrEmpty(exactSize) ? "large" : null
			};
			return query;
		}

		public async Task BuildGoogleImages(int limit = 50, int topicAmount = 1, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildGoogleImages");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				await _assignments.ParallelForEachAsync(async worker =>
				{
					ContentSearcherHandler searcher = new ContentSearcherHandler(worker.Worker, worker.Worker.GetContext.Proxy);
					
					var res = await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaForSpecificUserGoogle,worker.Topic.TopicFriendlyName,worker.InstagramRequests.InstagramAccount.Id);
					
					var by = new By { ActionType = 0, User = worker.InstagramRequests.InstagramAccount.Id };
					
					if (res==null || res.Count() <= 0 || res.Where(x => x!=null).All(s=>!s.SeenBy.Any(sb=> sb.User == by.User && sb.ActionType==by.ActionType))){
						List<string> topicTotal = new List<string>();

						var selectSubTopic = worker.Topic.SubTopics.Shuffle().ElementAt(SecureRandom.Next(worker.Topic.SubTopics.Count - 1));
						if (selectSubTopic == null)
							return;
						List<string> randomtopic = selectSubTopic.RelatedTopics.Distinct().ToList();
						randomtopic.Add(selectSubTopic.TopicName);
						string searchQueryTopics = randomtopic.ElementAt(SecureRandom.Next(randomtopic.Count - 1));

						//var subtopics = worker.Topic.SubTopics.Select(a=>a.TopicName);
						//var related = worker.Topic.SubTopics.Select(s=>s.RelatedTopics).SquashMe();
						//topicTotal.AddRange(subtopics);
						//topicTotal.AddRange(related);
						//var topicSelect = topicTotal.Distinct().TakeAny(topicAmount).ToList();

						var prf = worker.InstagramRequests.Profile;
						var colrSelect = prf.Theme.Colors.ElementAt(SecureRandom.Next(prf.Theme.Colors.Count));
						var imtype = ((ImageType)prf.AdditionalConfigurations.ImageType).GetDescription();
						var go = searcher.SearchViaGoogle(GoogleQueryBuilder(colrSelect.Name, searchQueryTopics,
							prf.AdditionalConfigurations.Sites,limit,exactSize:prf.AdditionalConfigurations.PostSize,imageType:imtype));
						if (go != null) { 
							if (go.StatusCode == ResponseCode.Success)
							{
								await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaForSpecificUserGoogle, worker.Topic.TopicFriendlyName, worker.InstagramRequests.InstagramAccount.Id, proxy: worker.Worker.GetContext.Proxy);
								
								var cut = go.Result.CutObjects(cutBy).ToList();
								cut.ForEach(async x =>
								{
									await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaForSpecificUserGoogle, worker.Topic.TopicFriendlyName,
										new __Meta__<Media> (x), worker.InstagramRequests.InstagramAccount.Id);
								});
							}
						}
					}
				});
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			Console.WriteLine($"Ended - BuildGoogleImages : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildYandexImagesQuery(int limit = 2, int topicAmount = 1, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildYandexImagesQuery");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				await _assignments.ParallelForEachAsync(async worker =>
				{
					if(worker.Worker.GetContext.Proxy == null)
					{				
						var proxy = await _proxyLogic.RetrieveRandomProxy(connectionType: ConnectionType.Mobile, get:true, cookies:true);
						if (proxy != null)
						{
							worker.Worker.GetContext.Proxy = proxy;
						}
					}
					ContentSearcherHandler searcher = new ContentSearcherHandler(worker.Worker, worker.Worker.GetContext.Proxy);
					List<string> topicTotal = new List<string>();

					var res = await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaForSepcificUserYandexQuery, worker.Topic.TopicFriendlyName, worker.InstagramRequests.InstagramAccount.Id);
					var by = new By { ActionType = 0, User = worker.InstagramRequests.InstagramAccount.Id };
					
					if (res == null || res.Count() <= 0 || res.Where(x => x != null).All(s => !s.SeenBy.Any(sb => sb.User == by.User && sb.ActionType == by.ActionType)))
					{					
						var selectSubTopic = worker.Topic.SubTopics.Shuffle().ElementAt(SecureRandom.Next(worker.Topic.SubTopics.Count-1));
						if (selectSubTopic == null)
							return ;
						List<string> randomtopic = selectSubTopic.RelatedTopics.Distinct().ToList();
						randomtopic.Add(selectSubTopic.TopicName);
						string searchQuery = randomtopic.ElementAt(SecureRandom.Next(randomtopic.Count - 1));

						var prf = worker.InstagramRequests.Profile;
						var colrSelect = prf.Theme.Colors.ElementAt(SecureRandom.Next(prf.Theme.Colors.Count - 1));

						var convertedSize = prf.AdditionalConfigurations.PostSize!=null ? prf.AdditionalConfigurations.PostSize.Split(','):null;
						
						YandexSearchQuery yandexSearchQuery =new YandexSearchQuery
						{
							OriginalTopic = worker.Topic.TopicFriendlyName,
							SearchQuery = searchQuery,
							Color = colrSelect.Name.GetValueFromDescription<ColorType>(),
							Format = FormatType.Any,
							Orientation = Orientation.Any,
							Size = SizeType.Large,
							Type = (ImageType) prf.AdditionalConfigurations.ImageType
						};

						var yan = searcher.SearchViaYandex(yandexSearchQuery, limit);
						if (yan != null) { 
							if (yan.StatusCode == ResponseCode.Success)
							{
								await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaForSepcificUserYandexQuery, worker.Topic.TopicFriendlyName, worker.InstagramRequests.InstagramAccount.Id, proxy: worker.Worker.GetContext.Proxy);
								var cut = yan.Result.CutObjects(cutBy).ToList();
								cut.ForEach(async x =>
								{
									await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaForSepcificUserYandexQuery, worker.Topic.TopicFriendlyName,
										new __Meta__<Media>(x), worker.InstagramRequests.InstagramAccount.Id);
								});
							}
							else if(yan.StatusCode == ResponseCode.Timeout)
							{

							}
							else if(yan.StatusCode == ResponseCode.CaptchaRequired)
							{
							
							}
						}
					}
				});
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			Console.WriteLine($"Ended - BuildYandexImagesQuery : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildYandexImages(int limit = 3, int takeTopicAmount = 1, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildYandexImages");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				await _assignments.ParallelForEachAsync(async worker =>
				{
					if (worker.Worker.GetContext.Proxy == null)
					{
						var proxy = await _proxyLogic.RetrieveRandomProxy(connectionType: ConnectionType.Mobile, get:true, cookies: true);
						if (proxy != null)
						{
							worker.Worker.GetContext.Proxy = proxy;
						}
					}
					ContentSearcherHandler searcher = new ContentSearcherHandler(worker.Worker, worker.Worker.GetContext.Proxy);
					var imalike = worker.InstagramRequests.Profile.Theme.ImagesLike;
					var res = await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaForSpecificUserYandex, worker.Topic.TopicFriendlyName, worker.InstagramRequests.InstagramAccount.Id);

					var by = new By { ActionType = 0, User = worker.InstagramRequests.InstagramAccount.Id };
					if (res == null || res.Count() <= 0 || res.Where(x => x != null).All(s => !s.SeenBy.Any(sb => sb.User == by.User && sb.ActionType == by.ActionType)))
					{
						var filter = imalike.Shuffle().Where(s => s.TopicGroup == worker.Topic.TopicFriendlyName);
						var yan = searcher.SearchViaYandexBySimilarImages(filter.TakeAny(takeTopicAmount).ToList(),limit);

						if(yan == null 
						|| yan.StatusCode == ResponseCode.CaptchaRequired 
						|| yan.StatusCode == ResponseCode.InternalServerError 
						|| yan.StatusCode == ResponseCode.ReachedEndAndNull) 
						{
							yan = searcher.SearchSimilarImagesViaGoogle(filter.TakeAny(takeTopicAmount).ToList(),limit*25);
							if (yan == null)
							{
								yan = searcher.SearchYandexSimilarSafeMode(filter.TakeAny(takeTopicAmount).ToList(), limit * 25);
							}
						}
						if (yan != null) { 
							if (yan.StatusCode == ResponseCode.Success) { 
								await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaForSpecificUserYandex, worker.Topic.TopicFriendlyName, worker.InstagramRequests.InstagramAccount.Id, proxy: worker.Worker.GetContext.Proxy);
								var cut = yan.Result.CutObjects(cutBy).ToList();
								cut.ForEach(async x =>
								{
									await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaForSpecificUserYandex, worker.Topic.TopicFriendlyName,
										new __Meta__<Media>(x), worker.InstagramRequests.InstagramAccount.Id);
								});
							}
							else if(yan.StatusCode == ResponseCode.Timeout)
							{
								//could try with a different proxy
								//await BuildYandexImages(limit,takeTopicAmount,cutBy);
							}
							else if (yan.StatusCode == ResponseCode.CaptchaRequired)
							{
								if (yan.Result?.Medias?.Count > 0)
								{
									await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaForSpecificUserYandex, worker.Topic.TopicFriendlyName, worker.InstagramRequests.InstagramAccount.Id, proxy: worker.Worker.GetContext.Proxy);
									var cut = yan.Result.CutObjects(cutBy).ToList();
									cut.ForEach(async x =>
									{
										await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaForSpecificUserYandex, worker.Topic.TopicFriendlyName,
											new __Meta__<Media>(x), worker.InstagramRequests.InstagramAccount.Id);
									});
								}
							}
						}
					}
				});
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			Console.WriteLine($"Ended - BuildYandexImages : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildBase(int limit =2, int cutBy = 1, int takeTopicAmount = 1)
		{
			Console.WriteLine("Began - BuildBase");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try {
				await _assignments.ParallelForEachAsync(async worker =>
				{
					ContentSearcherHandler searcher = new ContentSearcherHandler(worker.Worker, worker.Worker.GetContext.Proxy);
					List<string> topicTotal = new List<string>();

					var subtopics = worker.Topic.SubTopics.Shuffle().Select(a => a.TopicName);
					var related = worker.Topic.SubTopics.Shuffle().Select(s => s.RelatedTopics).SquashMe();

					topicTotal.AddRange(subtopics);
					topicTotal.AddRange(related);

					var topicSelect = topicTotal.Distinct().TakeAny(takeTopicAmount).ToList();

					var mediaByTopics = await searcher.SearchMediaDetailInstagram(topicSelect, limit);

					if (mediaByTopics != null) { 
						await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByTopic,worker.Topic.TopicFriendlyName, proxy:worker.Worker.GetContext.Proxy);
						var lmeta = mediaByTopics.CutObjects(cutBy).ToList();

						lmeta.ToList().ForEach(async z => {
							z.Medias = z.Medias.Where(t => !t.HasLikedBefore && !t.IsCommentsDisabled && t.User.Username != worker.InstagramRequests.InstagramAccount.Username).ToList();
							await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByTopic, worker.Topic.TopicFriendlyName, 
								new __Meta__<Media>(z));
						});
					}
				});
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			watch.Stop();
			Console.WriteLine($"Ended - BuildBase : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildUserFromLikers(int cutBy = 1, double sleepTime = 0.05,
			int takeMediaAmount = 7, int takeUserMediaAmount = 100)
		{
			Console.WriteLine("Began - BuildUserFromLikers");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try {
				await _assignments.ParallelForEachAsync(async worker =>
				{
					ContentSearcherHandler searcher = new ContentSearcherHandler(worker.Worker, worker.Worker.GetContext.Proxy);
					var results = await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopic, worker.Topic.TopicFriendlyName);

					if (results != null)
					{
						await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersViaPostLiked, worker.Topic.TopicFriendlyName, proxy: worker.Worker.GetContext.Proxy);
						foreach(var v in results.Shuffle().TakeAny(takeMediaAmount)) { 
							if (v != null) { 
								foreach(var _ in v.ObjectItem.Medias.Where(t=>t.MediaFrom == MediaFrom.Instagram))
								{
									var suggested = await searcher.SearchInstagramMediaLikers(_.MediaId);
									if (suggested == null && suggested.Count < 1) return;
									#region Detail Search (off)
									//suggested = suggested.Select(check =>
									//{
									//	var details = searcher.SearchInstagramFullUserDetail(check.UserId).GetAwaiter().GetResult();
									//	if (details != null) { 
									//		double ratioff = (double)details.UserDetail.FollowerCount / details.UserDetail.FollowingCount;
									//		if (ratioff > 1 && details.UserDetail.MediaCount > 3)
									//		{
									//			return check;
									//		}
									//	}
									//	return null;
									//}).Where(an => an != null).ToList();
									#endregion
									if (suggested != null)
									{
										var totalcut = suggested.TakeAny(takeUserMediaAmount).ToList().CutObject(cutBy);
										totalcut.ForEach(async s =>
										{
											var filtered = s.Where(x => x.Username != worker.InstagramRequests.InstagramAccount.Username).ToList();
											await _heartbeatLogic.AddMetaData(MetaDataType.FetchUsersViaPostLiked, worker.Topic.TopicFriendlyName,
												new __Meta__<List<UserResponse<string>>>(s));
										});
									}
									await Task.Delay(TimeSpan.FromSeconds(sleepTime));
								}
							}
						};
					}
				});
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			watch.Stop();
			Console.WriteLine($"Ended - BuildUserFromLikers : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildMediaFromUsersLikers(int limit = 1, int cutBy = 1, double sleepTime = 0.05,
			int takeMediaAmount = 7, int takeUserMediaAmount = 10)
		{
			Console.WriteLine("Began - BuildMediaFromUsersLikers");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try {
				await _assignments.ParallelForEachAsync(async worker =>
				{
					ContentSearcherHandler searcher = new ContentSearcherHandler(worker.Worker, worker.Worker.GetContext.Proxy);
					var results = (await _heartbeatLogic.GetMetaData<List<UserResponse<string>>>
					(MetaDataType.FetchUsersViaPostLiked, worker.Topic.TopicFriendlyName)).ToList();

					if (results != null)
					{
						await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByLikers, worker.Topic.TopicFriendlyName, proxy: worker.Worker.GetContext.Proxy);
						foreach(var _ in results.Shuffle().TakeAny(takeMediaAmount)) { 
							if (_ != null) { 
								foreach(var d in _.ObjectItem)
								{
									var suggested = await searcher.SearchUsersMediaDetailInstagram(d.Username, limit);
									if (suggested != null) { 
										var sugVCut = suggested.CutObjects(cutBy).ToList();
										foreach(var z in sugVCut?.TakeAny(takeUserMediaAmount))
										{
											z.Medias = z.Medias.Where(t => !t.HasLikedBefore && !t.IsCommentsDisabled && t.User.Username != worker.InstagramRequests.InstagramAccount.Username).ToList();
											await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByLikers, worker.Topic.TopicFriendlyName, new __Meta__<Media>(z));
										};
									}
									await Task.Delay(TimeSpan.FromSeconds(sleepTime));
								};
							}
						};
					}
				});
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			watch.Stop();
			Console.WriteLine($"Ended - BuildMediaFromUsersLikers : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s"); 
		}
		public async Task BuildUsersFromCommenters(int limit = 1, int cutBy = 1, double sleepTime = 0.05, 
			int takeMediaAmount = 7, int takeUserMediaAmount = 100)
		{
			Console.WriteLine("Began - BuildUsersFromCommenters");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				await _assignments.ParallelForEachAsync(async worker =>
				{
					ContentSearcherHandler searcher = new ContentSearcherHandler(worker.Worker, worker.Worker.GetContext.Proxy);
					var results = await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopic, worker.Topic.TopicFriendlyName);
					if (results != null)
					{
						await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersViaPostCommented, worker.Topic.TopicFriendlyName, proxy: worker.Worker.GetContext.Proxy);
						foreach(var v in results.Shuffle().TakeAny(takeMediaAmount))
						{
							if (v != null)
							{
								foreach(var _ in v.ObjectItem.Medias)
								{
									if (!_.IsCommentsDisabled && _.MediaFrom == MediaFrom.Instagram && _.MediaId!=null) { 
										var suggested = await searcher.SearchInstagramMediaCommenters(_.MediaId, limit);
										if(suggested == null && suggested.Count < 1) return;

										#region Detail Search (off)
										//suggested = suggested.Select(check =>
										//{
										//	Task.Delay(TimeSpan.FromSeconds(1));
										//	var details = searcher.SearchInstagramFullUserDetail(check.UserId).GetAwaiter().GetResult();
										//	if (details != null) { 
										//		double ratioff = (double)details.UserDetail.FollowerCount / details.UserDetail.FollowingCount;
										//		if(ratioff > 1 && details.UserDetail.MediaCount > 3)
										//		{
										//			return check;
										//		}
										//	}
										//	return null;
										//}).Where(an => an!=null).ToList();
										#endregion

										if (suggested != null)
										{
											var totalcut = suggested.TakeAny(takeUserMediaAmount).ToList().CutObject(cutBy);
											foreach(var s in totalcut)
											{
												var filtered = s.Where(x => x.Username != worker.InstagramRequests.InstagramAccount.Username).ToList();
												await _heartbeatLogic.AddMetaData(MetaDataType.FetchUsersViaPostCommented, worker.Topic.TopicFriendlyName,new __Meta__<List<UserResponse<InstaComment>>>(filtered));
											}
										}
									}
									await Task.Delay(TimeSpan.FromSeconds(sleepTime));
								};
							}
						};
					}
				});
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			watch.Stop();
			Console.WriteLine($"Ended - BuildUsersFromCommenters : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildCommentsFromSpecifiedSource(MetaDataType extractFrom, MetaDataType saveTo,bool includeUser = false, int limit = 1, int cutBy = 1, double secondSleep = 0.05, 
			int takeMediaAmount = 10, int takeuserAmount = 100)
		{
			Console.WriteLine("Began - BuildCommentsFromSpecifiedSource " + saveTo.ToString());
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				await _assignments.ParallelForEachAsync(async worker =>
				{
					ContentSearcherHandler searcher = new ContentSearcherHandler(worker.Worker, worker.Worker.GetContext.Proxy);
					var res = await _heartbeatLogic.GetMetaData<Media>(extractFrom, worker.Topic.TopicFriendlyName,includeUser ? worker.InstagramRequests.InstagramAccount.Id:null);
					if (res != null) { 
						await _heartbeatLogic.RefreshMetaData(saveTo, worker.Topic.TopicFriendlyName, proxy: worker.Worker.GetContext.Proxy);
						foreach(var _ in res.Shuffle().TakeAny(takeMediaAmount))
						{
							if (_ != null)
							{
								foreach(var __ in _.ObjectItem.Medias)
								{
									if (!__.IsCommentsDisabled && __.MediaFrom == MediaFrom.Instagram && __.MediaId != null)
									{
										var suggested = await searcher.SearchInstagramMediaCommenters(__.MediaId, limit);
										if (suggested == null && suggested?.Count < 1) 
											return;

										if (suggested != null)
										{
											var totalcut = suggested.TakeAny(takeuserAmount).ToList().CutObject(cutBy);
											foreach (var s in totalcut)
											{
												var filtered = s.Where(x => x.Username != worker.InstagramRequests.InstagramAccount.Username).ToList();
											
												await _heartbeatLogic.AddMetaData(saveTo, worker.Topic.TopicFriendlyName,
												new __Meta__<List<UserResponse<InstaComment>>>(filtered),
												includeUser ? worker.InstagramRequests.InstagramAccount.Id : null);
											}
										}
									}
									await Task.Delay(TimeSpan.FromSeconds(secondSleep));
								};
							}
						};
					}
				});
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			watch.Stop();
			Console.WriteLine($"Ended - BuildCommentsFromSpecifiedSource  {saveTo.ToString()} : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildMediaFromUsersCommenters(int limit = 1, int cutBy = 1, double secondsSleep = 0.05, 
			int takeMediaAmount = 10, int takeUserMediaAmount = 10)
		{
			Console.WriteLine("Began - BuildMediaFromUsersCommenters");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				await _assignments.ParallelForEachAsync(async worker =>
				{
					ContentSearcherHandler searcher = new ContentSearcherHandler(worker.Worker, worker.Worker.GetContext.Proxy);
					var results = (await _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>
					(MetaDataType.FetchUsersViaPostCommented, worker.Topic.TopicFriendlyName)).ToList();

					if (results != null)
					{
						await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByCommenters, worker.Topic.TopicFriendlyName, proxy: worker.Worker.GetContext.Proxy);
						foreach(var _ in results.Shuffle().TakeAny(takeMediaAmount)) {
							if (_ != null)
							{
								foreach(var d in _.ObjectItem)
								{	
									var suggested = await searcher.SearchUsersMediaDetailInstagram(d.Username, limit);								
									if (suggested != null)
									{
										var sugVCut = suggested.CutObjects(cutBy).ToList();
										foreach(var z in sugVCut.TakeAny(takeUserMediaAmount))
										{
											z.Medias = z.Medias.Where(t => !t.HasLikedBefore && !t.IsCommentsDisabled && t.User.Username != worker.InstagramRequests.InstagramAccount.Username).ToList();
											var comments = new __Meta__<Media>(z);
											await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByCommenters, worker.Topic.TopicFriendlyName, comments);
										};
									}
									await Task.Delay(TimeSpan.FromSeconds(secondsSleep));
								};
							}
						};
					}
				});
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			watch.Stop();
			Console.WriteLine($"Ended - BuildMediaFromUsersCommenters : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildUsersTargetListMedia(int limit = 1, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildUsersTargetListMedia");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				await _assignments.ParallelForEachAsync(async worker => {
					ContentSearcherHandler searcher = new ContentSearcherHandler(worker.Worker, worker.Worker.GetContext.Proxy);
					var user = worker.InstagramRequests.InstagramAccount;
					var usertargetlist = worker.InstagramRequests.Profile.UserTargetList;
					if(usertargetlist!=null && usertargetlist.Count > 0)
					{
						foreach(var tuser in usertargetlist)
						{
							var fetchUsersMedia = await searcher.SearchUsersMediaDetailInstagram(tuser, limit);
							if (fetchUsersMedia != null)
							{
								await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByUserTargetList, worker.Topic.TopicFriendlyName, user.Id, proxy: worker.Worker.GetContext.Proxy);
								foreach(var s in fetchUsersMedia.CutObjects(cutBy))
								{
									await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByUserTargetList, worker.Topic.TopicFriendlyName,
										new __Meta__<Media>(s), user.Id);
								};
							}

						}			
					}			
				});
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			Console.WriteLine($"Ended - BuildUsersTargetListMedia : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildLocationTargetListMedia(int limit = 1, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildLocationTargetListMedia");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				await _assignments.ParallelForEachAsync(async worker => {
					ContentSearcherHandler searcher = new ContentSearcherHandler(worker.Worker, worker.Worker.GetContext.Proxy);
					var user = worker.InstagramRequests.InstagramAccount;
					var locationtargetlist = worker.InstagramRequests.Profile.LocationTargetList;
					if (locationtargetlist != null && locationtargetlist.Count > 0)
					{
						foreach (var tloc in locationtargetlist)
						{
							var fetchUsersMedia = await searcher.SearchTopLocationMediaDetailInstagram(tloc, limit);
							if (fetchUsersMedia != null)
							{
								await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByUserLocationTargetList, worker.Topic.TopicFriendlyName, user.Id, proxy: worker.Worker.GetContext.Proxy);
								foreach(var s in fetchUsersMedia.CutObjects(cutBy))
								{
									await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByUserLocationTargetList, worker.Topic.TopicFriendlyName,
										new __Meta__<Media>(s), user.Id);
								};
							}

						}
					}
				});
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			Console.WriteLine($"Ended - BuildLocationTargetListMedia : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildUsersOwnMedias(IInstagramAccountLogic _logic, int limit = 1, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildUsersOwnMedias");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				await _assignments.ParallelForEachAsync(async worker => {
					ContentSearcherHandler searcher = new ContentSearcherHandler(worker.Worker, worker.Worker.GetContext.Proxy);
					var user = worker.InstagramRequests.InstagramAccount;
					var fetchUsersMedia = await searcher.SearchUsersMediaDetailInstagram(user.Username,limit);
					if (fetchUsersMedia != null)
					{
						await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUserOwnProfile, worker.Topic.TopicFriendlyName, user.Id, proxy: worker.Worker.GetContext.Proxy);
						foreach(var s in fetchUsersMedia.CutObjects(cutBy))
						{
							await _heartbeatLogic.AddMetaData(MetaDataType.FetchUserOwnProfile, worker.Topic.TopicFriendlyName,
								new __Meta__<Media>(s),user.Id);
						};
						var user_ = fetchUsersMedia.Medias.FirstOrDefault();
						if (user_ != null) { 
							var details = await searcher.SearchInstagramFullUserDetail(user_.User.UserId);
							
							await _logic.PartialUpdateInstagramAccount(user.AccountId, user.Id, new QuarklessContexts.Models.InstagramAccounts.InstagramAccountModel
							{
								FollowersCount = details.UserDetail.FollowerCount,
								FollowingCount = details.UserDetail.FollowingCount,
								TotalPostsCount = details.UserDetail.MediaCount,
								Email = details.UserDetail.PublicEmail,
								ProfilePicture = details.UserDetail.ProfilePicture ?? details.UserDetail.ProfilePicUrl,
								FullName = details.UserDetail.FullName,
								UserBiography = new QuarklessContexts.Models.InstagramAccounts.Biography
								{
									Text = details.UserDetail.BiographyWithEntities.Text,
									Hashtags = details.UserDetail.BiographyWithEntities.Entities.Select(_=>_.Hashtag.Name).ToList()
								},
								IsBusiness =  details.UserDetail.IsBusiness,
								Location = new QuarklessContexts.Models.Profiles.Location
								{
									Address = details.UserDetail.AddressStreet,
									City = details.UserDetail.CityName,
									Coordinates = new QuarklessContexts.Models.Profiles.Coordinates
									{
										Latitude = details.UserDetail.Latitude,
										Longitude = details.UserDetail.Longitude
									},
									PostCode = details.UserDetail.ZipCode
								},
								PhoneNumber = details.UserDetail.PublicPhoneCountryCode + (!string.IsNullOrEmpty(details.UserDetail.PublicPhoneNumber) ? details.UserDetail.PublicPhoneNumber : details.UserDetail.ContactPhoneNumber)
							});
						}
					}
				});
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			Console.WriteLine($"Ended - BuildUsersOwnMedias : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildUsersFeed(int limit = 1, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildUsersFeed");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				await _assignments.ParallelForEachAsync(async worker => {
					ContentSearcherHandler searcher = new ContentSearcherHandler(worker.Worker, worker.Worker.GetContext.Proxy);
					var user = worker.InstagramRequests.InstagramAccount;
					searcher.ChangeUser(new APIClientContainer(_context,user.AccountId,user.Id));
					var fetchUsersMedia = await searcher.SearchUserFeedMediaDetailInstagram(limit:limit);
					searcher.ChangeUser(worker.Worker);
					if (fetchUsersMedia != null)
					{
						await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersFeed, worker.Topic.TopicFriendlyName, user.Id, proxy: worker.Worker.GetContext.Proxy);
						foreach(var s in fetchUsersMedia.CutObjects(cutBy))
						{
							s.Medias = s.Medias.Where(_=>!_.HasLikedBefore && !_.IsCommentsDisabled && _.User.Username!=user.Username).ToList();
							
							await _heartbeatLogic.AddMetaData(MetaDataType.FetchUsersFeed, worker.Topic.TopicFriendlyName,
								new __Meta__<Media>(s), user.Id);
						};
					}
				});
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			Console.WriteLine($"Ended - BuildUsersFeed : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildUserFollowList(int limit = 3, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildUserUnfollowList");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				await _assignments.ParallelForEachAsync(async worker => {
					ContentSearcherHandler searcher = new ContentSearcherHandler(worker.Worker, worker.Worker.GetContext.Proxy);
					var user = worker.InstagramRequests.InstagramAccount;
					var fetchUsersMedia = await searcher.GetUserFollowingList(user.Username, limit);
					if (fetchUsersMedia != null)
					{
						await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersFollowingList, worker.Topic.TopicFriendlyName, user.Id, proxy: worker.Worker.GetContext.Proxy);
						foreach(var s in fetchUsersMedia.ToList().CutObject(cutBy))
						{
							await _heartbeatLogic.AddMetaData(MetaDataType.FetchUsersFollowingList, worker.Topic.TopicFriendlyName,
								new __Meta__<List<UserResponse<string>>>(s), user.Id);
						};
					}
				});
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			Console.WriteLine($"Ended - BuildUserUnfollowList : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildUsersFollowSuggestions(int limit = 1, int cutObjectBy = 1)
		{
			Console.WriteLine("Began - BuildUsersFollowSuggestions");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				await _assignments.ParallelForEachAsync(async worker => {
					ContentSearcherHandler searcher = new ContentSearcherHandler(worker.Worker, worker.InstagramRequests.ProxyUsing);
					var user = worker.InstagramRequests.InstagramAccount;
					searcher.ChangeUser(new APIClientContainer(_context, user.AccountId, user.Id));
					var fetchUsersMedia = await searcher.GetSuggestedPeopleToFollow(limit);
					searcher.ChangeUser(worker.Worker);
					if (fetchUsersMedia != null)
					{
						await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersFollowSuggestions, worker.Topic.TopicFriendlyName, user.Id, proxy: worker.Worker.GetContext.Proxy);
						foreach(var s in fetchUsersMedia.ToList().CutObject(cutObjectBy))
						{
							await _heartbeatLogic.AddMetaData(MetaDataType.FetchUsersFollowSuggestions, worker.Topic.TopicFriendlyName,
								new __Meta__<List<UserResponse<UserSuggestionDetails>>>(s), user.Id);
						};
					}
				});
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			Console.WriteLine($"Ended - BuildUsersFollowSuggestions : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
	}
}
