using Hangfire.Mongo.Dto;
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

namespace QuarklessRepositories.RepositoryClientManager
{
	public interface IRepositoryContext
	{
		IMongoCollection<InstagramAccountModel> InstagramAccounts { get; }
		IMongoCollection<InstagramClientAccount> InstagramClientAccount { get;}
		IMongoCollection<ProxyModel> Proxies { get;}
		IMongoCollection<AccountUser> Account { get; }
		IMongoCollection<ProfileModel> Profiles { get;  }
		IMongoCollection<LoggerModel> Logger { get; }
		IMongoCollection<PostServiceModel> PostingService { get; }
		IMongoCollection<TopicsModel> Topics {get; }
		IMongoCollection<CommentCorpus> CorpusComments { get; }
		IMongoCollection<MediaCorpus> CorpusMedia { get; }

		IMongoCollection<CommentsModel> Comments { get; }
		IMongoCollection<CaptionsModel> Captions { get; }
		IMongoCollection<HashtagsModel> Hashtags { get; }
		IMongoCollection<UserBiographyModel> UserBiography { get; }
		IMongoCollection<JobDto> Timeline { get; }
	}
}