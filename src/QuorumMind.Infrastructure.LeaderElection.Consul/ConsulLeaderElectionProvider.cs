using Consul;
using Microsoft.Extensions.Logging;
using QuorumMind.Infrastructure.LeaderElection.Interfaces;

namespace QuorumMind.Infrastructure.LeaderElection.Consul;

public class ConsulLeaderElectionProvider : ILeaderElectionProvider, IDisposable
{
    private readonly ConsulClient _client;
    private readonly string _key;
    private readonly string _instanceId;
    private readonly TimeSpan _ttl;
    private string? _sessionId;
    private readonly ILogger<ConsulLeaderElectionProvider> _logger;

    public ConsulLeaderElectionProvider(
        ConsulLeaderElectionOptions options,
        ILogger<ConsulLeaderElectionProvider> logger)
    {
        base.RenewalInterval = options.RenewalInterval;

        _client = new ConsulClient(cfg => cfg.Address = new Uri(options.ConsulAddress));
        _key = options.LeaderKey;
        _instanceId = options.InstanceId;
        _ttl = options.LeaseTime;
        _logger = logger;
    }

    public override async Task<bool> TryAcquireLeadershipAsync(CancellationToken cancellationToken)
    {
        if (_sessionId == null)
        {
            var sessionReq = new SessionEntry
            {
                Name = $"leader-election-{_instanceId}",
                TTL = _ttl,
                Behavior = SessionBehavior.Delete
            };

            var session = await _client.Session.Create(sessionReq, cancellationToken);
            _sessionId = session.Response;
        }

        var kvPair = new KVPair(_key)
        {
            Session = _sessionId,
            Value = System.Text.Encoding.UTF8.GetBytes(_instanceId)
        };

        var acquired = await _client.KV.Acquire(kvPair, cancellationToken);
        _logger.LogDebug("[Consul] TryAcquireLeadership: {Result}", acquired.Response);
        return acquired.Response;
    }

    public override async Task<bool> TryRenewLeadershipAsync(CancellationToken cancellationToken)
    {
        if (_sessionId == null) return false;

        var renewed = await _client.Session.Renew(_sessionId, cancellationToken);
        var success = renewed.Response != null;
        _logger.LogDebug("[Consul] TryRenewLeadership: success={Success}", success);
        return success;
    }

    public void Dispose()
    {
        if (_sessionId != null)
        {
            try { _client.Session.Destroy(_sessionId).Wait(); } catch { /* ignore */ }
            _sessionId = null;
        }
        _client.Dispose();
    }
}
