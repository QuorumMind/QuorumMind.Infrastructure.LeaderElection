using Microsoft.AspNetCore.Mvc;
using QuorumMind.Infrastructure.LeaderElection.Interfaces;

namespace QuorumMind.Infrastructure.LeaderElection.ExampleApp.Controllers;

[ApiController]
[Route("api/leader-election")]
public class LeaderElectionController : Controller
{
    private readonly ILeaderLifecycleService _leaderLifecycle;

    public LeaderElectionController(ILeaderLifecycleService leaderLifecycle)
    {
        _leaderLifecycle = leaderLifecycle;
    }
    
    
    [HttpGet("status")]
    public IActionResult GetLeaderElectionStatus()
    {
        return Ok($"Is Current Instnce is leader: {_leaderLifecycle.IsLeader}");
    }
}