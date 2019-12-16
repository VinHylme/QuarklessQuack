using System;
using System.Threading.Tasks;
using Hangfire.Mongo.Dto;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using QuarklessContexts.Contexts.AccountContext;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessContexts.Models.Library;
using QuarklessContexts.Models.Logger;
using QuarklessContexts.Models.Options;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Proxies;
using QuarklessContexts.Models.ServicesModels.Corpus;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessContexts.Models.TimelineLoggingRepository;
using QuarklessContexts.Models.Topics;
using QuarklessRepositories.RepositoryClientManager;

namespace QuarklessRepositories.Repository.RepositoryClientManager
{
	public class RepositoryContext : IRepositoryContext
	{
		#region Initialisers
		private readonly IMongoDatabase _clientDatabase;
		private readonly IMongoDatabase _controlDatabase;
		private readonly IMongoDatabase _contentDatabase;
		private readonly IMongoDatabase _schedulerDatabase;
		public RepositoryContext(IOptions<MongoSettings> options)
		{
			var client = new MongoClient(options.Value.ConnectionString);
			_clientDatabase = client.GetDatabase(options.Value.MainDatabase);
			_controlDatabase = client.GetDatabase(options.Value.ControlDatabase);
			_contentDatabase = client.GetDatabase(options.Value.ContentDatabase);
			_schedulerDatabase = client.GetDatabase(options.Value.SchedulerDatabase);
		}
		#endregion

		#region Timeline and Report Handlers
		public IMongoCollection<LoggerModel> Logger => _controlDatabase.GetCollection<LoggerModel>("ReportHandle");
		public IMongoCollection<TimelineEventLog> TimelineLogger =>
			_controlDatabase.GetCollection<TimelineEventLog>("TimelineLogger");
		public IMongoCollection<JobDto> Timeline => _schedulerDatabase.GetCollection<JobDto>("Timeline.jobGraph");
		#endregion

		#region User Profiles
		public IMongoCollection<InstagramAccountModel> InstagramAccounts => _clientDatabase.GetCollection<InstagramAccountModel>("InstagramAccounts");
		public IMongoCollection<ProxyModel> Proxies => _clientDatabase.GetCollection<ProxyModel>("Proxies");
		public IMongoCollection<AccountUser> Account => _clientDatabase.GetCollection<AccountUser>("accountUsers");
		public IMongoCollection<ProfileModel> Profiles => _clientDatabase.GetCollection<ProfileModel>("Profiles");
		#endregion

		#region User Saved Medias / Captions / Messages/ Hashtags
		public IMongoCollection<MediasLib> MediaLibrary =>
			_clientDatabase.GetCollection<MediasLib>("UsersMediasLibrary");
		public IMongoCollection<HashtagsLib> HashtagLibrary =>
			_clientDatabase.GetCollection<HashtagsLib>("UsersHashtagsLibrary");
		public IMongoCollection<CaptionsLib> CaptionLibrary =>
			_clientDatabase.GetCollection<CaptionsLib>("UsersCaptionsLibrary");
		public IMongoCollection<MessagesLib> MessagesLibrary=>
			_clientDatabase.GetCollection<MessagesLib>("UsersMessagesLibrary");
		#endregion

		public IMongoCollection<CTopic> TopicLookup => _contentDatabase.GetCollection<CTopic>("TopicLookup");
		public IMongoCollection<TopicsModel> Topics => _contentDatabase.GetCollection<TopicsModel>("Topics");
		public IMongoCollection<HashtagsModel> Hashtags => _contentDatabase.GetCollection<HashtagsModel>("CCHashtags");
		public IMongoCollection<CommentCorpus> CorpusComments => _contentDatabase.GetCollection<CommentCorpus>("CCComments");
		public IMongoCollection<MediaCorpus> CorpusMedia => _contentDatabase.GetCollection<MediaCorpus>("CCMedias");
		public IMongoCollection<TopicCategory> TopicCategories => _contentDatabase.GetCollection<TopicCategory>("CategoryTopic");

		#region Local Functions
		internal async Task<bool> CreateCollection(string collectionName, BsonDocument document = null)
		{
			try
			{
				await _clientDatabase.CreateCollectionAsync(collectionName);
				IMongoCollection<BsonDocument> collection = _clientDatabase.GetCollection<BsonDocument>(collectionName);
				if (collection != null)
				{
					if (document != null)
					{
						await collection.InsertOneAsync(document); return true;
					}
					else
						return false;
				}
				else
					return false;
			}
			catch (Exception ee)
			{
				return false;
			}
		}
		internal async Task<IAsyncCursor<string>> Collections()
		{
			return await _clientDatabase.ListCollectionNamesAsync();
		}
		#endregion
	}
}
