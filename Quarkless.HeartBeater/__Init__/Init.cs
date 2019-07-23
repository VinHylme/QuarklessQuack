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
		private readonly IAPIClientContext _context;
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private readonly IProfileLogic _profileLogic;
		private readonly IProxyLogic _proxyLogic;
		private readonly ITopicBuilder _topicBuilder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private List<Assignments> Assignments { get; set; }
		public Init(IInstagramAccountLogic instagramAccountLogic, IProfileLogic profileLogic, IProxyLogic proxyLogic,
			IAPIClientContext context, IHeartbeatLogic heartbeatLogic,
			ITopicBuilder topicBuilder)
		{
			_instagramAccountLogic = instagramAccountLogic;
			_profileLogic = profileLogic;
			_proxyLogic = proxyLogic;
			_heartbeatLogic = heartbeatLogic;
			_context = context;
			_topicBuilder = topicBuilder;
			Assignments = new List<Assignments>();
		}
		public async Task Endeavor(Settings settings)
		{
			while (true) { 
				Console.WriteLine("Heartbeat started");
				Console.WriteLine("------------------");
				var watch = System.Diagnostics.Stopwatch.StartNew();
				try
				{ 
					var workers = await GetAccounts(settings.Accounts.Select(_ => _.Username).ToArray());
					var requesters = await GetActiveInstagramAccountRequests();
					var ratio = (double) workers.Count() / requesters.Count;

					if (ratio > 1)
					{
						for(int x = 0; x < workers.Count(); x++)
						{
							if (x < requesters.Count) { 
								var wo_ = workers.ElementAtOrDefault(x);
								_topicBuilder.Init(new UserStoreDetails { OAccountId = wo_.AccountId, OInstagramAccountUser = wo_._id, OInstagramAccountUsername = wo_.Username });
								var currAcc = requesters.ElementAtOrDefault(x);
								var profileTopic = currAcc.Profile.Topics;
								if (profileTopic.SubTopics == null || profileTopic.SubTopics.Count <= 1 || profileTopic.SubTopics.Any(s=> s.RelatedTopics==null || s.RelatedTopics.Count<3))
								{
									profileTopic = _topicBuilder.BuildTopics(currAcc.Profile).GetAwaiter().GetResult();
								}

								Assignments.Add(new Assignments
								{
									InstagramRequests = requesters.ElementAt(x),
									Topic = profileTopic,
									Worker = new APIClientContainer(_context, wo_.AccountId, wo_._id)
								});
							}
						}
					}
					else
					{
						int wpos = 0;
						for(int y = 0; y < requesters.Count ; y++,wpos++)
						{
							var worker_ = workers.ElementAtOrDefault(wpos);
							if (worker_ != null) { 
								_topicBuilder.Init(new UserStoreDetails { OAccountId = worker_.AccountId, OInstagramAccountUser = worker_._id, OInstagramAccountUsername = worker_.Username });
								var currAcc = requesters.ElementAtOrDefault(y);
								var profileTopic = currAcc.Profile.Topics;

								if (profileTopic.SubTopics == null || profileTopic.SubTopics.Count <= 1 || profileTopic.SubTopics.Any(s => s.RelatedTopics == null || s.RelatedTopics.Count < 3))
								{
									profileTopic = _topicBuilder.BuildTopics(currAcc.Profile).GetAwaiter().GetResult();
								}

								Assignments.Add(new Assignments
								{
									InstagramRequests = requesters.ElementAtOrDefault(y),
									Topic = profileTopic,
									Worker = new APIClientContainer(_context,worker_.AccountId,worker_._id)
								});
							}
							else
							{
								wpos = 0;
								y--;
							}
						}
					}

					MetadataBuilder metadataBuilder = new MetadataBuilder(Assignments, _context, _heartbeatLogic,_proxyLogic);
			
					//Build Around Topics
			
					var ins = Task.Run(async () => await metadataBuilder.BuildBase());

					//Google & Yandex search
					var go =Task.Run(async () => await metadataBuilder.BuildGoogleImages(50));
					var yanq = Task.Run(async () => await metadataBuilder.BuildYandexImagesQuery(50));
					var yan =Task.Run(async () => await metadataBuilder.BuildYandexImages());

					var profileRefresh = Task.Run(async () => await metadataBuilder.BuildUsersOwnMedias(_instagramAccountLogic));
					//independed can run by themselves seperate tiem
					var feedRefresh = Task.Run(async () => await metadataBuilder.BuildUsersFeed()).ContinueWith(async x=> 
					{
						await metadataBuilder.BuildUsersFollowSuggestions();
						await metadataBuilder.BuildCommentsFromSpecifiedSource(MetaDataType.FetchUsersFeed, MetaDataType.FetchCommentsViaUserFeed, true);
					});

					var followingList = Task.Run(async () => await metadataBuilder.BuildUserFollowList());
					var userTargetList = Task.Run(async () => await metadataBuilder.BuildUsersTargetListMedia()).ContinueWith(async x=>
					{
						await metadataBuilder.BuildCommentsFromSpecifiedSource(MetaDataType.FetchMediaByUserTargetList, MetaDataType.FetchCommentsViaUserTargetList, true);
					});
					var locTargetList = Task.Run(async () => await metadataBuilder.BuildLocationTargetListMedia()).ContinueWith(async s =>
					{
						await metadataBuilder.BuildCommentsFromSpecifiedSource(MetaDataType.FetchMediaByUserLocationTargetList, MetaDataType.FetchCommentsViaLocationTargetList, true);
					}); ;
					Task.WaitAll(ins);

					var likers = Task.Run(async () => await metadataBuilder.BuildUserFromLikers()).ContinueWith(async a =>
					{
						await metadataBuilder.BuildMediaFromUsersLikers();
						await metadataBuilder.BuildCommentsFromSpecifiedSource(MetaDataType.FetchMediaByLikers, MetaDataType.FetchCommentsViaPostsLiked);
					});
					var commenters = Task.Run(async () => await metadataBuilder.BuildUsersFromCommenters()).ContinueWith(async c =>
					{	
						await metadataBuilder.BuildMediaFromUsersCommenters();
						await metadataBuilder.BuildCommentsFromSpecifiedSource(MetaDataType.FetchMediaByCommenters, MetaDataType.FetchCommentsViaPostCommented);
					});

					Task.WaitAll(go, yan, yanq, likers, commenters, profileRefresh, feedRefresh, userTargetList, locTargetList);
				}
				catch(Exception ee)
				{
					Console.WriteLine(ee.Message);
				}
				watch.Stop();
				Console.WriteLine($"Heartbeat ended : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
				await Task.Delay(TimeSpan.FromMinutes(5));
			}
		}

		private async Task<List<RequestAccountModel>> GetActiveInstagramAccountRequests()
		{
			try
			{
				List<RequestAccountModel> Requests = new List<RequestAccountModel>();
				var resp =  await _instagramAccountLogic.GetActiveAgentInstagramAccounts();
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
		private async Task<IEnumerable<InstagramAccountModel>> GetAccounts(params string[] accounts)
		{
			List<InstagramAccountModel> accs = new List<InstagramAccountModel>();
			foreach (var account in accounts)
			{
				var resp = await _instagramAccountLogic.GetInstagramAccountsOfUser(account, 1);
				if (resp != null && resp.Count() > 0)
				{
					foreach (var acc in resp)
					{
						accs.Add(acc);
					}
				}
				continue;
			}
			return accs;
		}
	}
}
