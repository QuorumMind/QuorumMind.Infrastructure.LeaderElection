using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuorumMind.Infrastructure.LeaderElection.Extentions;
using QuorumMind.Infrastructure.LeaderElection.Interfaces;

namespace QuorumMind.Infrastructure.LeaderElection.Consul;

public static class ConsulLeaderElectionExtensions
{
    public static IServiceCollection AddLeaderElectionConsul(this IServiceCollection services, Action<ConsulLeaderElectionOptions> configure)
    {
        var options = new ConsulLeaderElectionOptions();
        configure(options);
        services.AddSingleton(options);

        services.AddSingleton<ILeaderElectionProvider>(sp =>
        {
            var opts = sp.GetRequiredService<ConsulLeaderElectionOptions>();
            var logger = sp.GetRequiredService<ILogger<ConsulLeaderElectionProvider>>();
            return new ConsulLeaderElectionProvider(opts, logger);
        });

        services.AddLeaderElectionCore();
        return services;
    }
}