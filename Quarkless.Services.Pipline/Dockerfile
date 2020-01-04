#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1.100-buster AS build
WORKDIR /src
COPY ["Quarkless.Services.Pipline/Quarkless.Services.Pipeline.csproj", "Quarkless.Services.Pipline/"]
COPY ["Quarkless.Common.Clients/Quarkless.Common.Clients.csproj", "Quarkless.Common.Clients/"]
COPY ["QuarklessLogic/QuarklessLogic.csproj", "QuarklessLogic/"]
COPY ["InstagramApiSharp/InstagramApiSharp.csproj", "InstagramApiSharp/"]
COPY ["QuarklessRepositories/QuarklessRepositories.csproj", "QuarklessRepositories/"]
COPY ["QuarklessContexts/QuarklessContexts.csproj", "QuarklessContexts/"]
COPY ["Quarkless.Interfacing/Quarkless.Interfacing.csproj", "Quarkless.Interfacing/"]
COPY ["Quarkless.Vision/Quarkless.Vision.csproj", "Quarkless.Vision/"]
COPY ["Quarkless.Analyser/Quarkless.Analyser.csproj", "Quarkless.Analyser/"]
COPY ["../Quarkless.Api.Operations/Quarkless.EmailServices.Repository/Quarkless.EmailServices.Repository.csproj", "../Quarkless.Api.Operations/Quarkless.EmailServices.Repository/"]
COPY ["../Quarkless.Api.Operations/Quarkless.Repository.MongoContext/Quarkless.Repository.MongoContext.csproj", "../Quarkless.Api.Operations/Quarkless.Repository.MongoContext/"]
COPY ["../Quarkless.Api.Operations/Quarkless.InstagramCreator.Repository/Quarkless.InstagramCreator.Repository.csproj", "../Quarkless.Api.Operations/Quarkless.InstagramCreator.Repository/"]
RUN dotnet restore "Quarkless.Services.Pipline/Quarkless.Services.Pipeline.csproj"
COPY . .
WORKDIR "/src/Quarkless.Services.Pipline"
RUN dotnet build "Quarkless.Services.Pipeline.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Quarkless.Services.Pipeline.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Quarkless.Services.Pipeline.dll"]