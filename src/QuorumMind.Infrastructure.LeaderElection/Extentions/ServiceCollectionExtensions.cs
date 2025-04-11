using Microsoft.Extensions.DependencyInjection;
using QuorumMind.Infrastructure.LeaderElection.Interfaces;
using QuorumMind.Infrastructure.LeaderElection.Service;

namespace QuorumMind.Infrastructure.LeaderElection.Extentions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLeaderElectionCore(this IServiceCollection services)
    {
        services.AddSingleton<LeaderElectionBackgroundService>();

        services.AddSingleton<ILeaderLifecycleService>(sp =>
            sp.GetRequiredService<LeaderElectionBackgroundService>());

        services.AddHostedService(sp =>
            sp.GetRequiredService<LeaderElectionBackgroundService>());
    
        return services;
    }
}