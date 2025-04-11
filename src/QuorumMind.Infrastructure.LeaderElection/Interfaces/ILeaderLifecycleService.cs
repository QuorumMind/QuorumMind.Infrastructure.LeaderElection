namespace QuorumMind.Infrastructure.LeaderElection.Interfaces;

public interface ILeaderLifecycleService
{
    /// <summary>
    /// Returns true if this instance currently holds leadership.
    /// </summary>
    bool IsLeader { get; }

    /// <summary>
    /// Raised when this instance gains leadership.
    /// </summary>
    event Func<CancellationToken, Task> OnLeadershipAcquired;

    /// <summary>
    /// Raised when this instance loses leadership.
    /// </summary>
    event Func<CancellationToken, Task> OnLeadershipLost;
}