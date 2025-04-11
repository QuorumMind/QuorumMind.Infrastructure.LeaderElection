namespace QuorumMind.Infrastructure.LeaderElection.Consul;

public class ConsulLeaderElectionOptions
{
    public string ConsulAddress { get; set; } = "http://localhost:8500";
    public string LeaderKey { get; set; } = "leader-election/key";
    public string InstanceId { get; set; } = Environment.MachineName;
    public TimeSpan LeaseTime { get; set; } = TimeSpan.FromSeconds(10);
    public TimeSpan RenewalInterval { get; set; } = TimeSpan.FromSeconds(1);
}
