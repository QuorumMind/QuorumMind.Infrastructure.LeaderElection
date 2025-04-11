using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuorumMind.Infrastructure.LeaderElection.Interfaces;

namespace QuorumMind.Infrastructure.LeaderElection.Service;


public class LeaderElectionBackgroundService : BackgroundService, ILeaderLifecycleService
    {
        private readonly ILeaderElectionProvider _provider;
        private readonly ILogger<LeaderElectionBackgroundService> _logger;

        private bool _isLeader;

        public bool IsLeader => _isLeader;

        public event Func<CancellationToken, Task>? OnLeadershipAcquired;
        public event Func<CancellationToken, Task>? OnLeadershipLost;

        public LeaderElectionBackgroundService(
            ILeaderElectionProvider provider,
            ILogger<LeaderElectionBackgroundService> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Leader election background service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (!_isLeader)
                    {
                        _isLeader = await _provider.TryAcquireLeadershipAsync(stoppingToken);

                        if (_isLeader)
                        {
                            _logger.LogInformation("🏆 Leadership acquired.");
                            if (OnLeadershipAcquired != null)
                                await OnLeadershipAcquired.Invoke(stoppingToken);
                        }
                    }
                    else
                    {
                        var renewed = await _provider.TryRenewLeadershipAsync(stoppingToken);

                        if (!renewed)
                        {
                            _isLeader = false;
                            _logger.LogWarning("👋 Leadership lost.");
                            if (OnLeadershipLost != null)
                                await OnLeadershipLost.Invoke(stoppingToken);
                        }
                        else
                        {
                            _logger.LogDebug("🔁 Leadership renewed.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during leader election loop.");
                }

                await Task.Delay(_provider.RenewalInterval, stoppingToken);

            }

            _logger.LogInformation("Leader election background service stopped");
        }
    }
    