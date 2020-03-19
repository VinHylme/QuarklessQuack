FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Quarkless.Operations/Quarkless.Services.DataFetcher.Console/Quarkless.Run.Services.DataFetcher/Quarkless.Run.Services.DataFetcher.csproj", "Quarkless.Operations/Quarkless.Services.DataFetcher.Console/Quarkless.Run.Services.DataFetcher/"]
COPY ["Quarkless.Projects/Quarkless.Repository.MongoContext/Quarkless.Repository.MongoContext.csproj", "Quarkless.Projects/Quarkless.Repository.MongoContext/"]
COPY ["Quarkless.Projects/Quarkless.Base.InstagramComments/Quarkless.Base.InstagramComments.csproj", "Quarkless.Projects/Quarkless.Base.InstagramComments/"]
COPY ["Quarkless.Projects/Quarkless.Models.Common/Quarkless.Models.Common.csproj", "Quarkless.Projects/Quarkless.Models.Common/"]
COPY ["Quarkless.Projects/Quarkless.Base.ReportHandler/Quarkless.Base.ReportHandler.csproj", "Quarkless.Projects/Quarkless.Base.ReportHandler/"]
COPY ["Quarkless.Projects/Quarkless.Repository.RedisClientContext/Quarkless.Repository.RedisContext.csproj", "Quarkless.Projects/Quarkless.Repository.RedisClientContext/"]
COPY ["InstagramApiSharp/InstagramApiSharp.csproj", "InstagramApiSharp/"]
COPY ["Quarkless.Projects/Quarkless.Base.InstagramClient/Quarkless.Base.InstagramClient.csproj", "Quarkless.Projects/Quarkless.Base.InstagramClient/"]
COPY ["Quarkless.Projects/Quarkless.Base.Profile/Quarkless.Base.Profile.csproj", "Quarkless.Projects/Quarkless.Base.Profile/"]
COPY ["Quarkless.Projects/Quarkless.Events/Quarkless.Events.csproj", "Quarkless.Projects/Quarkless.Events/"]
COPY ["Quarkless.Projects/Quarkless.Models.SearchResponse/Quarkless.Models.SearchResponse.csproj", "Quarkless.Projects/Quarkless.Models.SearchResponse/"]
COPY ["Quarkless.Projects/Quarkless.Base.Proxy/Quarkless.Base.Proxy.csproj", "Quarkless.Projects/Quarkless.Base.Proxy/"]
COPY ["Quarkless.Projects/Quarkless.Geolocation/Quarkless.Geolocation.csproj", "Quarkless.Projects/Quarkless.Geolocation/"]
COPY ["Quarkless.Projects/Quarkless.Base.InstagramAccounts/Quarkless.Base.InstagramAccounts.csproj", "Quarkless.Projects/Quarkless.Base.InstagramAccounts/"]
COPY ["Quarkless.Projects/Quarkless.Base.Auth.Common/Quarkless.Base.Auth.Common.csproj", "Quarkless.Projects/Quarkless.Base.Auth.Common/"]
COPY ["Quarkless.Projects/Quarkless.Base.Hashtag/Quarkless.Base.Hashtag.csproj", "Quarkless.Projects/Quarkless.Base.Hashtag/"]
COPY ["Quarkless.Projects/Quarkless.Base.Topic/Quarkless.Base.Topic.csproj", "Quarkless.Projects/Quarkless.Base.Topic/"]
COPY ["Quarkless.Projects/Quarkless.Base.ContentSearch/Quarkless.Base.ContentSearch.csproj", "Quarkless.Projects/Quarkless.Base.ContentSearch/"]
COPY ["Quarkless.Projects/Quarkless.Base.InstagramSearch/Quarkless.Base.InstagramSearch.csproj", "Quarkless.Projects/Quarkless.Base.InstagramSearch/"]
COPY ["Quarkless.Projects/Quarkless.Base.ResponseResolver/Quarkless.Base.ResponseResolver.csproj", "Quarkless.Projects/Quarkless.Base.ResponseResolver/"]
COPY ["Quarkless.Projects/Quarkless.Base.Lookup/Quarkless.Base.Lookup.csproj", "Quarkless.Projects/Quarkless.Base.Lookup/"]
COPY ["Quarkless.Projects/Quarkless.Base.Notification/Quarkless.Base.Notification.csproj", "Quarkless.Projects/Quarkless.Base.Notification/"]
COPY ["Quarkless.Projects/Quarkless.Base.Media/Quarkless.Base.Media.csproj", "Quarkless.Projects/Quarkless.Base.Media/"]
COPY ["Quarkless.Projects/Quarkless.Analyser/Quarkless.Analyser.csproj", "Quarkless.Projects/Quarkless.Analyser/"]
COPY ["Quarkless.Projects/Quarkless.Base.Stories/Quarkless.Base.Stories.csproj", "Quarkless.Projects/Quarkless.Base.Stories/"]
COPY ["Quarkless.Projects/Quarkless.Base.InstagramUser/Quarkless.Base.InstagramUser.csproj", "Quarkless.Projects/Quarkless.Base.InstagramUser/"]
COPY ["Quarkless.Projects/Quarkless.Base.Messaging/Quarkless.Base.Messaging.csproj", "Quarkless.Projects/Quarkless.Base.Messaging/"]
COPY ["Quarkless.Projects/Quarkless.Base.PuppeteerClient/Quarkless.Base.PuppeteerClient.csproj", "Quarkless.Projects/Quarkless.Base.PuppeteerClient/"]
COPY ["Quarkless.Projects/Quarkless.Base.RestSharpClientManager/Quarkless.Base.RestSharpClientManager.csproj", "Quarkless.Projects/Quarkless.Base.RestSharpClientManager/"]
COPY ["Quarkless.Projects/Quarkless.Base.WorkerManager/Quarkless.Base.WorkerManager.csproj", "Quarkless.Projects/Quarkless.Base.WorkerManager/"]
COPY ["Quarkless.Projects/Quarkless.Base.Heartbeat/Quarkless.Base.Heartbeat.csproj", "Quarkless.Projects/Quarkless.Base.Heartbeat/"]
COPY ["Quarkless.Projects/Quarkless.Base.Utilities/Quarkless.Base.Utilities.csproj", "Quarkless.Projects/Quarkless.Base.Utilities/"]
COPY ["Quarkless.Projects/Quarkless.Base.HashtagGenerator/Quarkless.Base.HashtagGenerator.csproj", "Quarkless.Projects/Quarkless.Base.HashtagGenerator/"]
COPY ["Quarkless.Projects/Quarkless.Vision/Quarkless.Vision.csproj", "Quarkless.Projects/Quarkless.Vision/"]
COPY ["Quarkless.Projects/Quarkless.Base.TextGenerator/Quarkless.Base.TextGenerator.csproj", "Quarkless.Projects/Quarkless.Base.TextGenerator/"]
COPY ["Quarkless.Projects/Quarkless.Base.AuthDetails/Quarkless.Base.AuthDetails.csproj", "Quarkless.Projects/Quarkless.Base.AuthDetails/"]
COPY ["Quarkless.Projects/Quarkless.Models.Shared/Quarkless.Models.Shared.csproj", "Quarkless.Projects/Quarkless.Models.Shared/"]
RUN dotnet restore "Quarkless.Operations/Quarkless.Services.DataFetcher.Console/Quarkless.Run.Services.DataFetcher/Quarkless.Run.Services.DataFetcher.csproj"
COPY . .
WORKDIR "/src/Quarkless.Operations/Quarkless.Services.DataFetcher.Console/Quarkless.Run.Services.DataFetcher"
RUN dotnet build "Quarkless.Run.Services.DataFetcher.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Quarkless.Run.Services.DataFetcher.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Quarkless.Run.Services.DataFetcher.dll"]