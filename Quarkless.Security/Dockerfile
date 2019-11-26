FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-nanoserver-1809 AS base
WORKDIR /app
EXPOSE 65115/tcp

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-nanoserver-1809 AS build
WORKDIR /src
COPY ["Quarkless.Security/Quarkless.Security.csproj", "Quarkless.Security/"]
COPY ["QuarklessContexts/QuarklessContexts.csproj", "QuarklessContexts/"]
COPY ["InstagramApiSharp/InstagramApiSharp.csproj", "InstagramApiSharp/"]
RUN dotnet restore "Quarkless.Security/Quarkless.Security.csproj"
COPY . .
WORKDIR "/src/Quarkless.Security"
RUN dotnet build "Quarkless.Security.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Quarkless.Security.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Quarkless.Security.dll"]