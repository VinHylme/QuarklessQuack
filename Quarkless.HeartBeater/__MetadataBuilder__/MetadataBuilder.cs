using InstagramApiSharp.Classes.Models;
using Quarkless.HeartBeater.__Init__;
using Quarkless.Services.Extensions;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.ServicesModels.HeartbeatModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.RestSharpClient;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Quarkless.HeartBeater.__MetadataBuilder__
{
	public enum FetchType{
		Media,
		User,
		Comments
	}
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
		private readonly IRestSharpClientManager _restSharpClient;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly IAPIClientContext _context;
		public MetadataBuilder(List<Assignments> assignments, IRestSharpClientManager restSharpClient, IAPIClientContext aPIClient, IHeartbeatLogic heartbeatLogic)
		{
			_assignments = assignments;
			_restSharpClient = restSharpClient;
			_heartbeatLogic = heartbeatLogic;
			_context = aPIClient;
		}
		private SearchImageModel GoogleQueryBuilder(string color, List<string> topics, List<string> sites, int limit = 10,
		string imageType = null, string exactSize = null, string similarImage = null)
		{
			var query = new SearchImageModel
			{
				color = color,
				prefix_keywords = string.Join(", ", sites),
				keywords = string.Join(", ", topics),
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
		public void BuildGoogleImages(int limit = 15, int topicAmount = 1, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildGoogleImages");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				Parallel.ForEach(_assignments,worker =>
				{
					ContentSearch.ContentSearcher searcher = new ContentSearch.ContentSearcher(_restSharpClient, worker.Worker);
					var res = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaForSpecificUserGoogle,worker.Topic.TopicFriendlyName,worker.InstagramRequests.InstagramAccount.Id).GetAwaiter().GetResult();
					if(res==null || res.Count() <= 0 || res.Where(x => x.HasValue).All(s=>s.Value.SeenBy.Contains(new By{ ActionType = 0, User = worker.InstagramRequests.InstagramAccount.Id }))){
						List<string> topicTotal = new List<string>();
						var subtopics = worker.Topic.SubTopics.Select(a=>a.TopicName);
						var related = worker.Topic.SubTopics.Select(s=>s.RelatedTopics).SquashMe();
						topicTotal.AddRange(subtopics);
						topicTotal.AddRange(related);
						var topicSelect = topicTotal.Distinct().TakeAny(topicAmount).ToList();
						topicSelect.Add(worker.Topic.TopicFriendlyName);		

						var prf = worker.InstagramRequests.Profile;
						var colrSelect = prf.Theme.Colors.ElementAt(SecureRandom.Next(prf.Theme.Colors.Count));
						var imtype = ((ImageType)prf.AdditionalConfigurations.ImageType).GetDescription();
						var go = searcher.SearchViaGoogle(GoogleQueryBuilder(colrSelect.Name, topicSelect,
							prf.AdditionalConfigurations.Sites,limit,exactSize:prf.AdditionalConfigurations.PostSize,imageType:imtype));

						if (go != null)
						{
							_heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaForSpecificUserGoogle, worker.Topic.TopicFriendlyName, worker.InstagramRequests.InstagramAccount.Id);
							var cut = go.CutObjects(cutBy).ToList();
							cut.ForEach(x =>
							{
								_heartbeatLogic.AddMetaData(MetaDataType.FetchMediaForSpecificUserGoogle, worker.Topic.TopicFriendlyName,
									new __Meta__<Media> (x), worker.InstagramRequests.InstagramAccount.Id);
							});
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
		public void BuildYandexImagesQuery(int limit = 40, int topicAmount = 1, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildYandexImagesQuery");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				Parallel.ForEach(_assignments, worker =>
				{
					ContentSearch.ContentSearcher searcher = new ContentSearch.ContentSearcher(_restSharpClient, worker.Worker);
					List<string> topicTotal = new List<string>();
					var res = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaForSepcificUserYandexQuery, worker.Topic.TopicFriendlyName, worker.InstagramRequests.InstagramAccount.Id).GetAwaiter().GetResult();
					if (res == null || res.Count() <= 0 || res.Where(x=>x.HasValue).All(s => s.Value.SeenBy.Contains(new By { ActionType = 0, User = worker.InstagramRequests.InstagramAccount.Id })))
					{
						var subtopics = worker.Topic.SubTopics.Select(a => a.TopicName);

						var prf = worker.InstagramRequests.Profile;
						var colrSelect = prf.Theme.Colors.ElementAt(SecureRandom.Next(prf.Theme.Colors.Count));

						var convertedSize = prf.AdditionalConfigurations.PostSize!=null ? prf.AdditionalConfigurations.PostSize.Split(','):null;
						YandexSearchQuery yandexSearchQuery =new YandexSearchQuery
						{
							SearchQuery = worker.Topic.TopicFriendlyName + " " + subtopics.ElementAt(SecureRandom.Next(subtopics.Count()-1)) + " " + prf.AdditionalConfigurations.Sites.ElementAt(SecureRandom.Next(prf.AdditionalConfigurations.Sites.Count - 1)),
							Color = colrSelect.Name.GetValueFromDescription<ColorType>(),
							Format = FormatType.Any,
							Orientation = Orientation.Any,
							Size = SizeType.Large,
							Type = (ImageType) prf.AdditionalConfigurations.ImageType
						};

						var yan = searcher.SearchViaYandex(yandexSearchQuery,limit);

						if (yan != null)
						{
							_heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaForSepcificUserYandexQuery, worker.Topic.TopicFriendlyName, worker.InstagramRequests.InstagramAccount.Id);
							var cut = yan.CutObjects(cutBy).ToList();
							cut.ForEach(x =>
							{
								_heartbeatLogic.AddMetaData(MetaDataType.FetchMediaForSepcificUserYandexQuery, worker.Topic.TopicFriendlyName,
									new __Meta__<Media>(x), worker.InstagramRequests.InstagramAccount.Id);
							});
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

		public void BuildYandexImages(int limit = 60, int takeTopicAmount = 3, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildYandexImages");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{	
				_assignments.ForEach(worker =>
				{
					ContentSearch.ContentSearcher searcher = new ContentSearch.ContentSearcher(_restSharpClient, worker.Worker);
					var imalike = worker.InstagramRequests.Profile.Theme.ImagesLike;
					var res = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaForSpecificUserYandex, worker.Topic.TopicFriendlyName, worker.InstagramRequests.InstagramAccount.Id).GetAwaiter().GetResult();
					if (res == null || res.Count() <= 0 || res.Where(x => x.HasValue).All(s => s.Value.SeenBy.Contains(new By { ActionType = 0, User = worker.InstagramRequests.InstagramAccount.Id })))
					{
						var filter = imalike.Where(s => s.TopicGroup == worker.Topic.TopicFriendlyName);
						var yan = searcher.SearchViaYandexBySimilarImages(filter.TakeAny(takeTopicAmount).ToList(),limit);
						if(yan == null) 
						{
							yan = searcher.SearchSimilarImagesViaGoogle(filter.TakeAny(takeTopicAmount).ToList(),limit);
						}
						if (yan != null) { 
							_heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaForSpecificUserYandex, worker.Topic.TopicFriendlyName, worker.InstagramRequests.InstagramAccount.Id);
							var cut = yan.CutObjects(cutBy).ToList();
							cut.ForEach(x =>
							{
								_heartbeatLogic.AddMetaData(MetaDataType.FetchMediaForSpecificUserYandex, worker.Topic.TopicFriendlyName,
									new __Meta__<Media>(x), worker.InstagramRequests.InstagramAccount.Id);
							});
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
		public void BuildBase(int limit =2, int cutBy = 1, int takeTopicAmount = 1)
		{
			Console.WriteLine("Began - BuildBase");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try { 
				Parallel.ForEach(_assignments, worker =>
				{
					ContentSearch.ContentSearcher searcher = new ContentSearch.ContentSearcher(_restSharpClient,worker.Worker);
					List<string> topicTotal = new List<string>();
					var subtopics = worker.Topic.SubTopics.Select(a => a.TopicName);
					var related = worker.Topic.SubTopics.Select(s => s.RelatedTopics).SquashMe();
					topicTotal.AddRange(subtopics);
					topicTotal.AddRange(related);
					var topicSelect = topicTotal.Distinct().TakeAny(takeTopicAmount).ToList();
					topicSelect.Add(worker.Topic.TopicFriendlyName);

					var mediaByTopics = searcher.SearchMediaDetailInstagram(topicSelect, limit).GetAwaiter().GetResult();
					if (mediaByTopics != null) { 
						_heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByTopic,worker.Topic.TopicFriendlyName).GetAwaiter().GetResult();
						var lmeta = mediaByTopics.CutObjects(cutBy).ToList();
						lmeta.ToList().ForEach(z => {
							z.Medias = z.Medias.Where(t => !t.HasLikedBefore && !t.IsCommentsDisabled && t.User.Username != worker.InstagramRequests.InstagramAccount.Username).ToList();
							_heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByTopic, worker.Topic.TopicFriendlyName, 
								new __Meta__<Media>(z)).GetAwaiter().GetResult();
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

		public void BuildUserFromLikers(int cutBy = 1, double sleepTime = 0.05,
			int takeMediaAmount = 7, int takeUserMediaAmount = 10)
		{
			Console.WriteLine("Began - BuildUserFromLikers");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try { 
				Parallel.ForEach(_assignments, worker =>
				{
					ContentSearch.ContentSearcher searcher = new ContentSearch.ContentSearcher(_restSharpClient, worker.Worker);
					var results = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopic, worker.Topic.TopicFriendlyName)
					.GetAwaiter().GetResult();

					if (results != null)
					{
						_heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersViaPostLiked, worker.Topic.TopicFriendlyName).GetAwaiter().GetResult();
						results.TakeAny(takeMediaAmount).ToList().ForEach(v =>
						{
							if (v.HasValue) { 
								v.Value.ObjectItem.Medias.Where(t=>t.MediaFrom == MediaFrom.Instagram).ToList().ForEach(_ =>
								{
									var suggested = searcher.SearchInstagramMediaLikers(_.MediaId).GetAwaiter().GetResult();
									if (suggested == null && suggested.Count < 1) return;

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

									if (suggested?.TakeAny(takeUserMediaAmount)?.ToList() != null) { 
										var totalcut = suggested.CutObject(cutBy);
										totalcut.ForEach(s =>
										{
											var filtered = s.Where(x => x.Username != worker.InstagramRequests.InstagramAccount.Username).ToList();
											_heartbeatLogic.AddMetaData(MetaDataType.FetchUsersViaPostLiked, worker.Topic.TopicFriendlyName, 
												new __Meta__<List<UserResponse<string>>>(s)).GetAwaiter().GetResult();
										});
									}
									Task.Delay(TimeSpan.FromSeconds(sleepTime));
								});
							}
						});
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
		public void BuildMediaFromUsersLikers(int limit = 1, int cutBy = 1, double sleepTime = 0.05,
			int takeMediaAmount = 7, int takeUserMediaAmount = 10)
		{
			Console.WriteLine("Began - BuildMediaFromUsersLikers");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try { 
				Parallel.ForEach(_assignments, worker =>
				{
					ContentSearch.ContentSearcher searcher = new ContentSearch.ContentSearcher(_restSharpClient, worker.Worker);
					var results = _heartbeatLogic.GetMetaData<List<UserResponse<string>>>
					(MetaDataType.FetchUsersViaPostLiked, worker.Topic.TopicFriendlyName).GetAwaiter().GetResult().ToList();
					if (results != null)
					{
						_heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByLikers, worker.Topic.TopicFriendlyName).GetAwaiter().GetResult();
						results.TakeAny(takeMediaAmount).ToList().ForEach(_ =>{
							if (_.HasValue) { 
								_.Value.ObjectItem.ForEach(d =>
								{
									var suggested = searcher.SearchUsersMediaDetailInstagram(d.Username, limit).GetAwaiter().GetResult();
									if (suggested != null) { 
										var sugVCut = suggested.CutObjects(cutBy).ToList();
										sugVCut?.TakeAny(takeUserMediaAmount)?.ToList().ForEach(z =>
										{
											z.Medias = z.Medias.Where(t => !t.HasLikedBefore && !t.IsCommentsDisabled && t.User.Username != worker.InstagramRequests.InstagramAccount.Username).ToList();
											_heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByLikers, worker.Topic.TopicFriendlyName,
											new __Meta__<Media>(z)).GetAwaiter().GetResult();
										});
									}
									Task.Delay(TimeSpan.FromSeconds(sleepTime));
								});
							}
						});
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
		public void BuildUsersFromCommenters(int limit = 1, int cutBy = 1, double sleepTime = 0.05, 
			int takeMediaAmount = 7, int takeUserMediaAmount = 10)
		{
			Console.WriteLine("Began - BuildUsersFromCommenters");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				Parallel.ForEach(_assignments, worker =>
				{
					ContentSearch.ContentSearcher searcher = new ContentSearch.ContentSearcher(_restSharpClient, worker.Worker);
					var results = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopic, worker.Topic.TopicFriendlyName).GetAwaiter().GetResult();
					if (results != null)
					{
						_heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersViaPostCommented, worker.Topic.TopicFriendlyName).GetAwaiter().GetResult();
						results.TakeAny(takeMediaAmount).ToList().ForEach(v =>
						{
							if (v.HasValue)
							{
								v.Value.ObjectItem.Medias.ForEach(_ =>
								{
									if (!_.IsCommentsDisabled && _.MediaFrom == MediaFrom.Instagram && _.MediaId!=null) { 
										var suggested = searcher.SearchInstagramMediaCommenters(_.MediaId,limit).GetAwaiter().GetResult();
										if(suggested == null && suggested.Count < 1) return;

										//suggested = suggested.Select(check =>
										//{
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

										if (suggested.TakeAny(takeUserMediaAmount)?.ToList() != null)
										{
											var totalcut = suggested.CutObject(cutBy);
											foreach(var s in totalcut)
											{
												var filtered = s.Where(x => x.Username != worker.InstagramRequests.InstagramAccount.Username).ToList();
												_heartbeatLogic.AddMetaData(MetaDataType.FetchUsersViaPostCommented, worker.Topic.TopicFriendlyName,
												new __Meta__<List<UserResponse<InstaComment>>>(filtered)).GetAwaiter().GetResult();
											}
										}
									}
									Task.Delay(TimeSpan.FromSeconds(sleepTime));
								});
							}
						});
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
		public void BuildMediaFromUsersCommenters(int limit = 1, int cutBy = 1, double secondsSleep = 0.05, 
			int takeMediaAmount = 7, int takeUserMediaAmount = 10)
		{
			Console.WriteLine("Began - BuildMediaFromUsersCommenters");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				Parallel.ForEach(_assignments, worker =>
				{
					ContentSearch.ContentSearcher searcher = new ContentSearch.ContentSearcher(_restSharpClient, worker.Worker);
					var results = _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>
					(MetaDataType.FetchUsersViaPostCommented, worker.Topic.TopicFriendlyName).GetAwaiter().GetResult().ToList();
					if (results != null)
					{
						_heartbeatLogic.RefreshMetaData(MetaDataType.FetchMediaByCommenters, worker.Topic.TopicFriendlyName).GetAwaiter().GetResult();
						lock (results) { 
							results.TakeAny(takeMediaAmount).ToList().ForEach(_ => {
								if (_.HasValue)
								{
									_.Value.ObjectItem.ForEach(d =>
									{
										
										var suggested = searcher.SearchUsersMediaDetailInstagram(d.Username, limit).GetAwaiter().GetResult();
										if (suggested != null)
										{
											var sugVCut = suggested.CutObjects(cutBy).ToList();
											sugVCut.TakeAny(takeUserMediaAmount)?.ToList().ForEach(z =>
											{
												z.Medias = z.Medias.Where(t => !t.HasLikedBefore && !t.IsCommentsDisabled && t.User.Username != worker.InstagramRequests.InstagramAccount.Username).ToList();
												var comments = new __Meta__<Media>(z);
												_heartbeatLogic.AddMetaData(MetaDataType.FetchMediaByCommenters, worker.Topic.TopicFriendlyName, comments).GetAwaiter().GetResult();
											});
										}
										Task.Delay(TimeSpan.FromSeconds(secondsSleep));
									});
								}
							});
						}
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
		public void BuildUsersOwnMedias(int limit = 1, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildUsersOwnMedias");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				Parallel.ForEach(_assignments,worker=>{
					ContentSearch.ContentSearcher searcher = new ContentSearch.ContentSearcher(_restSharpClient, worker.Worker);
					var user = worker.InstagramRequests.InstagramAccount;
					var fetchUsersMedia = searcher.SearchUsersMediaDetailInstagram(user.Username,limit).GetAwaiter().GetResult();
					if (fetchUsersMedia != null)
					{
						_heartbeatLogic.RefreshMetaData(MetaDataType.FetchUserOwnProfile, worker.Topic.TopicFriendlyName, user.Id).GetAwaiter().GetResult();
						fetchUsersMedia.CutObjects(cutBy).ToList().ForEach(s =>
						{
							_heartbeatLogic.AddMetaData(MetaDataType.FetchUserOwnProfile, worker.Topic.TopicFriendlyName,
								new __Meta__<Media>(s),user.Id).GetAwaiter().GetResult();
						});
					}
				});
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			Console.WriteLine($"Ended - BuildUsersOwnMedias : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public void BuildUsersFeed(int limit = 1, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildUsersFeed");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				Parallel.ForEach(_assignments, worker => {
					ContentSearch.ContentSearcher searcher = new ContentSearch.ContentSearcher(_restSharpClient, worker.Worker);
					var user = worker.InstagramRequests.InstagramAccount;
					searcher.ChangeUser(new APIClientContainer(_context,user.AccountId,user.Id));
					var fetchUsersMedia = searcher.SearchUserFeedMediaDetailInstagram(limit:limit).GetAwaiter().GetResult();
					searcher.ChangeUser(worker.Worker);
					if (fetchUsersMedia != null)
					{
						_heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersFeed, worker.Topic.TopicFriendlyName, user.Id).GetAwaiter().GetResult();
						fetchUsersMedia.CutObjects(cutBy).ToList().ForEach(s =>
						{
							s.Medias = s.Medias.Where(_=>!_.HasLikedBefore && !_.IsCommentsDisabled && _.User.Username!=user.Username).ToList();
							_heartbeatLogic.AddMetaData(MetaDataType.FetchUsersFeed, worker.Topic.TopicFriendlyName,
								new __Meta__<Media>(s), user.Id).GetAwaiter().GetResult();
						});
					}
				});
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			Console.WriteLine($"Ended - BuildUsersFeed : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public void BuildUserFollowList(int limit = 3, int cutBy = 1)
		{
			Console.WriteLine("Began - BuildUserUnfollowList");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				Parallel.ForEach(_assignments, worker => {
					ContentSearch.ContentSearcher searcher = new ContentSearch.ContentSearcher(_restSharpClient, worker.Worker);
					var user = worker.InstagramRequests.InstagramAccount;
					var fetchUsersMedia = searcher.GetUserFollowingList(user.Username, limit).GetAwaiter().GetResult();
					if (fetchUsersMedia != null)
					{
						_heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersFollowingList, worker.Topic.TopicFriendlyName, user.Id).GetAwaiter().GetResult();
						fetchUsersMedia.ToList().CutObject(cutBy).ToList().ForEach(s =>
						{
							_heartbeatLogic.AddMetaData(MetaDataType.FetchUsersFollowingList, worker.Topic.TopicFriendlyName,
								new __Meta__<List<UserResponse<string>>>(s), user.Id).GetAwaiter().GetResult();
						});
					}
				});
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			Console.WriteLine($"Ended - BuildUserUnfollowList : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		public void BuildUsersFollowSuggestions(int limit = 1, int cutObjectBy = 1)
		{
			Console.WriteLine("Began - BuildUsersFollowSuggestions");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				Parallel.ForEach(_assignments, worker => {
					ContentSearch.ContentSearcher searcher = new ContentSearch.ContentSearcher(_restSharpClient, worker.Worker);
					var user = worker.InstagramRequests.InstagramAccount;
					searcher.ChangeUser(new APIClientContainer(_context, user.AccountId, user.Id));
					var fetchUsersMedia = searcher.GetSuggestedPeopleToFollow(limit).GetAwaiter().GetResult();
					searcher.ChangeUser(worker.Worker);
					if (fetchUsersMedia != null)
					{
						_heartbeatLogic.RefreshMetaData(MetaDataType.FetchUsersFollowSuggestions, worker.Topic.TopicFriendlyName, user.Id).GetAwaiter().GetResult();
						fetchUsersMedia.ToList().CutObject(cutObjectBy).ToList().ForEach(s =>
						{
							_heartbeatLogic.AddMetaData(MetaDataType.FetchUsersFollowSuggestions, worker.Topic.TopicFriendlyName,
								new __Meta__<List<UserResponse<UserSuggestionDetails>>>(s), user.Id).GetAwaiter().GetResult();
						});
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
