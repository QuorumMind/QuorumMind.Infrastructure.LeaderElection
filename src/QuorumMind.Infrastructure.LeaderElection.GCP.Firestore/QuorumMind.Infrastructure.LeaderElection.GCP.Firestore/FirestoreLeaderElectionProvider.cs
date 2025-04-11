using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using QuorumMind.Infrastructure.LeaderElection.Interfaces;

namespace QuorumMind.Infrastructure.LeaderElection.GCP.Firestore;

public class FirestoreLeaderElectionProvider : ILeaderElectionProvider
{
    private readonly FirestoreDb _db;
    private readonly FirestoreLeaderElectionOptions _options;
    private readonly ILogger<FirestoreLeaderElectionProvider> _logger;

    public FirestoreLeaderElectionProvider(FirestoreDb db, FirestoreLeaderElectionOptions options, ILogger<FirestoreLeaderElectionProvider> logger)
    {
        _db = db;
        _options = options;
        _logger = logger;
        RenewalInterval = _options.CheckInterval;
    }

    public  TimeSpan RenewalInterval { get; set; }

    private string LockPath => $"locks/{_options.LockKey}";

    public override async Task<bool> TryAcquireLeadershipAsync(CancellationToken cancellationToken)
    {
        var now = Timestamp.GetCurrentTimestamp();
        var expiresAt = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow.Add(_options.Ttl));

        var docRef = _db.Document(LockPath);

        try
        {
            return await _db.RunTransactionAsync<bool>(async tx =>
            {
                var snapshot = await tx.GetSnapshotAsync(docRef, cancellationToken:cancellationToken);

                if (!snapshot.Exists || snapshot.GetValue<Timestamp>("expiresAt") < now)
                {
                    var data = new Dictionary<string, object>
                    {
                        ["ownerId"] = _options.InstanceId,
                        ["expiresAt"] = expiresAt
                    };
                    tx.Set(docRef, data);
                    _logger.LogInformation("Leadership acquired for {LockKey} by {InstanceId}", _options.LockKey, _options.InstanceId);
                    return true;
                }

                _logger.LogDebug("Leadership is still held by another instance.");
                return false;
            },cancellationToken:cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to acquire leadership");
            return false;
        }
    }

    public override async Task<bool> TryRenewLeadershipAsync(CancellationToken cancellationToken)
    {
        var expiresAt = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow.Add(_options.Ttl));
        var docRef = _db.Document(LockPath);

        try
        {
            return await _db.RunTransactionAsync < bool>(async tx =>
            {
                var snapshot = await tx.GetSnapshotAsync(docRef,cancellationToken: cancellationToken);

                if (snapshot.Exists && snapshot.GetValue<string>("ownerId") == _options.InstanceId)
                {
                    tx.Update(docRef, new Dictionary<string, object> { ["expiresAt"] = expiresAt });
                    _logger.LogInformation("Leadership renewed for {LockKey} by {InstanceId}", _options.LockKey, _options.InstanceId);
                    return true;
                }

                _logger.LogWarning("Cannot renew leadership — not the current owner.");
                return false;
            },cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to renew leadership");
            return false;
        }
    }
}