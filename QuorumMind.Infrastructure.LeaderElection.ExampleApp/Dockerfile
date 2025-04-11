FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["QuorumMind.Infrastructure.LeaderElection.ExampleApp/QuorumMind.Infrastructure.LeaderElection.ExampleApp.csproj", "QuorumMind.Infrastructure.LeaderElection.ExampleApp/"]
COPY ["src/QuorumMind.Infrastructure.LeaderElection.Redis/QuorumMind.Infrastructure.LeaderElection.Redis.csproj", "src/QuorumMind.Infrastructure.LeaderElection.Redis/"]
COPY ["src/QuorumMind.Infrastructure.LeaderElection.Consul/QuorumMind.Infrastructure.LeaderElection.Consul.csproj", "src/QuorumMind.Infrastructure.LeaderElection.Consul/"]
COPY ["src/QuorumMind.Infrastructure.LeaderElection/QuorumMind.Infrastructure.LeaderElection.csproj", "src/QuorumMind.Infrastructure.LeaderElection/"]
RUN dotnet restore "QuorumMind.Infrastructure.LeaderElection.ExampleApp/QuorumMind.Infrastructure.LeaderElection.ExampleApp.csproj"
COPY . .
WORKDIR "/src/QuorumMind.Infrastructure.LeaderElection.ExampleApp"
RUN dotnet build "QuorumMind.Infrastructure.LeaderElection.ExampleApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "QuorumMind.Infrastructure.LeaderElection.ExampleApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QuorumMind.Infrastructure.LeaderElection.ExampleApp.dll"]
