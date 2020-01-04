FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1.100-buster AS build
WORKDIR /src

COPY ["Quarkless.Services.Heartbeat/Quarkless.Services.Heartbeat.csproj", "Quarkless.Services.Heartbeat/"]
COPY ["Quarkless.Common.Clients/Quarkless.Common.Clients.csproj", "Quarkless.Common.Clients/"]
COPY ["QuarklessLogic/QuarklessLogic.csproj", "QuarklessLogic/"]
COPY ["InstagramApiSharp/InstagramApiSharp.csproj", "InstagramApiSharp/"]
COPY ["QuarklessRepositories/QuarklessRepositories.csproj", "QuarklessRepositories/"]
COPY ["QuarklessContexts/QuarklessContexts.csproj", "QuarklessContexts/"]
COPY ["Quarkless.Interfacing/Quarkless.Interfacing.csproj", "Quarkless.Interfacing/"]
COPY ["Quarkless.Vision/Quarkless.Vision.csproj", "Quarkless.Vision/"]
COPY ["Quarkless.Analyser/Quarkless.Analyser.csproj", "Quarkless.Analyser/"]

RUN dotnet restore "Quarkless.Services.Heartbeat/Quarkless.Services.Heartbeat.csproj"

COPY . .
WORKDIR "/src/Quarkless.Services.Heartbeat"
RUN dotnet build "Quarkless.Services.Heartbeat.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Quarkless.Services.Heartbeat.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Quarkless.Services.Heartbeat.dll"]