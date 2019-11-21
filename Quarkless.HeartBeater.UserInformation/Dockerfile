FROM mcr.microsoft.com/dotnet/core/runtime:2.2-stretch-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["Quarkless.HeartBeater.UserInformation/Quarkless.HeartBeater.UserInformation.csproj", "Quarkless.HeartBeater.UserInformation/"]
COPY ["Quarkless.HeartBeater/Quarkless.HeartBeater.csproj", "Quarkless.HeartBeater/"]
COPY ["Quarkless.Common/Quarkless.Common.csproj", "Quarkless.Common/"]
COPY ["QuarklessLogic/QuarklessLogic.csproj", "QuarklessLogic/"]
COPY ["InstagramApiSharp/InstagramApiSharp.csproj", "InstagramApiSharp/"]
COPY ["QuarklessRepositories/QuarklessRepositories.csproj", "QuarklessRepositories/"]
COPY ["QuarklessContexts/QuarklessContexts.csproj", "QuarklessContexts/"]
COPY ["Quarkless.Interfacing/Quarkless.Interfacing.csproj", "Quarkless.Interfacing/"]
COPY ["Quarkless.MediaAnalyser/Quarkless.MediaAnalyser.csproj", "Quarkless.MediaAnalyser/"]
COPY ["Quarkless.Services/Quarkless.Services.csproj", "Quarkless.Services/"]
RUN dotnet restore "Quarkless.HeartBeater.UserInformation/Quarkless.HeartBeater.UserInformation.csproj"
COPY . .
WORKDIR "/src/Quarkless.HeartBeater.UserInformation"
RUN dotnet build "Quarkless.HeartBeater.UserInformation.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Quarkless.HeartBeater.UserInformation.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Quarkless.HeartBeater.UserInformation.dll"]