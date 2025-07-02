using Microsoft.AspNetCore.Mvc;
using GodotRestAPI.Models;

[ApiController]
[Route("api/game/players")]
public class PlayerController : ControllerBase {
    
    [HttpPost]
    public IActionResult AddPlayer([FromBody] Player player) {
        if (string.IsNullOrWhiteSpace(player.Pseudo)) return BadRequest("Player name cannot be empty");

        PlayerManager playerManager = PlayerManager.Instance;

        playerManager.AddPlayer(player);
        Console.WriteLine("Player added: " + player.Pseudo);
        return Ok(new { message = "Player added", players = Game.Instance.player });
    }
    
    [HttpPost("{playerId}/score")]
    public IActionResult UpdateScore(int playerId, [FromForm] int score) {
        if (score < 0) return BadRequest("Score cannot be negative");

        PlayerManager playerManager = PlayerManager.Instance;
        
        Player selectedPlayer = playerManager.GetPlayerById(playerId);

        if (selectedPlayer != null) selectedPlayer.Score = score;
        else return BadRequest("Player is not initialized");

        return Ok(new { message = "Score updated" });
    }

    [HttpPost("{playerId}/icon")]
    public IActionResult SelectedIcon(int playerId, [FromForm] int icon) {
        if (icon < 0) return BadRequest("Icon cannot be negative");
        PlayerManager playerManager = PlayerManager.Instance;

        Player selectedPlayer = playerManager.GetPlayerById(playerId);

        if (selectedPlayer != null) selectedPlayer.Icons = icon;
        else return BadRequest("Player is not initialized");

        return Ok(new { message = "Icon selected" });
    }

    [HttpPost("{playerId}/vote")]
    public IActionResult Vote(int playerId, [FromBody] int currentVote, int option) {
        if (option < 0) return BadRequest("Option cannot be negative");

        PlayerManager playerManager = PlayerManager.Instance;
        Player selectedPlayer = playerManager.GetPlayerById(playerId);

        VoteManagerImpl voteManagerImpl = new();
        Vote vote = voteManagerImpl.GetVoteById(currentVote);

        if (vote.IndexOfGoodOption == option) selectedPlayer.Score += vote.Reward;
        else selectedPlayer.Score -= vote.Penalty;

        return Ok(new { message = "Vote added" });
    }

    [HttpGet("count")]
    public IActionResult GetPlayersCount() {
        int count = PlayerManager.Instance.CurrentCountPlayer();
        Console.WriteLine($"Current player count: {count}");
        return Ok(new { count });
    }

    [HttpGet("{id}")]
    public IActionResult GetPlayerById(int id) {
        PlayerManager playerManager = PlayerManager.Instance;
        Player selectedPlayer = playerManager.GetPlayerById(id);

        if (selectedPlayer == null) return NotFound(new { message = "Player not found" });

        return Ok(selectedPlayer);
    }

    [HttpGet("pseudo/{pseudo}")]
    public IActionResult GetPlayerByPseudo(string pseudo) {
        PlayerManager playerManager = PlayerManager.Instance;
        Player selectedPlayer = playerManager.GetPlayerByPseudo(pseudo);

        if (selectedPlayer == null) return NotFound(new { message = "Player not found" });

        return Ok(selectedPlayer);
    }

    [HttpGet]
    public IActionResult GetAllPlayers() => Ok(PlayerManager.Instance.GetAllPlayers());

    [HttpDelete]
    public IActionResult RemovePlayer() {
        PlayerManager playerManager = PlayerManager.Instance;
        playerManager.RemoveAllPlayers();

        return Ok(new { message = "All players removed" });
    }

    [HttpDelete("{id}")]
    public IActionResult RemovePlayerById(int id) {
        PlayerManager playerManager = PlayerManager.Instance;
        Player selectedPlayer = playerManager.GetPlayerById(id);

        if (selectedPlayer == null) return NotFound(new { message = "Player not found" });

        playerManager.RemovePlayer(selectedPlayer);
        return Ok(new { message = "Player removed" });
    }
}
