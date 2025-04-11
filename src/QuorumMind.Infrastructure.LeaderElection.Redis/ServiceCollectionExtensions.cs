using Microsoft.Extensions.DependencyInjection;
using QuorumMind.Infrastructure.LeaderElection.Extentions;
using QuorumMind.Infrastructure.LeaderElection.Interfaces;
using StackExchange.Redis;

namespace QuorumMind.Infrastructure.LeaderElection.Redis;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLeaderElectionRedis(this IServiceCollection services, Action<RedisLeaderElectionOptions> configure)
    {
        var options = new RedisLeaderElectionOptions();
        configure(options);

        services.AddSingleton(options);
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(options.ConnectionString));
        services.AddSingleton<ILeaderElectionProvider, RedisLeaderElectionProvider>();
        services.AddLeaderElectionCore(); // registers the background service + ILeaderLifecycleService

        return services;
    }
}