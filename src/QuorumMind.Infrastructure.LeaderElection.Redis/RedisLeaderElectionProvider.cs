using Microsoft.Extensions.Logging;
using QuorumMind.Infrastructure.LeaderElection.Interfaces;
using StackExchange.Redis;

namespace QuorumMind.Infrastructure.LeaderElection.Redis;

public class RedisLeaderElectionProvider : ILeaderElectionProvider
{
    private readonly IDatabase _redis;
    private readonly string _instanceId;
    private readonly string _key;
    private readonly TimeSpan _leaseTime;
    private readonly ILogger<RedisLeaderElectionProvider> _logger;

    public RedisLeaderElectionProvider(
        IConnectionMultiplexer multiplexer,
        RedisLeaderElectionOptions options,
        ILogger<RedisLeaderElectionProvider> logger)
    {
        _redis = multiplexer.GetDatabase();
        _instanceId = options.InstanceId ?? Guid.NewGuid().ToString();
        _key = options.LeaderKey;
        _leaseTime = options.LeaseTime;
        RenewalInterval = options.RenewTimeSpan;

        _logger = logger;
        
    }

    public override async Task<bool> TryAcquireLeadershipAsync(CancellationToken cancellationToken)
    {
        return await _redis.StringSetAsync(
            _key,
            _instanceId,
            _leaseTime,
            When.NotExists
        );
    }
    
    public override async Task<bool> TryRenewLeadershipAsync(CancellationToken cancellationToken)
    {
        var current = await _redis.StringGetAsync(_key);
        if (current == _instanceId)
        {
            var renewed = await _redis.KeyExpireAsync(_key, _leaseTime);
            if (renewed)
            {
                _logger.LogDebug("🔁 Renewed leadership lease for {Instance}", _instanceId);
                return true;
            }
        }

        _logger.LogWarning("❌ Could not renew leadership — lost ownership");
        return false;
    }
}