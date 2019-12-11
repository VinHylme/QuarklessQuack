FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["Quarkless.Services.DataFetcher/Quarkless.Services.DataFetcher.csproj", "Quarkless.Services.DataFetcher/"]
COPY ["QuarklessContexts/QuarklessContexts.csproj", "QuarklessContexts/"]
COPY ["InstagramApiSharp/InstagramApiSharp.csproj", "InstagramApiSharp/"]
COPY ["Quarkless.Common.Clients/Quarkless.Common.Clients.csproj", "Quarkless.Common.Clients/"]
COPY ["QuarklessLogic/QuarklessLogic.csproj", "QuarklessLogic/"]
COPY ["QuarklessRepositories/QuarklessRepositories.csproj", "QuarklessRepositories/"]
COPY ["Quarkless.Interfacing/Quarkless.Interfacing.csproj", "Quarkless.Interfacing/"]
COPY ["Quarkless.Analyser/Quarkless.Analyser.csproj", "Quarkless.Analyser/"]
RUN dotnet restore "Quarkless.Services.DataFetcher/Quarkless.Services.DataFetcher.csproj"
COPY . .
WORKDIR "/src/Quarkless.Services.DataFetcher"
RUN dotnet build "Quarkless.Services.DataFetcher.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "Quarkless.Services.DataFetcher.csproj" -c Debug -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Quarkless.Services.DataFetcher.dll"]