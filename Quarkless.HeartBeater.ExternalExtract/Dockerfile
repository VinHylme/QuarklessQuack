#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-nanoserver-1809 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-nanoserver-1809 AS build
WORKDIR /src
COPY ["Quarkless.HeartBeater.ExternalExtract/Quarkless.HeartBeater.ExternalExtract.csproj", "Quarkless.HeartBeater.ExternalExtract/"]
COPY ["Quarkless.HeartBeater/Quarkless.HeartBeater.csproj", "Quarkless.HeartBeater/"]
COPY ["Quarkless.HeartBeater.Interfaces/Quarkless.HeartBeater.Interfaces.csproj", "Quarkless.HeartBeater.Interfaces/"]
COPY ["QuarklessLogic/QuarklessLogic.csproj", "QuarklessLogic/"]
COPY ["InstagramApiSharp/InstagramApiSharp.csproj", "InstagramApiSharp/"]
COPY ["QuarklessRepositories/QuarklessRepositories.csproj", "QuarklessRepositories/"]
COPY ["QuarklessContexts/QuarklessContexts.csproj", "QuarklessContexts/"]
COPY ["Quarkless.Interfacing/Quarkless.Interfacing.csproj", "Quarkless.Interfacing/"]
COPY ["Quarkless.Analyser/Quarkless.Analyser.csproj", "Quarkless.Analyser/"]
COPY ["Quarkless.Services/Quarkless.Services.csproj", "Quarkless.Services/"]
COPY ["Quarkless.Common.Clients/Quarkless.Common.Clients.csproj", "Quarkless.Common.Clients/"]
COPY ["Quarkless.Security/Quarkless.Security.csproj", "Quarkless.Security/"]
RUN dotnet restore "Quarkless.HeartBeater.ExternalExtract/Quarkless.HeartBeater.ExternalExtract.csproj"
COPY . .
WORKDIR "/src/Quarkless.HeartBeater.ExternalExtract"
RUN dotnet build "Quarkless.HeartBeater.ExternalExtract.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "Quarkless.HeartBeater.ExternalExtract.csproj" -c Debug -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Quarkless.HeartBeater.ExternalExtract.dll"]