using Microsoft.AspNetCore.Mvc;
using GodotRestAPI.Models;

[ApiController]
[Route("api/game/interaction")]
public class InteractController : ControllerBase {
    [HttpPost]
    public IActionResult SetInteraction([FromBody] Interaction interaction) {
        if (interaction == null) return BadRequest("Interaction cannot be null");

        InteractionManager.SetInteraction(interaction.Player.Pseudo, interaction);

        AddInteraction(interaction);

        return Ok(new { message = "Interaction set" });
    }
    [HttpPost("add")]
    public IActionResult AddInteraction([FromBody] Interaction interaction) {
        if (interaction == null) return BadRequest("Interaction cannot be null");

        InteractionManager.AddInteraction(interaction);
        return Ok(new { message = "Interaction added" });
    }
    [HttpPost("option")]
    public IActionResult SetOption([FromBody] string playerName, int option) {
        if (option < 0) return BadRequest("Option cannot be negative");

        InteractionManager.SetOption(playerName, option);
        return Ok(new { message = "Option set" });
    }
    [HttpGet]
    public IActionResult GetInteraction([FromBody] string playerName) => Ok(InteractionManager.GetCurrentInteraction(playerName));
    [HttpGet("option")]
    public IActionResult GetOption([FromBody] string playerName) => Ok(new { option = InteractionManager.GetOption(playerName) });
}
