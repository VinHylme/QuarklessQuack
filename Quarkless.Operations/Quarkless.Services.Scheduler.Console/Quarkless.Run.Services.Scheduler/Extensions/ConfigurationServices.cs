using System;
using System.Runtime.InteropServices;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Analyser;
using Quarkless.Analyser.Models;
using Quarkless.Events;
using Quarkless.Events.Interfaces;
using Quarkless.Geolocation;
using Quarkless.Logic.Actions.Factory.ActionExecute.Manager;
using Quarkless.Logic.InstagramAccounts;
using Quarkless.Logic.InstagramClient;
using Quarkless.Logic.Lookup;
using Quarkless.Logic.Notification;
using Quarkless.Logic.Profile;
using Quarkless.Logic.Proxy;
using Quarkless.Logic.ReportHandler;
using Quarkless.Logic.ResponseResolver;
using Quarkless.Logic.Timeline.TaskScheduler;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Auth;
using Quarkless.Models.Auth.Interfaces;
using Quarkless.Models.InstagramAccounts.Interfaces;
using Quarkless.Models.InstagramClient.Interfaces;
using Quarkless.Models.Lookup.Interfaces;
using Quarkless.Models.Notification.Interfaces;
using Quarkless.Models.Profile.Interfaces;
using Quarkless.Models.Proxy;
using Quarkless.Models.Proxy.Interfaces;
using Quarkless.Models.ReportHandler.Interfaces;
using Quarkless.Models.ResponseResolver.Interfaces;
using Quarkless.Models.Shared.Extensions;
using Quarkless.Models.Timeline.Interfaces;
using Quarkless.Models.Timeline.Interfaces.TaskScheduler;
using Quarkless.Models.Timeline.TaskScheduler;
using Quarkless.Repository.InstagramAccounts.Mongo;
using Quarkless.Repository.Lookup;
using Quarkless.Repository.MongoContext;
using Quarkless.Repository.MongoContext.Models;
using Quarkless.Repository.Notification;
using Quarkless.Repository.Profile;
using Quarkless.Repository.Proxy;
using Quarkless.Repository.RedisContext;
using Quarkless.Repository.RedisContext.Models;
using Quarkless.Repository.ReportHandler;
using Quarkless.Repository.Timeline;

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
