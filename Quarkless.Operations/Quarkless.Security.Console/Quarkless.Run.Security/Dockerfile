FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1.100-buster AS build
WORKDIR /src
COPY ["Quarkless.Operations/Quarkless.Security.Console/Quarkless.Run.Security/Quarkless.Run.Security.csproj", "Quarkless.Operations/Quarkless.Security.Console/Quarkless.Run.Security/"]
COPY ["Quarkless.Projects/Quarkless.Logic.Security/Quarkless.Logic.Security.csproj", "Quarkless.Projects/Quarkless.Logic.Security/"]
COPY ["Quarkless.Operations/Quarkless.Models.Security/Quarkless.Models.Security.csproj", "Quarkless.Operations/Quarkless.Models.Security/"]
COPY ["Quarkless.Operations/Quarkless.Models.ClientSender/Quarkless.Models.ClientSender.csproj", "Quarkless.Operations/Quarkless.Models.ClientSender/"]
COPY ["Quarkless.Projects/Quarkless.Models.ApiLogger/Quarkless.Models.ApiLogger.csproj", "Quarkless.Projects/Quarkless.Models.ApiLogger/"]
COPY ["Quarkless.Projects/Quarkless.Models.Auth/Quarkless.Models.Auth.csproj", "Quarkless.Projects/Quarkless.Models.Auth/"]
COPY ["Quarkless.Projects/Quarkless.Models.Common/Quarkless.Models.Common.csproj", "Quarkless.Projects/Quarkless.Models.Common/"]
RUN dotnet restore "Quarkless.Operations/Quarkless.Security.Console/Quarkless.Run.Security/Quarkless.Run.Security.csproj"
COPY . .
WORKDIR "/src/Quarkless.Operations/Quarkless.Security.Console/Quarkless.Run.Security"
RUN dotnet build "Quarkless.Run.Security.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Quarkless.Run.Security.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Quarkless.Run.Security.dll"]