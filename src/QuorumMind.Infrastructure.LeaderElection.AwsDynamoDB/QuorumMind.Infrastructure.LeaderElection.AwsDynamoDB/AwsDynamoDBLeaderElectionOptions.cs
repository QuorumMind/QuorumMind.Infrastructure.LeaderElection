namespace QuorumMind.Infrastructure.LeaderElection.AwsDynamoDB;

public class AwsDynamoDBLeaderElectionOptions
{
    public string TableName { get; set; } = "LeaderElection";
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();
    public string LockKey { get; set; } = "default";
    public TimeSpan Ttl { get; set; } = TimeSpan.FromSeconds(10);
    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromSeconds(2);
}