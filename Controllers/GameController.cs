using Microsoft.AspNetCore.Mvc;
using GodotRestAPI.Models;

[ApiController]
[Route("api/game")]
public class GameController : ControllerBase {
    [HttpGet("timer")]
    public IActionResult GetTimer() => Ok(new { timeElapsed = GameTimer.Instance.TimeElapsed });

    [HttpGet("pipeline")]
    public IActionResult GetPipeline() {
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "pipeline.json");
        if (!System.IO.File.Exists(filePath)) return NotFound("Pipeline file not found");

        string json = System.IO.File.ReadAllText(filePath);
        return Ok(json);
    }

    [HttpPost("timer")]
    public IActionResult SetTimer([FromBody] int time) {
        if (time < 0) return BadRequest("Time cannot be negative");

        GameTimer.Instance.SetTimer(time * 1000, 1000);
        Console.WriteLine("Timer set" + GameTimer.Instance.TimeElapsed + " seconds" + time + " seconds");
        return Ok(new { message = "Timer set", timeElapsed = GameTimer.Instance.TimeElapsed });
    }

    [HttpPost("start")]
    public IActionResult StartGame() { Game.Instance.StartGame(); return Ok(new { message = "Game started" }); }

    [HttpPost("resume")]
    public IActionResult ResumeGame() { Game.Instance.ResumeGame(); return Ok(new { message = "Game resumed" }); }

    [HttpPost("stop")]
    public IActionResult StopGame() { Game.Instance.StopGame(); return Ok(new { message = "Game stopped" }); }

    [HttpPost("pause")]
    public IActionResult PauseGame() { Game.Instance.PauseGame(); return Ok(new { message = "Game paused" }); }

    [HttpGet("status")]
    public IActionResult GetGameStatus() => Ok(new { status = Game.Instance.Status.ToString() });

    [HttpPost("message")]
    public IActionResult SetMessage([FromBody] string message) {
        if (string.IsNullOrWhiteSpace(message)) return BadRequest("Message cannot be empty");
        
        MessageManager.SetMessage(message);
        return Ok(new { message = MessageManager.CurrentMessage }); 
    }
    
    [HttpPost("interact")]
    public IActionResult Interact([FromBody] string action) {
        if (string.IsNullOrWhiteSpace(action)) return BadRequest("Action cannot be empty");

        switch (action) {
            case "start": Game.Instance.StartGame(); break;
            case "resume": Game.Instance.ResumeGame(); break;
            case "pause": Game.Instance.PauseGame(); break;
            case "stop": Game.Instance.StopGame(); break;
            default: return BadRequest("Invalid action");
        }

        return Ok(new { message = "Game " + action + "ed" });
    }
}
