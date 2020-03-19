using System;
using System.Runtime.InteropServices;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Quarkless.Analyser;
using Quarkless.Analyser.Models;
using Quarkless.Base.Actions.Logic.Factory.ActionExecute.Manager;
using Quarkless.Base.Actions.Models.Interfaces;
using Quarkless.Base.Auth.Common.Models.Interfaces;
using Quarkless.Base.AuthDetails.Logic;
using Quarkless.Base.InstagramAccounts.Logic;
using Quarkless.Base.InstagramAccounts.Models.Interfaces;
using Quarkless.Base.InstagramAccounts.Repository.Mongo;
using Quarkless.Base.InstagramClient.Logic;
using Quarkless.Base.InstagramClient.Models.Interfaces;
using Quarkless.Base.Lookup.Logic;
using Quarkless.Base.Lookup.Models.Interfaces;
using Quarkless.Base.Lookup.Repository;
using Quarkless.Base.Notification.Logic;
using Quarkless.Base.Notification.Models.Interfaces;
using Quarkless.Base.Notification.Repository;
using Quarkless.Base.Profile.Logic;
using Quarkless.Base.Profile.Models.Interfaces;
using Quarkless.Base.Profile.Repository;
using Quarkless.Base.Proxy.Logic;
using Quarkless.Base.Proxy.Models;
using Quarkless.Base.Proxy.Models.Interfaces;
using Quarkless.Base.Proxy.Repository;
using Quarkless.Base.ReportHandler.Logic;
using Quarkless.Base.ReportHandler.Models.Interfaces;
using Quarkless.Base.ReportHandler.Repository;
using Quarkless.Base.ResponseResolver.Logic;
using Quarkless.Base.ResponseResolver.Models.Interfaces;
using Quarkless.Base.Timeline.Logic.TaskScheduler;
using Quarkless.Base.Timeline.Models.Interfaces;
using Quarkless.Base.Timeline.Models.Interfaces.TaskScheduler;
using Quarkless.Base.Timeline.Repository;
using Quarkless.Events;
using Quarkless.Events.Interfaces;
using Quarkless.Geolocation;
using Quarkless.Models.Shared.Extensions;
using Quarkless.Repository.MongoContext;
using Quarkless.Repository.MongoContext.Models;
using Quarkless.Repository.RedisContext;
using Quarkless.Repository.RedisContext.Models;

namespace Quarkless.Run.Services.Scheduler.Extensions
{
	public static class ConfigurationServices
	{
		private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

		internal static void IncludeHangFrameworkServices(this IServiceCollection serviceCollection)
		{
			serviceCollection.AddSingleton<ITaskService, TaskService>();
			serviceCollection.AddSingleton<IBackgroundJobClient, BackgroundJobClient>();
			serviceCollection.AddSingleton<IJobRunner, JobRunner>();
		}

		internal static void IncludeLogic(this IServiceCollection serviceCollection)
		{
			serviceCollection.AddTransient<IInstagramAccountLogic, InstagramAccountLogic>();
			serviceCollection.AddTransient<IReportHandler, ReportHandler>();
			serviceCollection.AddTransient<IApiClientContext, ApiClientContext>();
			serviceCollection.AddTransient<IClientContextProvider, ClientContextProvider>();
			serviceCollection.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			serviceCollection.AddTransient<IUserContext, UserContext>();
			serviceCollection.AddTransient<IApiClientContainer, ApiClientContainer>();
			serviceCollection.AddTransient<IProxyLogic, ProxyLogic>();
			serviceCollection.AddTransient<IProfileLogic, ProfileLogic>();
			serviceCollection.AddTransient<IResponseResolver, ResponseResolver>();
			serviceCollection.AddTransient<ILookupLogic, LookupLogic>();
			serviceCollection.AddTransient<INotificationLogic, NotificationLogic>();
			serviceCollection.AddSingleton<IPostAnalyser, PostAnalyser>();
			serviceCollection.AddSingleton<IMediaManipulation, MediaManipulation>();
			serviceCollection.AddSingleton<IVideoEditor, VideoEditor>();
			serviceCollection.AddTransient<IEventPublisher, EventPublisher>(
				s => new EventPublisher(serviceCollection));

			serviceCollection.AddSingleton(
				s => new EventActionJob(s.GetService<IActionExecuteFactory>(), s.GetService<IActionExecuteLogsRepository>()));

		}

		internal static void IncludeRepositoryAndConfigs(this IServiceCollection services)
		{
			BsonSerializer.RegisterSerializer(typeof(Guid),
				new GuidSerializer(BsonType.String));

			var accessors = new Config().Environments;
			services.Configure<RedisOptions>(o =>
			{
				o.ConnectionString = accessors.RedisConnectionString;
				o.DefaultKeyExpiry = TimeSpan.FromDays(7);
			});

			services.AddSingleton<IFfmpegWrapper>(new FfmpegWrapper(new MediaAnalyserOptions
			{
				TempAudioPath = accessors.TempAudioPath,
				TempImagePath = accessors.TempImagePath,
				TempVideoPath = accessors.TempVideoPath,
				FfmpegEnginePath = accessors.FfmpegPath,
				IsOnWindows = IsWindows
			}));

			services.AddTransient<IRedisClient, RedisClient>();

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

			services.AddTransient<IInstagramAccountRepository, InstagramAccountRepository>();
			services.AddTransient<IProfileRepository, ProfileRepository>();

			services.AddTransient<IProxyAssignmentsRepository, ProxyAssignmentsRepository>();

			services.AddSingleton<IGeoLocationHandler, GeoLocationHandler>(s =>
				new GeoLocationHandler(new GeoLocationOptions
				{
					IpGeolocationToken = accessors.IpGeoLocationApiKey,
					GeonamesToken = accessors.GeonamesApiKey,
					GoogleGeocodeToken = accessors.GoogleGeocodeApiKey
				}));

			services.AddTransient<IProxyRequest, ProxyRequest>(s => new ProxyRequest(
				new ProxyRequestOptions(accessors.ProxyHandlerApiEndPoint),
				s.GetService<IGeoLocationHandler>(),
				s.GetService<IProxyAssignmentsRepository>()));

			services.AddTransient<ILoggerStore, LoggerStore>();
			services.AddTransient<IReportHandlerRepository, ReportHandlerRepository>();
			services.AddTransient<INotificationRepository, NotificationRepository>();
			services.AddTransient<ILookupCache, LookupCache>();
			services.AddTransient<IActionExecuteFactory, ActionExecuteFactoryManager>();
			services.AddTransient<IActionExecuteLogsRepository, ActionExecuteLogsRepository>();
		}
	}
}
