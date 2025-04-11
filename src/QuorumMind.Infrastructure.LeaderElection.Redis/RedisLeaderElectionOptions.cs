namespace QuorumMind.Infrastructure.LeaderElection.Redis;

public class RedisLeaderElectionOptions
{
    public string ConnectionString { get; set; }

    /// <summary>
    /// Key used in Redis to identify leadership ownership.
    /// </summary>
    public string LeaderKey { get; set; } = "app-leader";

    /// <summary>
    /// Unique instance ID for this application instance.
    /// </summary>
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Duration for which leadership will be held before it must be renewed.
    /// </summary>
    public TimeSpan LeaseTime { get; set; } = TimeSpan.FromSeconds(10);
    
    /// <summary>
    /// The interval at which the current leader should renew its leadership lease. 
    /// </summary>
    public TimeSpan RenewTimeSpan { get; set; } = TimeSpan.FromSeconds(2);
}