using System;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Quarkless.Events;
using Quarkless.Events.Interfaces;
using Quarkless.Logic.Agent;
using Quarkless.Logic.InstagramAccounts;
using Quarkless.Logic.ReportHandler;
using Quarkless.Models.Agent.Interfaces;
using Quarkless.Models.InstagramAccounts.Interfaces;
using Quarkless.Models.ReportHandler.Interfaces;
using Quarkless.Models.Shared.Extensions;
using Quarkless.Repository.InstagramAccounts.Mongo;
using Quarkless.Repository.MongoContext;
using Quarkless.Repository.MongoContext.Models;
using Quarkless.Repository.RedisContext;
using Quarkless.Repository.RedisContext.Models;
using Quarkless.Repository.ReportHandler;

namespace Quarkless.Run.Services.Delegator.Extensions
{
	internal static class ConfigurationServices
	{
		internal static void IncludeServices(this IServiceCollection services)
		{
			var accessors = new Config().Environments;

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
