FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base

RUN apt-get update -y
RUN apt-get install -y gconf-service libasound2 libatk1.0-0 libc6 libcairo2 libcups2 libdbus-1-3 libexpat1 libgdiplus libfontconfig1 libgcc1 libgconf-2-4 libgdk-pixbuf2.0-0 libglib2.0-0 libgtk-3-0 libnspr4 libpango-1.0-0 libpangocairo-1.0-0 libstdc++6 libx11-6 libx11-xcb1 libxcb1 libxcomposite1 libxcursor1 libxdamage1 libxext6 libxfixes3 libxi6 libxrandr2 libxrender1 libxss1 libxtst6 ca-certificates fonts-liberation libappindicator1 libnss3 lsb-release xdg-utils wget

WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Quarkless.Operations/Quarkless.Services.Automation.Console/Quarkless.Run.Services.Automation/Quarkless.Run.Services.Automation.csproj", "Quarkless.Operations/Quarkless.Services.Automation.Console/Quarkless.Run.Services.Automation/"]
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
COPY ["Quarkless.Projects/Quarkless.Base.Timeline/Quarkless.Base.Timeline.csproj", "Quarkless.Projects/Quarkless.Base.Timeline/"]
COPY ["Quarkless.Projects/Quarkless.Base.Lookup/Quarkless.Base.Lookup.csproj", "Quarkless.Projects/Quarkless.Base.Lookup/"]
COPY ["Quarkless.Projects/Quarkless.Base.Media/Quarkless.Base.Media.csproj", "Quarkless.Projects/Quarkless.Base.Media/"]
COPY ["Quarkless.Projects/Quarkless.Analyser/Quarkless.Analyser.csproj", "Quarkless.Projects/Quarkless.Analyser/"]
COPY ["Quarkless.Projects/Quarkless.Base.Actions/Quarkless.Base.Actions.csproj", "Quarkless.Projects/Quarkless.Base.Actions/"]
COPY ["Quarkless.Projects/Quarkless.Base.ResponseResolver/Quarkless.Base.ResponseResolver.csproj", "Quarkless.Projects/Quarkless.Base.ResponseResolver/"]
COPY ["Quarkless.Projects/Quarkless.Base.Notification/Quarkless.Base.Notification.csproj", "Quarkless.Projects/Quarkless.Base.Notification/"]
COPY ["Quarkless.Projects/Quarkless.Base.Stories/Quarkless.Base.Stories.csproj", "Quarkless.Projects/Quarkless.Base.Stories/"]
COPY ["Quarkless.Projects/Quarkless.Base.InstagramUser/Quarkless.Base.InstagramUser.csproj", "Quarkless.Projects/Quarkless.Base.InstagramUser/"]
COPY ["Quarkless.Projects/Quarkless.Base.Messaging/Quarkless.Base.Messaging.csproj", "Quarkless.Projects/Quarkless.Base.Messaging/"]
COPY ["Quarkless.Projects/Quarkless.Base.InstagramSearch/Quarkless.Base.InstagramSearch.csproj", "Quarkless.Projects/Quarkless.Base.InstagramSearch/"]
COPY ["Quarkless.Projects/Quarkless.Base.HashtagGenerator/Quarkless.Base.HashtagGenerator.csproj", "Quarkless.Projects/Quarkless.Base.HashtagGenerator/"]
COPY ["Quarkless.Projects/Quarkless.Base.Hashtag/Quarkless.Base.Hashtag.csproj", "Quarkless.Projects/Quarkless.Base.Hashtag/"]
COPY ["Quarkless.Projects/Quarkless.Vision/Quarkless.Vision.csproj", "Quarkless.Projects/Quarkless.Vision/"]
COPY ["Quarkless.Projects/Quarkless.Base.ContentSearch/Quarkless.Base.ContentSearch.csproj", "Quarkless.Projects/Quarkless.Base.ContentSearch/"]
COPY ["Quarkless.Projects/Quarkless.Base.PuppeteerClient/Quarkless.Base.PuppeteerClient.csproj", "Quarkless.Projects/Quarkless.Base.PuppeteerClient/"]
COPY ["Quarkless.Projects/Quarkless.Base.RestSharpClientManager/Quarkless.Base.RestSharpClientManager.csproj", "Quarkless.Projects/Quarkless.Base.RestSharpClientManager/"]
COPY ["Quarkless.Projects/Quarkless.Base.Topic/Quarkless.Base.Topic.csproj", "Quarkless.Projects/Quarkless.Base.Topic/"]
COPY ["Quarkless.Projects/Quarkless.Base.WorkerManager/Quarkless.Base.WorkerManager.csproj", "Quarkless.Projects/Quarkless.Base.WorkerManager/"]
COPY ["Quarkless.Projects/Quarkless.Common.Timeline/Quarkless.Common.Timeline.csproj", "Quarkless.Projects/Quarkless.Common.Timeline/"]
COPY ["Quarkless.Projects/Quarkless.Base.Library/Quarkless.Base.Library.csproj", "Quarkless.Projects/Quarkless.Base.Library/"]
COPY ["Quarkless.Projects/Quarkless.Base.Storage/Quarkless.Base.Storage.csproj", "Quarkless.Projects/Quarkless.Base.Storage/"]
COPY ["Quarkless.Projects/Quarkless.Base.Heartbeat/Quarkless.Base.Heartbeat.csproj", "Quarkless.Projects/Quarkless.Base.Heartbeat/"]
COPY ["Quarkless.Projects/Quarkless.Base.TextGenerator/Quarkless.Base.TextGenerator.csproj", "Quarkless.Projects/Quarkless.Base.TextGenerator/"]
COPY ["Quarkless.Projects/Quarkless.Base.ContentInfo/Quarkless.Base.ContentInfo.csproj", "Quarkless.Projects/Quarkless.Base.ContentInfo/"]
COPY ["Quarkless.Projects/Quarkless.Base.Utilities/Quarkless.Base.Utilities.csproj", "Quarkless.Projects/Quarkless.Base.Utilities/"]
COPY ["Quarkless.Projects/Quarkless.Base.RequestBuilder/Quarkless.Base.RequestBuilder.csproj", "Quarkless.Projects/Quarkless.Base.RequestBuilder/"]
COPY ["Quarkless.Projects/Quarkless.Models.Shared.Api/Quarkless.Models.Shared.Api.csproj", "Quarkless.Projects/Quarkless.Models.Shared.Api/"]
COPY ["Quarkless.Projects/Quarkless.Base.Auth/Quarkless.Base.Auth.csproj", "Quarkless.Projects/Quarkless.Base.Auth/"]
COPY ["Quarkless.Projects/Quarkless.Models.Shared/Quarkless.Models.Shared.csproj", "Quarkless.Projects/Quarkless.Models.Shared/"]
COPY ["Quarkless.Projects/Quarkless.Base.ApiLogger/Quarkless.Base.ApiLogger.csproj", "Quarkless.Projects/Quarkless.Base.ApiLogger/"]
COPY ["Quarkless.Projects/Quarkless.Base.Agent/Quarkless.Base.Agent.csproj", "Quarkless.Projects/Quarkless.Base.Agent/"]
COPY ["Quarkless.Projects/Quarkless.Base.AuthDetails/Quarkless.Base.AuthDetails.csproj", "Quarkless.Projects/Quarkless.Base.AuthDetails/"]
RUN dotnet restore "Quarkless.Operations/Quarkless.Services.Automation.Console/Quarkless.Run.Services.Automation/Quarkless.Run.Services.Automation.csproj"
COPY . .
WORKDIR "/src/Quarkless.Operations/Quarkless.Services.Automation.Console/Quarkless.Run.Services.Automation"
RUN dotnet build "Quarkless.Run.Services.Automation.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Quarkless.Run.Services.Automation.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Quarkless.Run.Services.Automation.dll"]