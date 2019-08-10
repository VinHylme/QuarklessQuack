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
							_topicBuilder.Init(new UserStoreDetails { OAccountId = wo_.AccountId, OInstagramAccountUser = wo_.Id, OInstagramAccountUsername = wo_.Username });
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
								Worker = new APIClientContainer(_context, wo_.AccountId, wo_.Id)
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
							_topicBuilder.Init(new UserStoreDetails { OAccountId = worker_.AccountId, OInstagramAccountUser = worker_.Id, OInstagramAccountUsername = worker_.Username });
							var currAcc = requesters.ElementAtOrDefault(y);
							var profileTopic = currAcc.Profile.Topics;

							if (profileTopic.SubTopics == null || profileTopic.SubTopics.Count <= 0 || profileTopic.SubTopics.Any(s => s.RelatedTopics == null || s.RelatedTopics.Count < 3))
							{
								profileTopic = _topicBuilder.BuildTopics(currAcc.Profile).GetAwaiter().GetResult();
							}

							Assignments.Add(new Assignments
							{
								InstagramRequests = requesters.ElementAtOrDefault(y),
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

				MetadataBuilder metadataBuilder = new MetadataBuilder(Assignments, _context, _heartbeatLogic,_proxyLogic);
			
				//Build Around Topics
			
			 	//await _topicBuilder.AddTopicCategories(await metadataBuilder.BuildTopicTypes());

				var suba = await _topicBuilder.GetAllTopicCategories();
				await _topicBuilder.BuildTopics(suba);

				var ins = Task.Run(async () => await metadataBuilder.BuildBase(4)).ContinueWith(async tasker=>{
					Console.WriteLine("Finished building Base");
					await Task.Run(() =>
					{
						var likers = Task.Run(async () => await metadataBuilder.BuildUserFromLikers(takeMediaAmount:10,takeUserMediaAmount:300)).ContinueWith(async a =>
						{
							await metadataBuilder.BuildMediaFromUsersLikers(takeMediaAmount:10,takeUserMediaAmount:30).ContinueWith(async s => {
								await metadataBuilder.BuildCommentsFromSpecifiedSource(MetaDataType.FetchMediaByLikers, MetaDataType.FetchCommentsViaPostsLiked, limit: 2, takeMediaAmount: 10, takeuserAmount: 200);
							});
						});

						var commenters = Task.Run(async () => await metadataBuilder.BuildUsersFromCommenters(takeMediaAmount: 10, takeUserMediaAmount: 300)).ContinueWith(async c =>
						{
							await metadataBuilder.BuildMediaFromUsersCommenters(takeMediaAmount: 10, takeUserMediaAmount: 30).ContinueWith(async s => {
								await metadataBuilder.BuildCommentsFromSpecifiedSource(MetaDataType.FetchMediaByCommenters, MetaDataType.FetchCommentsViaPostCommented, limit: 2, takeMediaAmount: 10, takeuserAmount: 200);
							});
						});
						Task.WaitAll(likers,commenters);
					});
				});

				//Google & Yandex search
				var go = Task.Run(async () => await metadataBuilder.BuildGoogleImages(75));
				var yanq = Task.Run(async () => await metadataBuilder.BuildYandexImagesQuery(1));
				var yan = Task.Run(async () => await metadataBuilder.BuildYandexImages());

				var profileRefresh = Task.Run(async () => await metadataBuilder.BuildUsersOwnMedias(_instagramAccountLogic));
				//independed can run by themselves seperate tiem
				var feedRefresh = await Task.Run(async () => await metadataBuilder.BuildUsersFeed()).ContinueWith(async x=> 
				{
					await metadataBuilder.BuildUsersFollowSuggestions(2);
					await metadataBuilder.BuildCommentsFromSpecifiedSource(MetaDataType.FetchUsersFeed, MetaDataType.FetchCommentsViaUserFeed, true);
				});

				var followingList = Task.Run(async () => await metadataBuilder.BuildUserFollowList());
				var userTargetList = Task.Run(async () => await metadataBuilder.BuildUsersTargetListMedia()).ContinueWith(async x=>
				{
					await metadataBuilder.BuildCommentsFromSpecifiedSource(MetaDataType.FetchMediaByUserTargetList, MetaDataType.FetchCommentsViaUserTargetList, true, 2, takeMediaAmount: 10, takeuserAmount: 200);
				});
				var locTargetList = Task.Run(async () => await metadataBuilder.BuildLocationTargetListMedia()).ContinueWith(async s =>
				{
					await metadataBuilder.BuildCommentsFromSpecifiedSource(MetaDataType.FetchMediaByUserLocationTargetList, MetaDataType.FetchCommentsViaLocationTargetList, true, 2, takeMediaAmount:10, takeuserAmount:200);
				}); 

				Task.WaitAll(ins, go, yan, yanq, profileRefresh, feedRefresh, userTargetList, locTargetList);
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
			watch.Stop();
			Console.WriteLine($"Heartbeat ended : Took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
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
		private async Task<IEnumerable<ShortInstagramAccountModel>> GetAccounts(params string[] accounts)
		{
			List<ShortInstagramAccountModel> accs = new List<ShortInstagramAccountModel>();
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
