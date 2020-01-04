FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.0-buster-slim AS base
RUN apt-get update && apt-get install -y libgdiplus
RUN apt-get update && apt-get install libfontconfig1 nano -y

WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1.100-buster AS build
WORKDIR /src

ADD Quarkless.Analyser/References /app

COPY ["Quarkless.Services/Quarkless.Services.csproj", "Quarkless.Services/"]
COPY ["QuarklessLogic/QuarklessLogic.csproj", "QuarklessLogic/"]
COPY ["InstagramApiSharp/InstagramApiSharp.csproj", "InstagramApiSharp/"]
COPY ["QuarklessRepositories/QuarklessRepositories.csproj", "QuarklessRepositories/"]
COPY ["QuarklessContexts/QuarklessContexts.csproj", "QuarklessContexts/"]
COPY ["Quarkless.Interfacing/Quarkless.Interfacing.csproj", "Quarkless.Interfacing/"]
COPY ["Quarkless.Vision/Quarkless.Vision.csproj", "Quarkless.Vision/"]
COPY ["Quarkless.Analyser/Quarkless.Analyser.csproj", "Quarkless.Analyser/"]
COPY ["Quarkless.Common.Clients/Quarkless.Common.Clients.csproj", "Quarkless.Common.Clients/"]
COPY ["Quarkless.Security/Quarkless.Security.csproj", "Quarkless.Security/"]

RUN dotnet restore "Quarkless.Services/Quarkless.Services.csproj"

COPY . .
WORKDIR "/src/Quarkless.Services"
RUN dotnet build "Quarkless.Services.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "Quarkless.Services.csproj" -c Debug -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Quarkless.Services.dll"]