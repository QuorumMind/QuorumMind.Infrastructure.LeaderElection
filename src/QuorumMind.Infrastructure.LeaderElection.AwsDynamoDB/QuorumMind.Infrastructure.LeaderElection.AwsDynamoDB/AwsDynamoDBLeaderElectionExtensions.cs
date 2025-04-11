using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuorumMind.Infrastructure.LeaderElection.Extentions;
using QuorumMind.Infrastructure.LeaderElection.Interfaces;

namespace QuorumMind.Infrastructure.LeaderElection.AwsDynamoDB;

public static class AwsLeaderElectionExtensions
{
    public static IServiceCollection AddLeaderElectionAws(this IServiceCollection services, Action<AwsDynamoDBLeaderElectionOptions> configure)
    {
        var options = new AwsDynamoDBLeaderElectionOptions();
        configure(options);
        services.AddSingleton(options);

        services.AddSingleton<ILeaderElectionProvider>(sp =>
        {
            var opts = sp.GetRequiredService<AwsDynamoDBLeaderElectionOptions>();
            var dynamo = sp.GetRequiredService<IAmazonDynamoDB>();
            var logger = sp.GetRequiredService<ILogger<AwsDynamoDbLeaderElectionProvider>>();
            return new AwsDynamoDbLeaderElectionProvider(dynamo, opts, logger);
        });

        services.AddLeaderElectionCore(); // core features, such as hosted background service, if defined
        return services;
    }
}