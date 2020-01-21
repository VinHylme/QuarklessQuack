FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1.100-buster AS build
WORKDIR /src

COPY ["Quarkless.Operations/Quarkless.Services.Heartbeat.Console/Quarkless.Run.Services.Heartbeat/Quarkless.Run.Services.Heartbeat.csproj", "Quarkless.Operations/Quarkless.Services.Heartbeat.Console/Quarkless.Run.Services.Heartbeat/"]
COPY ["Quarkless.Projects/Quarkless.Base.AccountOptions/Quarkless.Base.AccountOptions.csproj", "Quarkless.Projects/Quarkless.Base.AccountOptions/"]
COPY ["InstagramApiSharp/InstagramApiSharp.csproj", "InstagramApiSharp/"]
COPY ["Quarkless.Projects/Quarkless.Models.ReportHandler/Quarkless.Models.ReportHandler.csproj", "Quarkless.Projects/Quarkless.Models.ReportHandler/"]
COPY ["Quarkless.Projects/Quarkless.Models.InstagramClient/Quarkless.Models.InstagramClient.csproj", "Quarkless.Projects/Quarkless.Models.InstagramClient/"]
COPY ["Quarkless.Projects/Quarkless.Models.Profile/Quarkless.Models.Profile.csproj", "Quarkless.Projects/Quarkless.Models.Profile/"]
COPY ["Quarkless.Projects/Quarkless.Models.Topic/Quarkless.Models.Topic.csproj", "Quarkless.Projects/Quarkless.Models.Topic/"]
COPY ["Quarkless.Projects/Quarkless.Models.Common/Quarkless.Models.Common.csproj", "Quarkless.Projects/Quarkless.Models.Common/"]
COPY ["Quarkless.Projects/Quarkless.Models.SearchResponse/Quarkless.Models.SearchResponse.csproj", "Quarkless.Projects/Quarkless.Models.SearchResponse/"]
COPY ["Quarkless.Projects/Quarkless.Models.Proxy/Quarkless.Models.Proxy.csproj", "Quarkless.Projects/Quarkless.Models.Proxy/"]
COPY ["Quarkless.Projects/Quarkless.Models.InstagramAccounts/Quarkless.Models.InstagramAccounts.csproj", "Quarkless.Projects/Quarkless.Models.InstagramAccounts/"]
COPY ["Quarkless.Projects/Quarkless.Logic.Hashtag/Quarkless.Logic.Hashtag.csproj", "Quarkless.Projects/Quarkless.Logic.Hashtag/"]
COPY ["Quarkless.Projects/Quarkless.Models.Hashtag/Quarkless.Models.Hashtag.csproj", "Quarkless.Projects/Quarkless.Models.Hashtag/"]
COPY ["Quarkless.Projects/Quarkless.Repository.Topic/Quarkless.Repository.Topic.csproj", "Quarkless.Projects/Quarkless.Repository.Topic/"]
COPY ["Quarkless.Projects/Quarkless.Repository.MongoContext/Quarkless.Repository.MongoContext.csproj", "Quarkless.Projects/Quarkless.Repository.MongoContext/"]
COPY ["Quarkless.Projects/Quarkless.Logic.InstagramAccounts/Quarkless.Logic.InstagramAccounts.csproj", "Quarkless.Projects/Quarkless.Logic.InstagramAccounts/"]
COPY ["Quarkless.Projects/Quarkless.Events/Quarkless.Events.csproj", "Quarkless.Projects/Quarkless.Events/"]
COPY ["Quarkless.Projects/Quarkless.Repository.InstagramAccounts/Quarkless.Repository.InstagramAccounts.csproj", "Quarkless.Projects/Quarkless.Repository.InstagramAccounts/"]
COPY ["Quarkless.Projects/Quarkless.Repository.RedisClientContext/Quarkless.Repository.RedisContext.csproj", "Quarkless.Projects/Quarkless.Repository.RedisClientContext/"]
COPY ["Quarkless.Projects/Quarkless.Logic.Lookup/Quarkless.Logic.Lookup.csproj", "Quarkless.Projects/Quarkless.Logic.Lookup/"]
COPY ["Quarkless.Projects/Quarkless.Models.Lookup/Quarkless.Models.Lookup.csproj", "Quarkless.Projects/Quarkless.Models.Lookup/"]
COPY ["Quarkless.Projects/Quarkless.Repository.Heartbeat/Quarkless.Repository.Heartbeat.csproj", "Quarkless.Projects/Quarkless.Repository.Heartbeat/"]
COPY ["Quarkless.Projects/Quarkless.Models.Heartbeat/Quarkless.Models.Heartbeat.csproj", "Quarkless.Projects/Quarkless.Models.Heartbeat/"]
COPY ["Quarkless.Projects/Quarkless.Repository.Hashtag/Quarkless.Repository.Hashtag.csproj", "Quarkless.Projects/Quarkless.Repository.Hashtag/"]
COPY ["Quarkless.Operations/Quarkless.Models.Services.Heartbeat/Quarkless.Models.Services.Heartbeat.csproj", "Quarkless.Operations/Quarkless.Models.Services.Heartbeat/"]
COPY ["Quarkless.Projects/Quarkless.Logic.Heartbeat/Quarkless.Logic.Heartbeat.csproj", "Quarkless.Projects/Quarkless.Logic.Heartbeat/"]
COPY ["Quarkless.Projects/Quarkless.Logic.Topic/Quarkless.Logic.Topic.csproj", "Quarkless.Projects/Quarkless.Logic.Topic/"]
COPY ["Quarkless.Projects/Quarkless.Models.WorkerManager/Quarkless.Models.WorkerManager.csproj", "Quarkless.Projects/Quarkless.Models.WorkerManager/"]
COPY ["Quarkless.Projects/Quarkless.Models.ResponseResolver/Quarkless.Models.ResponseResolver.csproj", "Quarkless.Projects/Quarkless.Models.ResponseResolver/"]
COPY ["Quarkless.Projects/Quarkless.Models.ContentSearch/Quarkless.Models.ContentSearch.csproj", "Quarkless.Projects/Quarkless.Models.ContentSearch/"]
COPY ["Quarkless.Projects/Quarkless.Base.InstagramComments/Quarkless.Base.InstagramComments.csproj", "Quarkless.Projects/Quarkless.Base.InstagramComments/"]
COPY ["Quarkless.Projects/Quarkless.Repository.ReportHandler/Quarkless.Repository.ReportHandler.csproj", "Quarkless.Projects/Quarkless.Repository.ReportHandler/"]
COPY ["Quarkless.Projects/Quarkless.Repository.Auth/Quarkless.Repository.Auth.csproj", "Quarkless.Projects/Quarkless.Repository.Auth/"]
COPY ["Quarkless.Projects/Quarkless.Models.Auth/Quarkless.Models.Auth.csproj", "Quarkless.Projects/Quarkless.Models.Auth/"]
COPY ["Quarkless.Projects/Quarkless.Repository.Timeline/Quarkless.Repository.Timeline.csproj", "Quarkless.Projects/Quarkless.Repository.Timeline/"]
COPY ["Quarkless.Projects/Quarkless.Models.Timeline/Quarkless.Models.Timeline.csproj", "Quarkless.Projects/Quarkless.Models.Timeline/"]
COPY ["Quarkless.Projects/Quarkless.Models.Messaging/Quarkless.Models.Messaging.csproj", "Quarkless.Projects/Quarkless.Models.Messaging/"]
COPY ["Quarkless.Projects/Quarkless.Models.Library/Quarkless.Models.Library.csproj", "Quarkless.Projects/Quarkless.Models.Library/"]
COPY ["Quarkless.Projects/Quarkless.Logic.SeleniumClient/Quarkless.Logic.SeleniumClient.csproj", "Quarkless.Projects/Quarkless.Logic.SeleniumClient/"]
COPY ["Quarkless.Projects/Quarkless.Models.SeleniumClient/Quarkless.Models.SeleniumClient.csproj", "Quarkless.Projects/Quarkless.Models.SeleniumClient/"]
COPY ["Quarkless.Projects/Quarkless.Logic.Profile/Quarkless.Logic.Profile.csproj", "Quarkless.Projects/Quarkless.Logic.Profile/"]
COPY ["Quarkless.Projects/Quarkless.Logic.ResponseResolver/Quarkless.Logic.ResponseResolver.csproj", "Quarkless.Projects/Quarkless.Logic.ResponseResolver/"]
COPY ["Quarkless.Projects/Quarkless.Logic.Timeline/Quarkless.Logic.Timeline.csproj", "Quarkless.Projects/Quarkless.Logic.Timeline/"]
COPY ["Quarkless.Projects/Quarkless.Models.Media/Quarkless.Models.Media.csproj", "Quarkless.Projects/Quarkless.Models.Media/"]
COPY ["Quarkless.Projects/Quarkless.Models.RestSharpClientManager/Quarkless.Models.RestSharpClientManager.csproj", "Quarkless.Projects/Quarkless.Models.RestSharpClientManager/"]
COPY ["Quarkless.Projects/Quarkless.Models.Storage/Quarkless.Models.Storage.csproj", "Quarkless.Projects/Quarkless.Models.Storage/"]
COPY ["Quarkless.Projects/Quarkless.Models.RequestBuilder/Quarkless.Models.RequestBuilder.csproj", "Quarkless.Projects/Quarkless.Models.RequestBuilder/"]
COPY ["Quarkless.Projects/Quarkless.Analyser/Quarkless.Analyser.csproj", "Quarkless.Projects/Quarkless.Analyser/"]
COPY ["Quarkless.Projects/Quarkless.Logic.ReportHandler/Quarkless.Logic.ReportHandler.csproj", "Quarkless.Projects/Quarkless.Logic.ReportHandler/"]
COPY ["Quarkless.Projects/Quarkless.Repository.Profile/Quarkless.Repository.Profile.csproj", "Quarkless.Projects/Quarkless.Repository.Profile/"]
COPY ["Quarkless.Projects/Quarkless.Logic.RestSharpClientManager/Quarkless.Logic.RestSharpClientManager.csproj", "Quarkless.Projects/Quarkless.Logic.RestSharpClientManager/"]
COPY ["Quarkless.Projects/Quarkless.Models.Shared/Quarkless.Models.Shared.csproj", "Quarkless.Projects/Quarkless.Models.Shared/"]
COPY ["Quarkless.Projects/Quarkless.Repository.Lookup/Quarkless.Repository.Lookup.csproj", "Quarkless.Projects/Quarkless.Repository.Lookup/"]
COPY ["Quarkless.Projects/Quarkless.Logic.ContentSearch/Quarkless.Logic.ContentSearch.csproj", "Quarkless.Projects/Quarkless.Logic.ContentSearch/"]
COPY ["Quarkless.Projects/Quarkless.Repository.ContentSearch/Quarkless.Repository.ContentSearch.csproj", "Quarkless.Projects/Quarkless.Repository.ContentSearch/"]
COPY ["Quarkless.Operations/Quarkless.Logic.Services.Heartbeat/Quarkless.Logic.Services.Heartbeat.csproj", "Quarkless.Operations/Quarkless.Logic.Services.Heartbeat/"]
COPY ["Quarkless.Projects/Quarkless.Logic.WorkerManager/Quarkless.Logic.WorkerManager.csproj", "Quarkless.Projects/Quarkless.Logic.WorkerManager/"]
COPY ["Quarkless.Projects/Quarkless.Logic.InstagramClient/Quarkless.Logic.InstagramClient.csproj", "Quarkless.Projects/Quarkless.Logic.InstagramClient/"]
COPY ["Quarkless.Projects/Quarkless.Logic.Proxy/Quarkless.Logic.Proxy.csproj", "Quarkless.Projects/Quarkless.Logic.Proxy/"]
COPY ["Quarkless.Projects/Quarkless.Repository.Proxy/Quarkless.Repository.Proxy.csproj", "Quarkless.Projects/Quarkless.Repository.Proxy/"]
COPY ["Quarkless.Projects/Quarkless.Base.ContentSearch/Quarkless.Base.ContentSearch.csproj", "Quarkless.Projects/Quarkless.Base.ContentSearch/"]
COPY ["Quarkless.Projects/Quarkless.Models.InstagramSearch/Quarkless.Models.InstagramSearch.csproj", "Quarkless.Projects/Quarkless.Models.InstagramSearch/"]
COPY ["Quarkless.Projects/Quarkless.Logic.InstagramSearch/Quarkless.Logic.InstagramSearch.csproj", "Quarkless.Projects/Quarkless.Logic.InstagramSearch/"]
RUN dotnet restore "Quarkless.Operations/Quarkless.Services.Heartbeat.Console/Quarkless.Run.Services.Heartbeat/Quarkless.Run.Services.Heartbeat.csproj"
COPY . .
WORKDIR "/src/Quarkless.Operations/Quarkless.Services.Heartbeat.Console/Quarkless.Run.Services.Heartbeat"
RUN dotnet build "Quarkless.Run.Services.Heartbeat.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Quarkless.Run.Services.Heartbeat.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Quarkless.Run.Services.Heartbeat.dll"]