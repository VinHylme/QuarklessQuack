using System;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Quarkless.Base.Agent.Logic;
using Quarkless.Base.Agent.Models.Interfaces;
using Quarkless.Base.InstagramAccounts.Logic;
using Quarkless.Base.InstagramAccounts.Models.Interfaces;
using Quarkless.Base.InstagramAccounts.Repository.Mongo;
using Quarkless.Base.Profile.Logic;
using Quarkless.Base.Profile.Models.Interfaces;
using Quarkless.Base.Profile.Repository;
using Quarkless.Base.ReportHandler.Logic;
using Quarkless.Base.ReportHandler.Models.Interfaces;
using Quarkless.Base.ReportHandler.Repository;
using Quarkless.Events;
using Quarkless.Events.Interfaces;
using Quarkless.Models.Shared.Extensions;
using Quarkless.Repository.MongoContext;
using Quarkless.Repository.MongoContext.Models;
using Quarkless.Repository.RedisContext;
using Quarkless.Repository.RedisContext.Models;

namespace Quarkless.Run.Services.Delegator.Extensions
{
	internal static class ConfigurationServices
	{
		internal static void IncludeServices(this IServiceCollection services)
		{
			var accessors = new Config().Environments;
			services.AddTransient<IProfileLogic, ProfileLogic>();
			services.AddTransient<IProfileRepository, ProfileRepository>();
			services.AddTransient<IInstagramAccountLogic, InstagramAccountLogic>();
			services.AddTransient<IInstagramAccountRepository, InstagramAccountRepository>();
			services.AddTransient<IAgentLogic, AgentLogic>();
			services.AddTransient<IReportHandler, ReportHandler>();
			services.AddTransient<ILoggerStore, LoggerStore>();
			services.AddTransient<IRedisClient, RedisClient>();
			services.AddTransient<IReportHandlerRepository, ReportHandlerRepository>();

			services.AddTransient<IEventPublisher, EventPublisher>(
				s => new EventPublisher(services));

			services.Configure<RedisOptions>(o =>
			{
				o.ConnectionString = accessors.RedisConnectionString;
				o.DefaultKeyExpiry = TimeSpan.FromDays(7);
			});
			
			BsonSerializer.RegisterSerializer(typeof(Guid), new GuidSerializer(BsonType.String));
			services.AddTransient<IMongoClientContext, MongoClientContext>(s =>
				new MongoClientContext(new MongoOptions
				{
					ConnectionString = accessors.ConnectionString,
					AccountCreatorDatabase = accessors.AccountCreationDatabase,
					AccountDatabase = accessors.MainDatabase,
					StatisticsDatabase = accessors.StatisticsDatabase,
					ControlDatabase = accessors.ControlDatabase,
					ContentDatabase = accessors.ContentDatabase
				}));
		}
	}
}
