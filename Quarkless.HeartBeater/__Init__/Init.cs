using Quarkless.HeartBeater.__MetadataBuilder__;
using Quarkless.Services.ContentBuilder.TopicBuilder;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Proxies;
using QuarklessContexts.Models.ServicesModels.HeartbeatModels;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Logic.InstagramAccountLogic;
using QuarklessLogic.Logic.ProfileLogic;
using QuarklessLogic.Logic.ProxyLogic;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using Quarkless.HeartBeater.Creator;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.FakerModels;
using QuarklessContexts.Models.Topics;
using QuarklessLogic.Logic.ResponseLogic;

namespace Quarkless.HeartBeater.__Init__
{
	public struct RequestAccountModel
	{
		public ShortInstagramAccountModel InstagramAccount { get; set; }
		public ProfileModel Profile { get; set; } 
		public ProxyModel ProxyUsing { get; set; }
	}
	public struct Assignments
	{
		public IAPIClientContainer Worker { get; set; }
		public Topics Topic { get; set; }
		public RequestAccountModel InstagramRequests { get; set; }
	}
	public class Init : IInit
	{
		private readonly IResponseResolver _responseResolver;
		private readonly IAPIClientContext _context;
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private readonly IProfileLogic _profileLogic;
		private readonly IProxyLogic _proxyLogic;
		private readonly ITopicBuilder _topicBuilder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly ICreator _creator;
		private List<Assignments> Assignments { get; set; }
		public Init(IResponseResolver responseResolver, IInstagramAccountLogic instagramAccountLogic, IProfileLogic profileLogic, IProxyLogic proxyLogic,
			IAPIClientContext context, IHeartbeatLogic heartbeatLogic,
			ITopicBuilder topicBuilder, ICreator creator)
		{
			_responseResolver = responseResolver;
			_instagramAccountLogic = instagramAccountLogic;
			_profileLogic = profileLogic;
			_proxyLogic = proxyLogic;
			_heartbeatLogic = heartbeatLogic;
			_context = context;
			_topicBuilder = topicBuilder;
			_creator = creator;
			Assignments = new List<Assignments>();
		}

		public struct TopicAss
		{
			public string Searchable { get; set;  }
			public string FriendlyName { get; set;  }
		}
		public struct PopulateAssignments
		{
			public List<TopicAss> TopicsAssigned { get ; set;  }
			public IAPIClientContainer Worker { get; set; }
		}

