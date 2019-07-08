using Hangfire.Mongo.Dto;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using QuarklessContexts.Contexts.AccountContext;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessContexts.Models.Logger;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Proxies;
using QuarklessContexts.Models.ServicesModels;
using QuarklessContexts.Models.ServicesModels.Corpus;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using System;
using System.Threading.Tasks;

namespace QuarklessRepositories.RepositoryClientManager
{
	public class RepositoryContext : IRepositoryContext
	{
		private readonly IMongoDatabase _clientDatabase;
		private readonly IMongoDatabase _controlDatabase;
		private readonly IMongoDatabase _contentDatabase;
		private readonly IMongoDatabase _schedulerDatabase;
		public RepositoryContext(IOptions<Settings> options)
		{
			var client = new MongoClient(options.Value.ConnectionString);
			if (client != null)
			{
				_clientDatabase = client.GetDatabase(options.Value.MainDatabase);
				_controlDatabase = client.GetDatabase(options.Value.ControlDatabase);
				_contentDatabase = client.GetDatabase(options.Value.ContentDatabase);
				_schedulerDatabase = client.GetDatabase(options.Value.SchedulerDatabase);
			}
		}

		public IMongoCollection<TopicsModel> Topics
		{
			get
			{
				return _contentDatabase.GetCollection<TopicsModel>("Topics");
			}
		}

		public IMongoCollection<LoggerModel> Logger
		{
			get
			{
				return _controlDatabase.GetCollection<LoggerModel>("ReportHandle");
			}
		}

		public IMongoCollection<InstagramAccountModel> InstagramAccounts
		{
			get
			{
				return _clientDatabase.GetCollection<InstagramAccountModel>("InstagramAccounts");
			}
		}
		public IMongoCollection<InstagramClientAccount> InstagramClientAccount
		{
			get
			{
				return _clientDatabase.GetCollection<InstagramClientAccount>("InstagramAccounts");
			}
		}
		public IMongoCollection<ProxyModel> Proxies
		{
			get
			{
				return _clientDatabase.GetCollection<ProxyModel>("Proxies");
			}
		}

		public IMongoCollection<AccountUser> Account { 
			get 
			{
				return _clientDatabase.GetCollection<AccountUser>("accountUsers");	
			} 
		}

		public IMongoCollection<ProfileModel> Profiles 
		{
			get
			{
				return _clientDatabase.GetCollection<ProfileModel>("Profiles");
			}
		}

		public IMongoCollection<PostServiceModel> PostingService
		{
			get
			{
				return _controlDatabase.GetCollection<PostServiceModel>("PostsAnalyser");
			}
		}

		public IMongoCollection<CommentsModel> Comments 
		{ 
			get 
			{
				return _contentDatabase.GetCollection<CommentsModel>("Comments");
			} 
		}

		public IMongoCollection<CaptionsModel> Captions => _contentDatabase.GetCollection<CaptionsModel>("Captions");
		public IMongoCollection<HashtagsModel> Hashtags => _contentDatabase.GetCollection<HashtagsModel>("CHashtags");
		public IMongoCollection<UserBiographyModel> UserBiography => _contentDatabase.GetCollection<UserBiographyModel>("BiographyDetail");
		public IMongoCollection<JobDto> Timeline => _schedulerDatabase.GetCollection<JobDto>("Timeline.jobGraph");
		public IMongoCollection<CommentCorpus> CorpusComments => _contentDatabase.GetCollection<CommentCorpus>("CComments");
		public IMongoCollection<MediaCorpus> CorpusMedia => _contentDatabase.GetCollection<MediaCorpus>("CMedias");
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
	}
}
