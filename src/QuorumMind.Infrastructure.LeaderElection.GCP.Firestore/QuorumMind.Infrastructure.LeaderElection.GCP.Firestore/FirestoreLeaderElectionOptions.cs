namespace QuorumMind.Infrastructure.LeaderElection.GCP.Firestore;

public class FirestoreLeaderElectionOptions
{
    public string LockKey { get; set; } = "default";
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();
    public TimeSpan Ttl { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromSeconds(2);
}