using Quarkless.HeartBeater.__MetadataBuilder__;
using Quarkless.Services.ContentBuilder.TopicBuilder;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Logic.InstagramAccountLogic;
using QuarklessLogic.Logic.ProfileLogic;
using QuarklessLogic.RestSharpClient;
using QuarklessLogic.ServicesLogic;
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
	}
	public struct Assignments
	{
		public IAPIClientContainer Worker { get; set; }
		public TopicsModel Topic { get; set; }
		public RequestAccountModel InstagramRequests { get; set; }
	}
	public class Init : IInit
	{
		private readonly IAPIClientContext _context;
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private readonly IProfileLogic _profileLogic;
		private readonly ITopicBuilder _topicBuilder;
		private readonly IRestSharpClientManager _restSharpClient;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private List<Assignments> Assignments { get; set; }
		public Init(IInstagramAccountLogic instagramAccountLogic, IProfileLogic profileLogic, 
			IAPIClientContext context, IRestSharpClientManager restSharpClient, IHeartbeatLogic heartbeatLogic,
			ITopicBuilder topicBuilder)
		{
			_instagramAccountLogic = instagramAccountLogic;
			_profileLogic = profileLogic;
			_restSharpClient = restSharpClient;
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
							_topicBuilder.Init(new UserStoreDetails { OAccountId = wo_.AccountId, OInstagramAccountUser = wo_._id, OInstagramAccountUsername = wo_.Username });
							
							var topic_ = await _topicBuilder.Build(requesters.ElementAtOrDefault(x).Profile.Topic.ToLower(), 15); 
							Assignments.Add(new Assignments
							{
								InstagramRequests = requesters.ElementAt(x),
								Topic = topic_,
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
							var topic = await _topicBuilder.Build(requesters.ElementAtOrDefault(y).Profile.Topic.ToLower(),25);
							if(topic==null) return ;
							Assignments.Add(new Assignments
							{
								InstagramRequests = requesters.ElementAtOrDefault(y),
								Topic = topic,
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

				MetadataBuilder metadataBuilder = new MetadataBuilder(Assignments,_restSharpClient, _context, _heartbeatLogic);
			
				//Build Around Topics
			
				var ins = Task.Run(()=>metadataBuilder.BuildBase());
				//google and yandex search are seperate
				var go =Task.Run(()=>metadataBuilder.BuildGoogleImages());
				var yan =Task.Run(()=> metadataBuilder.BuildYandexImages());
			
				//independed can run by themselves seperate tiem
				var profileRefresh = Task.Run(() => { 
					metadataBuilder.BuildUsersOwnMedias();
				});
				var feedRefresher = Task.Run(() => { 
					metadataBuilder.BuildUsersFeed();
				}).ContinueWith(f=>{
					metadataBuilder.BuildUsersFollowSuggestions();
				});
				var followingList = Task.Run(() => {
					metadataBuilder.BuildUserFollowList();
				});
				Task.WaitAll(ins);  //depended on the initial media fetch
				var likers = Task.Run(() => { 
					metadataBuilder.BuildUserFromLikers();
				}).ContinueWith(a=>{
					metadataBuilder.BuildMediaFromUsersLikers();
				});
				var commenters = Task.Run(() => { 
					metadataBuilder.BuildUsersFromCommenters();
				}).ContinueWith(c => {
					metadataBuilder.BuildMediaFromUsersCommenters();
				});

				Task.WaitAll(go,yan,likers,commenters,profileRefresh,feedRefresher);
					//Build Around specific Users
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
					Profile = _profileLogic.GetProfile(p.AccountId,p.Id).GetAwaiter().GetResult()
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
