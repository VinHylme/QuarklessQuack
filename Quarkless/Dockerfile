FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src

COPY ["Quarkless/Quarkless.csproj", "Quarkless/"]
COPY ["QuarklessLogic/QuarklessLogic.csproj", "QuarklessLogic/"]
COPY ["InstagramApiSharp/InstagramApiSharp.csproj", "InstagramApiSharp/"]
COPY ["QuarklessRepositories/QuarklessRepositories.csproj", "QuarklessRepositories/"]
COPY ["QuarklessContexts/QuarklessContexts.csproj", "QuarklessContexts/"]
COPY ["Quarkless.Interfacing/Quarkless.Interfacing.csproj", "Quarkless.Interfacing/"]
COPY ["Quarkless.Analyser/Quarkless.Analyser.csproj", "Quarkless.Analyser/"]
COPY ["Quarkless.Vision/Quarkless.Vision.csproj", "Quarkless.Vision/"]
COPY ["Quarkless.Common.Clients/Quarkless.Common.Clients.csproj", "Quarkless.Common.Clients/"]

RUN dotnet restore "Quarkless/Quarkless.csproj"

COPY . .
WORKDIR "/src/Quarkless"
RUN dotnet build "Quarkless.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "Quarkless.csproj" -c Debug -o /app/publish

ENV ASPNETCORE_URLS http://+:51518
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Quarkless.dll"]
