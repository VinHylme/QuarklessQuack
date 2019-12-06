using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Dasync.Collections;
using InstagramApiSharp.Classes.Models;
using Quarkless.HeartBeater.Interfaces.Models;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.ResponseModels;
using QuarklessContexts.Models.ServicesModels.HeartbeatModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessContexts.Models.Topics;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Logic.InstagramAccountLogic;
using QuarklessLogic.Logic.ProxyLogic;
using QuarklessLogic.ServicesLogic.ContentSearch;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Proxies;
using QuarklessLogic.ContentSearch.GoogleSearch;
using QuarklessLogic.ContentSearch.YandexSearch;
using QuarklessLogic.Logic.ResponseLogic;

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
			for(var j = start; j < limit + start; j++)
			{
				yield return obj.ElementAtOrDefault(j);
			}
		}
		public static List<List<TCut>> CutObject<TCut>(this List<TCut> @item, int cutAmount) where TCut: new()
		{
			var pos = 0;
			var objects = new List<List<TCut>>();
			for(var x = 0 ; x <= @item.Count(); x++)
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
			var pos = 0;
			var media_ = new List<Media>();
			for(var x = 0 ; x <= medias.Medias.Count; x++)
			{
				if (x % amount != 0 || x == 0) continue;
				media_.Add(new Media
				{
					Medias = medias.Medias.Between(pos,amount).ToList(),
					errors = medias.errors
				});
				pos = x;
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
		private readonly IResponseResolver _responseResolver;
		private readonly IGoogleSearchLogic _googleSearchLogic;
		private readonly IYandexImageSearch _yandexImageSearch;

		public MetadataBuilder(List<Assignments> assignments, IAPIClientContext aPIClient, 
			IHeartbeatLogic heartbeatLogic, IProxyLogic proxyLogic, 
			IResponseResolver responseResolver, 
			IGoogleSearchLogic googleSearchLogic, IYandexImageSearch yandexImageSearch)
		{
			_proxyLogic = proxyLogic;
			_assignments = assignments;
			_heartbeatLogic = heartbeatLogic;
			_context = aPIClient;
			_responseResolver = responseResolver;
			_googleSearchLogic = googleSearchLogic;
			_yandexImageSearch = yandexImageSearch;
		}

		private ContentSearcherHandler CreateSearch(IAPIClientContainer context, ProxyModel proxy) 
			=> new ContentSearcherHandler(context, _responseResolver, _googleSearchLogic, _yandexImageSearch, proxy);

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
				type = imageType == "any" ? null : imageType,
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
					var searcher = CreateSearch(worker.Worker, worker.Worker.GetContext.Proxy);

					var res = await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaForSpecificUserGoogle,worker.Topic.TopicFriendlyName,worker.InstagramRequests.InstagramAccount.Id);
					var by = new By { ActionType = 0, User = worker.InstagramRequests.InstagramAccount.Id };

					var meta_S = res as __Meta__<Media>[] ?? res.ToArray();
					if (!meta_S.Any() || meta_S.Where(x => x!=null).All(s=>!s.SeenBy.Any(sb=> sb.User == by.User && sb.ActionType==by.ActionType))){
						var topicTotal = new List<string>();

						var selectSubTopic = worker.Topic.SubTopics.TakeAny(1).SingleOrDefault();
						if (selectSubTopic == null)
							return;
						var randomtopic = selectSubTopic.RelatedTopics.Distinct().ToList();
						randomtopic.Add(selectSubTopic.TopicName);
						var searchQueryTopics = randomtopic.TakeAny(1).SingleOrDefault();

						//var subtopics = worker.Topic.SubTopics.Select(a=>a.TopicName);
						//var related = worker.Topic.SubTopics.Select(s=>s.RelatedTopics).SquashMe();
						//topicTotal.AddRange(subtopics);
						//topicTotal.AddRange(related);
						//var topicSelect = topicTotal.Distinct().TakeAny(topicAmount).ToList();

						var prf = worker.InstagramRequests.Profile;
						var colrSelect = prf.Theme.Colors.TakeAny(1).SingleOrDefault();
						var imtype = ((ImageType)prf.AdditionalConfigurations.ImageType).GetDescription();
						if (colrSelect != null)
						{
							var go = searcher.SearchViaGoogle(GoogleQueryBuilder(colrSelect.Name, searchQueryTopics,
								prf.AdditionalConfigurations.Sites,limit,exactSize:prf.AdditionalConfigurations.PostSize,imageType:imtype));
							if (go != null) { 
								if (go.StatusCode == ResponseCode.Success)
								{
									await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaForSpecificUserGoogle, worker.Topic.TopicFriendlyName, worker.InstagramRequests.InstagramAccount.Id);
								
									var cut = go.Result.CutObjects(cutBy).ToList();
									cut.ForEach(async x =>
									{
										await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaForSpecificUserGoogle, worker.Topic.TopicFriendlyName,
											new __Meta__<Media> (x), worker.InstagramRequests.InstagramAccount.Id);
									});
								}
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
				
				foreach(var worker in _assignments)
				{
					//if(worker.Worker.GetContext.Proxy == null)
					//{				
					//	var proxy = await _proxyLogic.RetrieveRandomProxy(connectionType: ConnectionType.Mobile, get:true, cookies:true,country:"UK");
					//	if (proxy != null)
					//	{
					//		worker.Worker.GetContext.Proxy = proxy;
					//	}
					//}
					worker.Worker.GetContext.Proxy = new ProxyModel
					{
						Address = "37.48.118.4",
						Port = 13010,
						NeedServerAuth = false
					};

					var searcher = CreateSearch(worker.Worker, worker.Worker.GetContext.Proxy);

					var topicTotal = new List<string>();

					var res = await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaForSepcificUserYandexQuery, worker.Topic.TopicFriendlyName, worker.InstagramRequests.InstagramAccount.Id);
					var by = new By { ActionType = 0, User = worker.InstagramRequests.InstagramAccount.Id };

					var meta_S = res as __Meta__<Media>[] ?? res.ToArray();
					if (!meta_S.Any() || meta_S.Where(x => x != null).All(s => !s.SeenBy.Any(sb => sb.User == by.User && sb.ActionType == by.ActionType)))
					{
						var selectSubTopic = worker.Topic.SubTopics.TakeAny(1).SingleOrDefault();
						if (selectSubTopic == null)
							return ;
						var randomtopic = selectSubTopic.RelatedTopics.Distinct().ToList();
						randomtopic.Add(selectSubTopic.TopicName);
						var searchQuery = randomtopic.TakeAny(1).SingleOrDefault();

						var prf = worker.InstagramRequests.Profile;
						var colrSelect = prf.Theme.Colors.TakeAny(1).SingleOrDefault();

						var convertedSize = prf.AdditionalConfigurations.PostSize?.Split(',');

						if (colrSelect != null)
						{
							var yandexSearchQuery =new YandexSearchQuery
							{
								OriginalTopic = worker.Topic.TopicFriendlyName,
								SearchQuery = searchQuery,
								Color = colrSelect.Name.GetValueFromDescription<ColorType>(),
								Format = FormatType.Any,
								Orientation = Orientation.Any,
								Size = SizeType.Large,
								Type = (ImageType) prf.AdditionalConfigurations.ImageType == ImageType.Any ? Enum.GetValues(typeof(ImageType))
									.Cast<ImageType>().Where(x=>x!= ImageType.Any).TakeAny(1).SingleOrDefault() : (ImageType) prf.AdditionalConfigurations.ImageType
							};

							var yan = searcher.SearchViaYandex(yandexSearchQuery, limit);
							if (yan != null)
							{
								switch (yan.StatusCode)
								{
									case ResponseCode.Success:
									{
										await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaForSepcificUserYandexQuery, worker.Topic.TopicFriendlyName, worker.InstagramRequests.InstagramAccount.Id);
										var cut = yan.Result.CutObjects(cutBy).ToList();
										cut.ForEach(async x =>
										{
											await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaForSepcificUserYandexQuery, worker.Topic.TopicFriendlyName,
												new __Meta__<Media>(x), worker.InstagramRequests.InstagramAccount.Id);
										});
										break;
									}
									case ResponseCode.Timeout:
										break;
									case ResponseCode.CaptchaRequired:
										break;
								}
							}
						}
					}
				};
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
					//if (worker.Worker.GetContext.Proxy == null)
					//{
					//	var proxy = await _proxyLogic.RetrieveRandomProxy(connectionType: ConnectionType.Mobile, get:true, cookies: true);
					//	if (proxy != null)
					//	{
					//		worker.Worker.GetContext.Proxy = proxy;
					//	}
					//}
					worker.Worker.GetContext.Proxy = new ProxyModel
					{
						Address = "37.48.118.4",
						Port = 13010,
						NeedServerAuth = false
					};

					var searcher = CreateSearch(worker.Worker, worker.Worker.GetContext.Proxy);

					var imalike = worker.InstagramRequests.Profile.Theme.ImagesLike;
					if (imalike == null) return;
					var res = await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaForSpecificUserYandex, worker.Topic.TopicFriendlyName, worker.InstagramRequests.InstagramAccount.Id);

					var by = new By { ActionType = 0, User = worker.InstagramRequests.InstagramAccount.Id };
					var meta_S = res as __Meta__<Media>[] ?? res.ToArray();
					if (!meta_S.Any() || meta_S.Where(x => x != null).All(s => !s.SeenBy.Any(sb => sb.User == by.User && sb.ActionType == by.ActionType)))
					{
						var filter = imalike.Where(s => s.TopicGroup.ToLower() == worker.Topic.TopicFriendlyName.ToLower());
						var groupImagesAlikes = filter as GroupImagesAlike[] ?? filter.ToArray();
						var yan = searcher.SearchViaYandexBySimilarImages(groupImagesAlikes.TakeAny(takeTopicAmount).ToList(),limit);

						if(yan == null 
						|| yan.StatusCode == ResponseCode.CaptchaRequired 
						|| yan.StatusCode == ResponseCode.InternalServerError 
						|| yan.StatusCode == ResponseCode.ReachedEndAndNull) 
						{
							yan = searcher.SearchYandexSimilarSafeMode(groupImagesAlikes.TakeAny(takeTopicAmount).ToList(), limit * 25);
							if(yan==null || yan.StatusCode == ResponseCode.CaptchaRequired)
								yan = searcher.SearchSimilarImagesViaGoogle(groupImagesAlikes.TakeAny(takeTopicAmount).ToList(),limit*25);
						}
						if (yan != null)
						{
							switch (yan.StatusCode)
							{
								case ResponseCode.Success:
								{
									await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaForSpecificUserYandex, worker.Topic.TopicFriendlyName, worker.InstagramRequests.InstagramAccount.Id);
									var cut = yan.Result.CutObjects(cutBy).ToList();
									cut.ForEach(async x =>
									{
										await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaForSpecificUserYandex, worker.Topic.TopicFriendlyName,
											new __Meta__<Media>(x), worker.InstagramRequests.InstagramAccount.Id);
									});
									break;
								}
								case ResponseCode.Timeout:
									//could try with a different proxy
									//await BuildYandexImages(limit,takeTopicAmount,cutBy);
									break;
								case ResponseCode.CaptchaRequired:
								{
									if (yan.Result?.Medias?.Count > 0)
									{
										await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaForSpecificUserYandex, worker.Topic.TopicFriendlyName, worker.InstagramRequests.InstagramAccount.Id);
										var cut = yan.Result.CutObjects(cutBy).ToList();
										cut.ForEach(async x =>
										{
											await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaForSpecificUserYandex, worker.Topic.TopicFriendlyName,
												new __Meta__<Media>(x), worker.InstagramRequests.InstagramAccount.Id);
										});
									}

									break;
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
		public async Task<IEnumerable<TopicCategories>> BuildTopicTypes()
		{
			try
			{
				var anyworker = _assignments.FirstOrDefault().Worker;

				var contentSearcher = CreateSearch(anyworker, anyworker.GetContext.Proxy);

				return await contentSearcher.GetBusinessCategories();
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
				return null;
			}
		}
		public async Task PopulateCorpusData(List<PopulateAssignments> assignmentses, int mediaLimit = 2)
		{
			Console.WriteLine("Began - PopulateCorpusData");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				await assignmentses.ParallelForEachAsync(async work =>
				{
					foreach (var topic in work.TopicsAssigned)
					{
						try
						{
							Console.WriteLine($"Started For Topic Of: {topic.FriendlyName}");
							var innerWatch = System.Diagnostics.Stopwatch.StartNew();

							var searcher = CreateSearch(work.Worker, work.Worker.GetContext.Proxy);

							var mediaByTopics =
								await searcher.SearchMediaDetailInstagram(new List<string> {topic.Searchable}, mediaLimit);
							var recentMedias =
								await searcher.SearchMediaDetailInstagram(new List<string> {topic.Searchable}, mediaLimit, true);
							if (recentMedias != null)
								mediaByTopics.Medias.AddRange(recentMedias.Medias);
							
							_heartbeatLogic.PopulateCaption(mediaByTopics, topic.FriendlyName);

							var filtered = mediaByTopics.Medias.Where(_ =>
								!_.IsCommentsDisabled && _.MediaFrom == MediaFrom.Instagram && !_.HasSeen && _.MediaId != null);
							var mostComments = filtered.OrderByDescending(_ => _.CommentCount).Take(5);
							foreach (var comment in mostComments)
							{
								var comments =
									await searcher.SearchInstagramMediaCommenters(
										comment.MediaId,
										2);
								if (comments != null)
								{
									_heartbeatLogic.PopulateComments(comments, topic.FriendlyName);
								}

								await Task.Delay(SecureRandom.Next(100, 800));
							}
							//var selectRandomMediaToFetchComments = mostComments.TakeAny(1).SingleOrDefault();
							//if (selectRandomMediaToFetchComments != null)
							//{
							//	var comments =
							//		await searcher.SearchInstagramMediaCommenters(
							//			selectRandomMediaToFetchComments.MediaId,
							//			2);
							//	if (comments != null)
							//	{
							//		_heartbeatLogic.PopulateComments(comments, topic.FriendlyName);
							//	}
							//}
							innerWatch.Stop();
							Console.WriteLine(
								$"Ended For Topic Of: {topic.FriendlyName}, Took {TimeSpan.FromMilliseconds(innerWatch.ElapsedMilliseconds).TotalSeconds}s");
						}
						catch (Exception e)
						{
							Console.WriteLine(e.Message + $" {topic.FriendlyName}");
						}

						#region before
						/*
						if (mediaByTopics != null) { 
							await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByTopic, topic, proxy:work.Worker.GetContext.Proxy);
							var lmeta = mediaByTopics.CutObjects(1).ToList();

							lmeta.ToList().ForEach(async z => {
								z.Medias = z.Medias.Where(t => !t.HasLikedBefore && !t.IsCommentsDisabled && !t.HasSeen).ToList();
								await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByTopic, topic, 
									new __Meta__<Media>(z));
							});
						}
						if (recentMedias == null) continue;
						{
							await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByTopicRecent, topic, proxy: work.Worker.GetContext.Proxy);
							var lmeta = recentMedias.CutObjects(1).ToList();

							lmeta.ToList().ForEach(async z => {
								z.Medias = z.Medias.Where(t => !t.HasLikedBefore && !t.IsCommentsDisabled && !t.HasSeen).ToList();
								await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByTopicRecent, topic,
									new __Meta__<Media>(z));
							});
						}
						*/
						#endregion
					}
				});
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			watch.Stop();
			Console.WriteLine($"Ended - PopulateCorpusData : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildBase(int limit = 2, int cutBy = 1, int takeTopicAmount = 1)
		{
			Console.WriteLine("Began - BuildBase");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try {
				await _assignments.ParallelForEachAsync(async worker =>
				{
					var searcher = CreateSearch(worker.Worker, worker.Worker.GetContext.Proxy);

					var topicTotal = new List<string>();

					var subtopics = worker.Topic.SubTopics.Select(a => a.TopicName);
					var related = worker.Topic.SubTopics.Select(s => s.RelatedTopics).SquashMe();

					topicTotal.AddRange(subtopics);
					topicTotal.AddRange(related);

					var topicSelect = topicTotal.Distinct().TakeAny(takeTopicAmount);

					var mediaByTopics = await searcher.SearchMediaDetailInstagram(topicSelect, limit);
					var recentMedias = await searcher.SearchMediaDetailInstagram(topicSelect, limit, true);

					if (mediaByTopics != null) { 
						await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByTopic, worker.Topic.TopicFriendlyName);
						var meta = mediaByTopics.CutObjects(cutBy);

						meta.ToList().ForEach(async z => {
							z.Medias = z.Medias.Where(t => !t.HasLikedBefore && !t.HasSeen 
							&& t.User.Username != worker.InstagramRequests.InstagramAccount.Username).ToList();

							await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByTopic, worker.Topic.TopicFriendlyName, 
								new __Meta__<Media>(z));
						});
					}
					if(recentMedias != null)
					{
						await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByTopicRecent, worker.Topic.TopicFriendlyName);
						var meta = recentMedias.CutObjects(cutBy);

						meta.ToList().ForEach(async z => {
							z.Medias = z.Medias.Where(t => !t.HasLikedBefore && !t.HasSeen && t.User.Username != worker.InstagramRequests.InstagramAccount.Username).ToList();
							
							await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByTopicRecent, worker.Topic.TopicFriendlyName,
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
		public async Task BuildUsersTargetListMedia(int limit = 1, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildUsersTargetListMedia");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				await _assignments.ParallelForEachAsync(async worker => {

					var searcher = CreateSearch(worker.Worker, worker.Worker.GetContext.Proxy);

					var user = worker.InstagramRequests.InstagramAccount;
					var userTargetList = worker.InstagramRequests.Profile.UserTargetList;
					if(userTargetList!=null && userTargetList.Count > 0)
					{
						foreach(var theUser in userTargetList)
						{
							var fetchUsersMedia = await searcher.SearchUsersMediaDetailInstagram(theUser, limit);
							if (fetchUsersMedia == null) continue;
							await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByUserTargetList, worker.Topic.TopicFriendlyName, user.Id);
							foreach(var s in fetchUsersMedia.CutObjects(cutBy))
							{
								s.Medias = s.Medias.Where(t => 
									!t.HasLikedBefore && 
									!t.HasSeen).ToList();
								await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByUserTargetList, worker.Topic.TopicFriendlyName,
									new __Meta__<Media>(s), user.Id);
							};

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

					var searcher = CreateSearch(worker.Worker, worker.Worker.GetContext.Proxy);

					var user = worker.InstagramRequests.InstagramAccount;
					var profileLocationTargetList = worker.InstagramRequests.Profile.LocationTargetList;
					if (profileLocationTargetList != null && profileLocationTargetList.Count > 0)
					{
						foreach (var targetLocation in profileLocationTargetList)
						{
							var fetchUsersMedia = await searcher.SearchTopLocationMediaDetailInstagram(targetLocation, limit);
							await Task.Delay(TimeSpan.FromSeconds(1));
							var recentUserMedia =
								await searcher.SearchRecentLocationMediaDetailInstagram(targetLocation, limit);
							if (recentUserMedia != null && recentUserMedia.Medias.Count > 0)
							{
								fetchUsersMedia.Medias.AddRange(recentUserMedia.Medias);
							}
							if (fetchUsersMedia == null) continue;
							await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByUserLocationTargetList, worker.Topic.TopicFriendlyName, user.Id);
							foreach(var s in fetchUsersMedia.CutObjects(cutBy))
							{
								s.Medias = s.Medias.Where(t => 
									!t.HasLikedBefore && 
									!t.HasSeen && 
									t.User.Username != worker.InstagramRequests.InstagramAccount.Username).ToList();
								await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByUserLocationTargetList, worker.Topic.TopicFriendlyName,
									new __Meta__<Media>(s), user.Id);
							};
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

					var searcher = CreateSearch(worker.Worker, worker.Worker.GetContext.Proxy);

					var user = worker.InstagramRequests.InstagramAccount;
					var fetchUsersMedia = await searcher.SearchUsersMediaDetailInstagram(user.Username,limit);
					if (fetchUsersMedia != null)
					{
						await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUserOwnProfile, worker.Topic.TopicFriendlyName, user.Id);
						foreach(var s in fetchUsersMedia.CutObjects(cutBy))
						{
							await _heartbeatLogic.AddMetaData(MetaDataType.FetchUserOwnProfile, worker.Topic.TopicFriendlyName,
								new __Meta__<Media>(s),user.Id);
						};
						var userFirst = fetchUsersMedia.Medias.FirstOrDefault();
						if (userFirst != null) { 
							var details = await searcher.SearchInstagramFullUserDetail(userFirst.User.UserId);
							if (details.UserDetail.Pk != 0)
							{
								await _logic.PartialUpdateInstagramAccount(user.AccountId, user.Id,
									new QuarklessContexts.Models.InstagramAccounts.InstagramAccountModel
									{
										UserId = details.UserDetail.Pk,
										FollowersCount = details.UserDetail.FollowerCount,
										FollowingCount = details.UserDetail.FollowingCount,
										TotalPostsCount = details.UserDetail.MediaCount,
										Email = details.UserDetail.PublicEmail,
										ProfilePicture =
											details.UserDetail.ProfilePicture ?? details.UserDetail.ProfilePicUrl,
										FullName = details.UserDetail.FullName,
										UserBiography = new QuarklessContexts.Models.InstagramAccounts.Biography
										{
											Text = details.UserDetail.BiographyWithEntities.Text,
											Hashtags = details.UserDetail.BiographyWithEntities.Entities
												.Select(_ => _.Hashtag.Name).ToList()
										},
										IsBusiness = details.UserDetail.IsBusiness,
										Location = new Location
										{
											Address = details.UserDetail.AddressStreet,
											City = details.UserDetail.CityName,
											Coordinates = new Coordinates
											{
												Latitude = details.UserDetail.Latitude,
												Longitude = details.UserDetail.Longitude
											},
											PostCode = details.UserDetail.ZipCode
										},
										PhoneNumber = details.UserDetail.PublicPhoneCountryCode +
										              (!string.IsNullOrEmpty(details.UserDetail.PublicPhoneNumber)
											              ? details.UserDetail.PublicPhoneNumber
											              : details.UserDetail.ContactPhoneNumber)
									});
							}
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

					var searcher = CreateSearch(worker.Worker, worker.Worker.GetContext.Proxy);

					var user = worker.InstagramRequests.InstagramAccount;
					searcher.ChangeUser(new APIClientContainer(_context,user.AccountId,user.Id));
					var fetchUsersMedia = await searcher.SearchUserFeedMediaDetailInstagram(limit:limit);
					searcher.ChangeUser(worker.Worker);
					if (fetchUsersMedia != null)
					{
						await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersFeed, worker.Topic.TopicFriendlyName, user.Id);
						foreach(var s in fetchUsersMedia.CutObjects(cutBy))
						{
							s.Medias = s.Medias.Where(_=>
								!_.HasLikedBefore 
								&& !_.HasSeen
								&& _.User.Username!=user.Username).ToList();
							
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

					var searcher = CreateSearch(worker.Worker,  worker.Worker.GetContext.Proxy);

					var user = worker.InstagramRequests.InstagramAccount;
					var fetchUsersMedia = await searcher.GetUserFollowingList(user.Username, limit);
					if (fetchUsersMedia != null)
					{
						await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersFollowingList, worker.Topic.TopicFriendlyName, user.Id);
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
		public async Task BuildUserFollowerList(int limit = 4, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildUserFollowerList");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				await _assignments.ParallelForEachAsync(async worker => {

					var searcher = CreateSearch(worker.Worker, worker.Worker.GetContext.Proxy);

					var user = worker.InstagramRequests.InstagramAccount;
					var fetchUsersMedia = await searcher.GetUsersFollowersList(user.Username, limit);
					if (fetchUsersMedia != null)
					{
						await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersFollowerList, worker.Topic.TopicFriendlyName, user.Id);
						foreach(var s in fetchUsersMedia.ToList().CutObject(cutBy))
						{
							await _heartbeatLogic.AddMetaData(MetaDataType.FetchUsersFollowerList, worker.Topic.TopicFriendlyName,
								new __Meta__<List<UserResponse<string>>>(s), user.Id);
						};
					}
				});
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			Console.WriteLine($"Ended - BuildUserFollowerList : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildUsersFollowSuggestions(int limit = 1, int cutObjectBy = 1)
		{
			Console.WriteLine("Began - BuildUsersFollowSuggestions");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				await _assignments.ParallelForEachAsync(async worker => {

					var searcher = CreateSearch(worker.Worker, worker.InstagramRequests.ProxyUsing);

					var user = worker.InstagramRequests.InstagramAccount;
					searcher.ChangeUser(new APIClientContainer(_context, user.AccountId, user.Id));
					var fetchUsersMedia = await searcher.GetSuggestedPeopleToFollow(limit);
					searcher.ChangeUser(worker.Worker);
					if (fetchUsersMedia != null)
					{
						await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersFollowSuggestions, worker.Topic.TopicFriendlyName, user.Id);
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
		public async Task BuildUsersInbox(int limit = 1)
		{
			Console.WriteLine("Began - BuildUsersInbox");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				await _assignments.ParallelForEachAsync(async worker => {

					var searcher = CreateSearch(worker.Worker, worker.InstagramRequests.ProxyUsing);
					
					var user = worker.InstagramRequests.InstagramAccount;
					
					searcher.ChangeUser(new APIClientContainer(_context, user.AccountId, user.Id));
					var fetchUserInbox = await searcher.SearchUserInbox(limit);
					searcher.ChangeUser(worker.Worker);
					
					if (fetchUserInbox != null)
					{
						await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUserDirectInbox, worker.Topic.TopicFriendlyName, user.Id);

						await _heartbeatLogic.AddMetaData(MetaDataType.FetchUserDirectInbox,
							worker.Topic.TopicFriendlyName, new __Meta__<InstaDirectInboxContainer>(fetchUserInbox),
							user.Id);
					}
				});
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			Console.WriteLine($"Ended - BuildUsersInbox : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildUsersRecentComments(int howManyMedias = 10, int limit = 1)
		{
			Console.WriteLine("Began - BuildUsersRecentComments");
			var watch = System.Diagnostics.Stopwatch.StartNew();

			try
			{
				await _assignments.ParallelForEachAsync(async worker =>
				{
					var searcher = CreateSearch(worker.Worker, worker.Worker.GetContext.Proxy);

					var user = worker.InstagramRequests.InstagramAccount;
					await _heartbeatLogic.RefreshMetaData(MetaDataType.UsersRecentComments, worker.Topic.TopicFriendlyName, user.Id);

					var mediasForUser = (await _heartbeatLogic.GetMetaData<Media>
						(MetaDataType.FetchUserOwnProfile, worker.Topic.TopicFriendlyName, user.Id))
						.Select(x => x.ObjectItem.Medias)
						.SquashMe()
						.OrderByDescending(x=>x.TakenAt)
						.Take(howManyMedias);

					foreach (var media in mediasForUser)
					{
						var comments = await searcher.SearchInstagramMediaCommenters(media.MediaId, limit);

						await _heartbeatLogic.AddMetaData(MetaDataType.UsersRecentComments,
							worker.Topic.TopicFriendlyName,
							new __Meta__<List<UserResponse<InstaComment>>>(comments), user.Id);
					}
				});
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			Console.WriteLine($"Ended - BuildUsersRecentComments : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}

		public async Task BuildUserFromLikers(int cutBy = 1, double sleepTime = 0.05,
			int takeMediaAmount = 7, int takeUserMediaAmount = 100)
		{
			Console.WriteLine("Began - BuildUserFromLikers");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				await _assignments.ParallelForEachAsync(async worker =>
				{

					var searcher = CreateSearch(worker.Worker, worker.Worker.GetContext.Proxy);

					var results = (await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopic, worker.Topic.TopicFriendlyName)).ToList();
					var recentResults = await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopicRecent, worker.Topic.TopicFriendlyName);
					var meta_S = recentResults as __Meta__<Media>[] ?? recentResults.ToArray();
					if (meta_S.Any())
						results.AddRange(meta_S);

					await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersViaPostLiked, worker.Topic.TopicFriendlyName);
					foreach (var v in results.TakeAny(takeMediaAmount))
					{
						if (v == null) continue;
						foreach (var _ in v.ObjectItem.Medias.Where(t => t.MediaFrom == MediaFrom.Instagram))
						{
							var suggested = await searcher.SearchInstagramMediaLikers(_.MediaId);
							if (suggested == null) return;
							if (suggested.Count < 1) return;
							var separateSuggested = suggested.TakeAny(takeUserMediaAmount).ToList().CutObject(cutBy);
							separateSuggested.ForEach(async s =>
							{
								var filtered = s.Where(x => x.Username != worker.InstagramRequests.InstagramAccount.Username).ToList();
								await _heartbeatLogic.AddMetaData(MetaDataType.FetchUsersViaPostLiked, worker.Topic.TopicFriendlyName,
									new __Meta__<List<UserResponse<string>>>(filtered));
							});
							await Task.Delay(TimeSpan.FromSeconds(sleepTime));
						}
					};
				});
			}
			catch (Exception ee)
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
			try
			{
				await _assignments.ParallelForEachAsync(async worker =>
				{

					var searcher = CreateSearch(worker.Worker, worker.Worker.GetContext.Proxy);

					var results = (await _heartbeatLogic.GetMetaData<List<UserResponse<string>>>
					(MetaDataType.FetchUsersViaPostLiked, worker.Topic.TopicFriendlyName)).ToList();

					await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByLikers, worker.Topic.TopicFriendlyName);
					foreach (var _ in results.TakeAny(takeMediaAmount))
					{
						if (_ == null) continue;
						foreach (var d in _.ObjectItem)
						{
							var suggested = await searcher.SearchUsersMediaDetailInstagram(d.Username, limit);
							if (suggested != null)
							{
								var sugVCut = suggested.CutObjects(cutBy).ToList();
								foreach (var z in sugVCut?.TakeAny(takeUserMediaAmount))
								{
									z.Medias = z.Medias.Where(t =>
									!t.HasLikedBefore
									&& !t.HasSeen
									&& t.User.Username != worker.InstagramRequests.InstagramAccount.Username).ToList();
									await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByLikers, worker.Topic.TopicFriendlyName, new __Meta__<Media>(z));
								};
							}
							await Task.Delay(TimeSpan.FromSeconds(sleepTime));
						};
					};
				});
			}
			catch (Exception ee)
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

					var searcher = CreateSearch(worker.Worker, worker.Worker.GetContext.Proxy);

					var results = (await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopic, worker.Topic.TopicFriendlyName)).ToList();
					var recentResults = await _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopicRecent, worker.Topic.TopicFriendlyName);
					var meta_S = recentResults as __Meta__<Media>[] ?? recentResults.ToArray();
					if (meta_S.Any())
						results.AddRange(meta_S);
					await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersViaPostCommented, worker.Topic.TopicFriendlyName);
					foreach (var v in results.TakeAny(takeMediaAmount))
					{
						if (v == null) continue;
						foreach (var _ in v.ObjectItem.Medias)
						{
							if (!_.IsCommentsDisabled && int.TryParse(_.CommentCount, out var count) && count > 0 && _.MediaFrom == MediaFrom.Instagram && _.MediaId != null)
							{
								var suggested = await searcher.SearchInstagramMediaCommenters(_.MediaId, limit);
								if (suggested == null) return;
								if (suggested.Count < 1) return;
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
								var separatedCommenter = suggested.TakeAny(takeUserMediaAmount).ToList().CutObject(cutBy);
								foreach (var s in separatedCommenter)
								{
									var filtered = s.Where(x => x.Username != worker.InstagramRequests.InstagramAccount.Username).ToList();
									await _heartbeatLogic.AddMetaData(MetaDataType.FetchUsersViaPostCommented,
										worker.Topic.TopicFriendlyName,
										new __Meta__<List<UserResponse<InstaComment>>>(filtered));
								}
							}
							await Task.Delay(TimeSpan.FromSeconds(sleepTime));
						};
					};
				});
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			watch.Stop();
			Console.WriteLine($"Ended - BuildUsersFromCommenters : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public async Task BuildCommentsFromSpecifiedSource(MetaDataType extractFrom, MetaDataType saveTo, bool includeUser = false, int limit = 1, int cutBy = 1, double secondSleep = 0.05,
			int takeMediaAmount = 10, int takeuserAmount = 100)
		{
			Console.WriteLine("Began - BuildCommentsFromSpecifiedSource " + saveTo.ToString());
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				await _assignments.ParallelForEachAsync(async worker =>
				{
					var searcher = CreateSearch(worker.Worker, worker.Worker.GetContext.Proxy);

					var res = await _heartbeatLogic.GetMetaData<Media>(extractFrom, worker.Topic.TopicFriendlyName, includeUser ? worker.InstagramRequests.InstagramAccount.Id : null);
					if (res != null)
					{
						await _heartbeatLogic.RefreshMetaData(saveTo, worker.Topic.TopicFriendlyName);
						foreach (var _ in res.TakeAny(takeMediaAmount))
						{
							if (_ == null) continue;
							foreach (var __ in _.ObjectItem.Medias)
							{
								if (!__.IsCommentsDisabled && int.TryParse(__.CommentCount, out var count) && count > 0 && __.MediaFrom == MediaFrom.Instagram && __.MediaId != null && !__.HasSeen)
								{
									var suggested = await searcher.SearchInstagramMediaCommenters(__.MediaId, limit);

									if (suggested != null)
									{
										var calcutta = suggested.TakeAny(takeuserAmount).ToList().CutObject(cutBy);
										foreach (var filtered in calcutta.Select(s => s.Where(x => x.Username != worker.InstagramRequests.InstagramAccount.Username).ToList()))
										{
											await _heartbeatLogic.AddMetaData(saveTo, worker.Topic.TopicFriendlyName,
												new __Meta__<List<UserResponse<InstaComment>>>(filtered),
												includeUser ? worker.InstagramRequests.InstagramAccount.Id : null);
										}
									}
								}
								await Task.Delay(TimeSpan.FromSeconds(secondSleep));
							};
						};
					}
				});
			}
			catch (Exception ee)
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
					var searcher = CreateSearch(worker.Worker, worker.Worker.GetContext.Proxy);

					var results = (await _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>
					(MetaDataType.FetchUsersViaPostCommented, worker.Topic.TopicFriendlyName)).ToList();

					await _heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByCommenters, worker.Topic.TopicFriendlyName);
					foreach (var _ in results.TakeAny(takeMediaAmount))
					{
						if (_ == null) continue;
						foreach (var commenter in _.ObjectItem)
						{
							var suggested = await searcher.SearchUsersMediaDetailInstagram(commenter.Username, limit);
							if (suggested != null)
							{
								var sugVCut = suggested.CutObjects(cutBy).ToList();
								foreach (var z in sugVCut.TakeAny(takeUserMediaAmount))
								{
									z.Medias = z.Medias.Where(t =>
										!t.HasLikedBefore &&
										!t.HasSeen).ToList();
									var comments = new __Meta__<Media>(z);
									await _heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByCommenters, worker.Topic.TopicFriendlyName, comments);
								};
							}
							await Task.Delay(TimeSpan.FromSeconds(secondsSleep));
						};
					};
				});
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			watch.Stop();
			Console.WriteLine($"Ended - BuildMediaFromUsersCommenters : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
	}
}
