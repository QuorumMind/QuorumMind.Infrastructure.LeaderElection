using QuorumMind.Infrastructure.LeaderElection.Interfaces;

namespace QuorumMind.Infrastructure.LeaderElection.ExampleApp.Worker;

public class ReminderWorker : BackgroundService
{
    private readonly ILeaderLifecycleService _leaderLifecycle;
    private readonly ILogger<ReminderWorker> _logger;

    public ReminderWorker(ILeaderLifecycleService leaderLifecycle, ILogger<ReminderWorker> logger)
    {
        _leaderLifecycle = leaderLifecycle;
        _logger = logger;
        
        _leaderLifecycle.OnLeadershipAcquired += async (ct) =>
        {
            Console.WriteLine("🎉 I am now the leader!");
            await Task.CompletedTask;
        };

        _leaderLifecycle.OnLeadershipLost += async (ct) =>
        {
            Console.WriteLine("😢 I lost leadership.");
            await Task.CompletedTask;
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_leaderLifecycle.IsLeader)
                {
                    _logger.LogInformation("This instance is the leader. Executing scheduled task...");
                    await SendReminderAsync();
                }
                else
                {
                    _logger.LogInformation("This instance is not the leader. Skipping task execution.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during leader election or task execution");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }

    private Task SendReminderAsync()
    {
        Console.WriteLine("Sending reminder notification... .");
        return Task.CompletedTask;
    }
}