		public async Task Creator()
		{
			var results = new List<Tempo>();
			try
			{
				for (var x = 0; x < 2000; x++)
				{
					var res = await _creator.CreateInstagramAccountMobile();
					if (res != null)
						results.Add(res);
					await Task.Delay(TimeSpan.FromMinutes(5.10));
				}

//				for (var x = 0; x < 1000; x++)
//				{
//					try
//					{
//						var proxy = new ProxyModel
//						{
//							Address = "37.48.118.4",
//							Port = 13010
//						};
//						var res = await _creator.CreateInstagramAccountWeb(proxy);
//						if (res != null)
//							results.Add(res);
//
//						await Task.Delay(1200);
//					}
//					catch (Exception ee)
//					{
//						Console.WriteLine(ee.Message);
//					}
//				}
				Console.WriteLine("wowe i finished");
				results.ForEach(i=>Console.WriteLine($"{i.FirstName}:{i.Username}:{i.Password}"));
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
		}
		public async Task Populator(CorpusSettings settings)
		{
			while (true)
			{
				try
				{
					if (settings.PerformCleaning)
					{
						Console.WriteLine("Started Cleaning");
						await _heartbeatLogic.CleanCorpus();
						Console.WriteLine("Ended Cleaning");
					}
					else
					{
						var workers = (await GetAccounts(true, settings.Accounts.Select(_ => _.Username).ToArray()))
							.ToList();
						_topicBuilder.Init(new UserStoreDetails
						{
							OAccountId = workers.FirstOrDefault()?.AccountId,
							OInstagramAccountUser = workers.FirstOrDefault()?.Id
						});
						//await _topicBuilder.BuildTopics((await _topicBuilder.GetAllTopicCategories())
						//	.DistinctBy(x=>x.CategoryName)
						//	.Where(sub=>sub.CategoryName.Contains("&")||sub.CategoryName.Contains("/")).Skip(1));
						var total = (await _topicBuilder.GetTopics())
							.Select(s => s.SubTopics)
							.SquashMe()
							.Where(c => c.RelatedTopics.Count > 0)
							.DistinctBy(c => c.Topic)
							.DistinctBy(c => c.RelatedTopics)
							.ToList();

						var topics = new List<TopicAss>();
						foreach (var subItem in total)
						{
							//var item = await _topicBuilder.GetAllRelatedTopics(subItem);
							if (subItem.RelatedTopics.Count <= 0) continue;
							var low = subItem.RelatedTopics.Select(op => new
							{
								Similarity = op.Similarity(subItem.Topic),
								Item = op
							}).OrderBy(_ => _.Similarity).Take(10).ToList();
							var selected = low.ElementAt(SecureRandom.Next(low.Count()));
							//await _topicBuilder.Update(selected, subItem);
							topics.Add(new TopicAss
							{
								Searchable = selected.Item,
								FriendlyName = subItem.Topic
							});
						}

						var populateAssignments = new List<PopulateAssignments>();

						var amountEach = topics.Count / workers.Count;
						var position = 0;

						foreach (var worker in workers)
						{
							populateAssignments.AddRange(new List<PopulateAssignments>
							{
								new PopulateAssignments
								{
									Worker = new APIClientContainer(_context, worker.AccountId, worker.Id),
									TopicsAssigned = topics.TakeBetween(position, amountEach).ToList()
								}
							});
							position += amountEach;
						}

						var metadataBuilder =
							new MetadataBuilder(Assignments, _context, _heartbeatLogic, _proxyLogic, _responseResolver);

						//var currentBusinessCategories = await metadataBuilder.BuildTopicTypes();
						await metadataBuilder.PopulateCorpusData(populateAssignments, 2);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}
				await Task.Delay(TimeSpan.FromMinutes(5));
			}
		}

		/// <summary>
		/// TO DO: refactor this code for more workers
		/// also need to deal with proxies of each worker properly and change the current proxy for each build request (using users proxy rn)
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		public async Task Endeavor(Settings settings)
		{
			Console.WriteLine("Heartbeat started");
			Console.WriteLine("------------------");
			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{ 
				#region Initialise
				var workers = await GetAccounts(false,settings.Accounts.Select(_ => _.Username).ToArray());
				var requester = await GetActiveInstagramAccountRequests(settings.ActionExecute);
				var shortInstagramAccountModels = workers as ShortInstagramAccountModel[] ?? workers.ToArray();
				var ratio = (double) shortInstagramAccountModels.Count() / requester.Count;

				if (ratio > 1)
				{
					for(var x = 0; x < shortInstagramAccountModels.Count(); x++)
					{
						if (x >= requester.Count) continue;
						var wo_ = shortInstagramAccountModels.ElementAtOrDefault(x);
						_topicBuilder.Init(new UserStoreDetails { OAccountId = wo_.AccountId, OInstagramAccountUser = wo_.Id, OInstagramAccountUsername = wo_.Username });
						var currAcc = requester.ElementAtOrDefault(x);
						var profileTopic = currAcc.Profile.Topics;
						if (profileTopic.SubTopics == null || profileTopic.SubTopics.Count <= 1 || profileTopic.SubTopics.Any(s=> s.RelatedTopics==null || s.RelatedTopics.Count<3))
						{
							profileTopic = _topicBuilder.BuildTopics(currAcc.Profile).GetAwaiter().GetResult();
						}

						Assignments.Add(new Assignments
						{
							InstagramRequests = requester.ElementAt(x),
							Topic = profileTopic,
							Worker = new APIClientContainer(_context, wo_.AccountId, wo_.Id)
						});
					}
				}
				else
				{
					var wpos = 0;
					for(var y = 0; y < requester.Count ; y++,wpos++)
					{
						var worker_ = shortInstagramAccountModels.ElementAtOrDefault(wpos);
						if (worker_ != null) { 
							_topicBuilder.Init(new UserStoreDetails { OAccountId = worker_.AccountId, OInstagramAccountUser = worker_.Id, OInstagramAccountUsername = worker_.Username });
							var currAcc = requester.ElementAtOrDefault(y);
							var profileTopic = currAcc.Profile.Topics;

							if (profileTopic.SubTopics == null || profileTopic.SubTopics.Count <= 0 || profileTopic.SubTopics.Any(s => s.RelatedTopics == null || s.RelatedTopics.Count < 3))
							{
								profileTopic = _topicBuilder.BuildTopics(currAcc.Profile).GetAwaiter().GetResult();
							}

							Assignments.Add(new Assignments
							{
								InstagramRequests = requester.ElementAtOrDefault(y),
								Topic = profileTopic,
								Worker = new APIClientContainer(_context,worker_.AccountId,worker_.Id)
							});
						}
						else
						{
							wpos = 0;
							y--;
						}
					}
				}
				#endregion

				var metadataBuilder = new MetadataBuilder(Assignments, _context, _heartbeatLogic, _proxyLogic, _responseResolver);
				//var currentBusinessCategories = await metadataBuilder.BuildTopicTypes();
				//await _topicBuilder.AddTopicCategories(currentBusinessCategories);
				switch (settings.ActionExecute)
				{
					case ActionExecuteType.Base:
						var buildBase = Task.Run(async () => await metadataBuilder.BuildBase(5));
						Task.WaitAll(buildBase);

						var liker = Task.Run(async () =>
							await metadataBuilder.BuildUserFromLikers(takeMediaAmount: 20, takeUserMediaAmount: 400));
						var commenter = Task.Run(async () =>
							await metadataBuilder.BuildUsersFromCommenters(takeMediaAmount: 20, takeUserMediaAmount: 400));

						Task.WaitAll(liker, commenter);
						var mediaLiker = metadataBuilder.BuildMediaFromUsersLikers(takeMediaAmount: 20, takeUserMediaAmount: 50);
						var mediaCommenter = metadataBuilder.BuildMediaFromUsersCommenters(takeMediaAmount: 20, takeUserMediaAmount: 50);

						Task.WaitAll(mediaLiker, mediaCommenter);
						var commentMediaLiker = metadataBuilder.BuildCommentsFromSpecifiedSource(MetaDataType.FetchMediaByLikers, 
							MetaDataType.FetchCommentsViaPostsLiked, limit: 2, takeMediaAmount: 20, takeuserAmount: 400);
						var commentMediaCommenter = metadataBuilder.BuildCommentsFromSpecifiedSource(MetaDataType.FetchMediaByCommenters, 
							MetaDataType.FetchCommentsViaPostCommented, limit: 2, takeMediaAmount: 20, takeuserAmount: 400);

						Task.WaitAll(commentMediaLiker, commentMediaCommenter);
						break;
					case ActionExecuteType.Other:
						var googleImages = Task.Run(async () => await metadataBuilder.BuildGoogleImages(20));
						var yandexImages = Task.Run(async () => await metadataBuilder.BuildYandexImages(takeTopicAmount:1));
						Task.WaitAll(googleImages, yandexImages);
						break;
					case ActionExecuteType.UserSelf:
						var profileRefresh = Task.Run(async () => await metadataBuilder.BuildUsersOwnMedias(_instagramAccountLogic,3))
							.ContinueWith(async x => { await metadataBuilder.BuildUsersRecentComments(); });
						var followingList = Task.Run(async () => await metadataBuilder.BuildUserFollowList());
						var followerList = Task.Run(async () => await metadataBuilder.BuildUserFollowerList());
						var feedRefresh = await Task.Run(async () => await metadataBuilder.BuildUsersFeed()).ContinueWith(async x=> 
						{
							await metadataBuilder.BuildUsersFollowSuggestions(2);
							await metadataBuilder.BuildUsersInbox();
							await metadataBuilder.BuildCommentsFromSpecifiedSource(MetaDataType.FetchUsersFeed, MetaDataType.FetchCommentsViaUserFeed, true);
						});
						Task.WaitAll(profileRefresh, followerList, followingList, feedRefresh);
						break;
					case ActionExecuteType.TargetList:
						var userTargetList = await Task.Run(async () => await metadataBuilder.BuildUsersTargetListMedia(1))
							.ContinueWith(async x=>
							{
								await metadataBuilder.BuildCommentsFromSpecifiedSource(MetaDataType.FetchMediaByUserTargetList, MetaDataType.FetchCommentsViaUserTargetList, true, 2, takeMediaAmount: 14, takeuserAmount: 200);
							});
						var locTargetList = await Task.Run(async () => await metadataBuilder.BuildLocationTargetListMedia(1))
							.ContinueWith(async s =>
							{
								await metadataBuilder.BuildCommentsFromSpecifiedSource(MetaDataType.FetchMediaByUserLocationTargetList, MetaDataType.FetchCommentsViaLocationTargetList, true, 2, takeMediaAmount:14, takeuserAmount:200);
							});
						Task.WaitAll(userTargetList, locTargetList);
						break;
				}
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			watch.Stop();
			Console.WriteLine($"Heartbeat ended : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
		}
		private async Task<List<RequestAccountModel>> GetActiveInstagramAccountRequests(ActionExecuteType type)
		{
			try
			{
				var resp =  await _instagramAccountLogic.GetActiveAgentInstagramAccounts((int)type);
				return resp.Select(p=>new RequestAccountModel
				{
					InstagramAccount = p,
					Profile = _profileLogic.GetProfile(p.AccountId,p.Id).GetAwaiter().GetResult(),
					ProxyUsing = _proxyLogic.GetProxyAssignedTo(p.AccountId,p.Id).GetAwaiter().GetResult()
				}).ToList();
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}
		private async Task<IEnumerable<ShortInstagramAccountModel>> GetAccounts(bool both, params string[] accounts)
		{
			var accs = new List<ShortInstagramAccountModel>();
			foreach (var account in accounts)
			{
				var resp = await _instagramAccountLogic.GetInstagramAccountsOfUser(account, 1);
				if (both)
				{
					var respa = await _instagramAccountLogic.GetInstagramAccountsOfUser(account, 0);
					var instagramAccountModels = respa as ShortInstagramAccountModel[] ?? respa.ToArray();
					if(instagramAccountModels.Any())
						accs.AddRange(instagramAccountModels);
				}

				var shortInstagramAccountModels = resp as ShortInstagramAccountModel[] ?? resp.ToArray();
				if (shortInstagramAccountModels.Any())
				{
					accs.AddRange(shortInstagramAccountModels);
				}
				continue;
			}
			return accs;
		}
	}
}
