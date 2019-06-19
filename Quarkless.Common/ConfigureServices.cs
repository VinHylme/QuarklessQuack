using ContentSearcher.SeleniumClient;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Quarkless.Queue.Interfaces.Jobs;
using Quarkless.Queue.Jobs.Interfaces;
using Quarkless.Queue.Services;
using Quarkless.Services;
using Quarkless.Services.ContentBuilder.TopicBuilder;
using Quarkless.Services.ContentSearch;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Contexts;
using QuarklessContexts.InstaClient;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessLogic.Handlers.RequestBuilder.RequestBuilder;
using QuarklessLogic.Handlers.TextGeneration;
using QuarklessLogic.Handlers.TranslateService;
using QuarklessLogic.Handlers.Util;
using QuarklessLogic.Logic.CollectionsLogic;
using QuarklessLogic.Logic.CommentLogic;
using QuarklessLogic.Logic.DiscoverLogic;
using QuarklessLogic.Logic.HashtagLogic;
using QuarklessLogic.Logic.InstaAccountOptionsLogic;
using QuarklessLogic.Logic.InstagramAccountLogic;
using QuarklessLogic.Logic.InstaUserLogic;
using QuarklessLogic.Logic.MediaLogic;
using QuarklessLogic.Logic.ProfileLogic;
using QuarklessLogic.Logic.ProxyLogic;
using QuarklessLogic.RestSharpClient;
using QuarklessLogic.ServicesLogic;
using QuarklessLogic.ServicesLogic.TimelineServiceLogic.TimelineLogic;
using QuarklessRepositories.InstagramAccountRepository;
using QuarklessRepositories.ProfileRepository;
using QuarklessRepositories.ProxyRepository;
using QuarklessRepositories.Repository.ServicesRepositories;
using QuarklessRepositories.Repository.ServicesRepositories.CommentsRepository;
using QuarklessRepositories.Repository.ServicesRepositories.HashtagsRepository;
using QuarklessRepositories.Repository.ServicesRepositories.TopicsRepository;
using QuarklessRepositories.RepositoryClientManager;
using System;

namespace Quarkless.Common
{
	public static class ConfigureServices
	{
		public static void AddHangFrameworkServices(this IServiceCollection serviceCollection, Accessors accessors)
		{
			serviceCollection.AddTransient<ITaskService, TaskService>();
			serviceCollection.AddTransient<IBackgroundJobClient, BackgroundJobClient>();
			serviceCollection.AddTransient<IJobRunner, JobRunner>();		
		}

		public static void AddLogics(this IServiceCollection services)
		{
			services.AddSingleton<IDiscoverLogic, DiscoverLogic>();
			services.AddTransient<IProxyLogic, ProxyLogic>();
			services.AddTransient<IInstagramAccountLogic, InstagramAccountLogic>();
			services.AddTransient<IProfileLogic, ProfileLogic>();
			services.AddTransient<IInstaUserLogic, InstaUserLogic>();
			services.AddTransient<ICommentLogic, CommentLogic>();
			services.AddTransient<ICollectionsLogic, CollectionsLogic>();
			services.AddTransient<IInstaAccountOptionsLogic, InstaAccountOptionsLogic>();
			services.AddTransient<IInstaClient,InstaClient>();
			services.AddTransient<IHashtagLogic, HashtagLogic>();
			services.AddTransient<IAgentManager, AgentManager>();
			services.AddTransient<IMediaLogic,MediaLogic>();
			services.AddTransient<ITimelineLogic,TimelineLogic>();

		}
		public static void AddRepositories(this IServiceCollection services, Accessors _accessors)
		{
			BsonSerializer.RegisterSerializer(typeof(Guid),
			new GuidSerializer(BsonType.String));
			services.Configure<Settings>(o => {
				o.ConnectionString = _accessors.ConnectionString;
				o.MainDatabase = _accessors.MainDatabase;
				o.ControlDatabase = _accessors.ControlDatabase;
				o.ContentDatabase = _accessors.ContentDatabase;
			});
			services.AddSingleton<IRepositoryContext, RepositoryContext>();
			services.AddTransient<IInstagramAccountRepository, InstagramAccountRepository>();
			services.AddTransient<IProxyRepostory, ProxyRepository>();
			services.AddTransient<IProfileRepository, ProfileRepository>();
			services.AddTransient<IPostServicesRepository, PostServicesRepository>();
			services.AddTransient<ITopicsRepository,TopicsRepository>();
			services.AddTransient<ICommentsRepository,CommentsRepository>();
			services.AddTransient<IHashtagsRepository,HashtagsRepository>();
		}
		public static void AddHandlers(this IServiceCollection services)
		{
			services.AddTransient<IReportHandler, ReportHandler>();
			services.AddTransient<IRestSharpClientManager, RestSharpClientManager>();
			services.AddTransient<IContentSearch, ContentSearch>();
			services.AddTransient<ITopicServicesLogic, TopicServicesLogic>();

			services.AddTransient<IClientContextProvider, ClientContextProvider>();
			services.AddTransient<IAPIClientContext,APIClientContext>();
			services.AddTransient<IAPIClientContainer,APIClientContainer>();
			services.AddTransient<ISeleniumClient,SeleniumClient>();
			services.AddTransient<ITranslateService,TranslateService>();
			services.AddTransient<IUtilProviders,UtilProviders>();
			services.AddSingleton<IContentManager, ContentManager>();
			services.AddSingleton<ITextGeneration,TextGeneration>();
		
		}
		public static void AddContexts(this IServiceCollection services)
		{
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.AddTransient<IUserContext, UserContext>();
			services.AddSingleton<IRequestBuilder,RequestBuilder>();
			services.AddSingleton<ITopicBuilder, TopicBuilder>();
		}
	}
}
