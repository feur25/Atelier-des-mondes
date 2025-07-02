using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/game/votes")]
public class VoteController : ControllerBase {

    [HttpPost("{currentId}")]
    public IActionResult PostCurrentVoteId(int currentId) {
        _ = new VoteManagerImpl() { CurrentId = currentId };

        return Ok(new { message = "Current vote ID set" });
    }
    
    [HttpGet]
    public IActionResult GetVotes() {
        VoteManagerImpl voteManagerImpl = new();
        var votes = voteManagerImpl.GetVotes();

        Console.WriteLine($"Retrieved {votes.Count} votes.");

        return Ok(votes);
    }
    
    [HttpGet("{id}")]
    public IActionResult GetVote(int id) {
        VoteManagerImpl voteManagerImpl = new();

        return Ok(voteManagerImpl.GetVoteById(id));
    }
    [HttpGet("currentId")]
    public IActionResult GetCurrentVoteId() {
        VoteManagerImpl voteManagerImpl = new();

        return Ok(new { currentId = voteManagerImpl.CurrentId });
    }
    [HttpGet("count")]
    public IActionResult GetVotesCount() {
        VoteManagerImpl voteManagerImpl = new();

        return Ok(new { count = voteManagerImpl.GetVotes().Count });
    }
}
