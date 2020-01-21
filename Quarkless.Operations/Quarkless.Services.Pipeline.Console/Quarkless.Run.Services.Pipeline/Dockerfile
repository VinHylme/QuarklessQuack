FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1.100-buster AS build
WORKDIR /src
COPY ["Quarkless.Operations/Quarkless.Services.Pipeline.Console/Quarkless.Run.Services.Pipeline/Quarkless.Run.Services.Pipeline.csproj", "Quarkless.Operations/Quarkless.Services.Pipeline.Console/Quarkless.Run.Services.Pipeline/"]
COPY ["Quarkless.Operations/Quarkless.Logic.Services.Pipeline/Quarkless.Logic.Services.Pipeline.csproj", "Quarkless.Operations/Quarkless.Logic.Services.Pipeline/"]
COPY ["Quarkless.Operations/Quarkless.Models.Services.Pipeline/Quarkless.Models.Services.Pipeline.csproj", "Quarkless.Operations/Quarkless.Models.Services.Pipeline/"]
COPY ["Quarkless.Projects/Quarkless.Models.Shared/Quarkless.Models.Shared.csproj", "Quarkless.Projects/Quarkless.Models.Shared/"]
RUN dotnet restore "Quarkless.Operations/Quarkless.Services.Pipeline.Console/Quarkless.Run.Services.Pipeline/Quarkless.Run.Services.Pipeline.csproj"
COPY . .
WORKDIR "/src/Quarkless.Operations/Quarkless.Services.Pipeline.Console/Quarkless.Run.Services.Pipeline"
RUN dotnet build "Quarkless.Run.Services.Pipeline.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Quarkless.Run.Services.Pipeline.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Quarkless.Run.Services.Pipeline.dll"]