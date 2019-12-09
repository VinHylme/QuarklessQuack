#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["Quarkless.Security/Quarkless.Security.csproj", "Quarkless.Security/"]
COPY ["QuarklessContexts/QuarklessContexts.csproj", "QuarklessContexts/"]
COPY ["InstagramApiSharp/InstagramApiSharp.csproj", "InstagramApiSharp/"]
RUN dotnet restore "Quarkless.Security/Quarkless.Security.csproj"
COPY . .
WORKDIR "/src/Quarkless.Security"

RUN dotnet build "Quarkless.Security.csproj" -c Debug -o /app/build
FROM build AS publish
RUN dotnet publish "Quarkless.Security.csproj" -c Debug -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Quarkless.Security.dll"]