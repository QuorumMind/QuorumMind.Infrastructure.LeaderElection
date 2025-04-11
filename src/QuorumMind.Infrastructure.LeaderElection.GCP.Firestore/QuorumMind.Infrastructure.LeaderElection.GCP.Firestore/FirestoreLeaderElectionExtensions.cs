using Google.Cloud.Firestore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuorumMind.Infrastructure.LeaderElection.Extentions;
using QuorumMind.Infrastructure.LeaderElection.Interfaces;

namespace QuorumMind.Infrastructure.LeaderElection.GCP.Firestore;

public static class FirestoreLeaderElectionExtensions
{
    public static IServiceCollection AddLeaderElectionFirestore(this IServiceCollection services, Action<FirestoreLeaderElectionOptions> configure)
    {
        var options = new FirestoreLeaderElectionOptions();
        configure(options);
        services.AddSingleton(options);

        services.AddSingleton<ILeaderElectionProvider>(sp =>
        {
            var opts = sp.GetRequiredService<FirestoreLeaderElectionOptions>();
            var db = sp.GetRequiredService<FirestoreDb>();
            var logger = sp.GetRequiredService<ILogger<FirestoreLeaderElectionProvider>>();
            return new FirestoreLeaderElectionProvider(db, opts, logger);
        });

        services.AddLeaderElectionCore();
        return services;
    }
}