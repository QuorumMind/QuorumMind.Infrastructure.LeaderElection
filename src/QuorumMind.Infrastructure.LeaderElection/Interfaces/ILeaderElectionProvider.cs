namespace QuorumMind.Infrastructure.LeaderElection.Interfaces;

public abstract class ILeaderElectionProvider
{
    public TimeSpan RenewalInterval { get; set; }

    /// <summary>
    /// Tries to acquire leadership. Returns true if successful.
    /// </summary>
    public abstract Task<bool> TryAcquireLeadershipAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// Tries to renew leadership. Returns true if successful.
    /// </summary>
    public abstract Task<bool> TryRenewLeadershipAsync(CancellationToken cancellationToken);
}