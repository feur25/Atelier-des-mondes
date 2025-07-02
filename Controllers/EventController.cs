using Microsoft.AspNetCore.Mvc;

using GodotRestAPI.Managers;
using GodotRestAPI.Models;

[ApiController]
[Route("api/game/events")]
public class EventController : ControllerBase {

    [HttpPost]
    public IActionResult AddEvent([FromBody] Event evt) {
        if (evt == null) return BadRequest("Event cannot be null");
        evt.Id = EventManager.Events.Count > 0 ? EventManager.Events.Max(e => e.Id) + 1 : 1;
        evt.Orded = EventManager.Events.Count + 1;
        
        EventManager.AddEvent(evt);
        return Ok(new { message = "Event added" });
    }

    [HttpPost("Orded")]
    public IActionResult UpdateEventOrded([FromBody] List<int> eventIds) {
        if (eventIds == null || eventIds.Count == 0) return BadRequest("Event IDs cannot be empty");

        for (int i = 0; i < eventIds.Count; i++) {
            Event? evt = EventManager.Events.FirstOrDefault(e => e.Id == eventIds[i]);
            if (evt != null) evt.Orded = i + 1;
        }

        EventManager.SaveEvents();
        return Ok(new { message = "Events order updated" });
    }

    [HttpDelete]
    public IActionResult RemoveAllEvents() {
        var success = EventManager.Events.Count > 0;
        EventManager.RemoveAllEvents();

        if (!success) return NotFound(new { message = "Aucun événement à supprimer" });

        return Ok(new { message = "Tous les événements ont été supprimés" });
    }

    [HttpGet]
    public IActionResult GetEvents() => Ok(EventManager.Events);

    [HttpDelete("{id}")]
    public IActionResult RemoveEvent(int id) {
        var result = EventManager.RemoveEvent(id);
        if (!result) return NotFound(new { message = "Event not found" });

        return Ok(new { message = "Event removed" });
    }

    [HttpPost("{id}/participant")]
    public IActionResult AddParticipant(int id, [FromBody] string participant) {
        if (string.IsNullOrWhiteSpace(participant)) return BadRequest("Participant name cannot be empty");

        Event? evt = EventManager.Events.FirstOrDefault(e => e.Id == id);
        if (evt == null) return NotFound(new { message = "Event not found" });

        evt.AddParticipant(participant);
        EventManager.SaveEvents();
        return Ok(new { message = "Participant added" });
    }

    [HttpDelete("{id}/participant")]
    public IActionResult RemoveParticipant(int id, [FromBody] string participant) {
        if (string.IsNullOrWhiteSpace(participant)) return BadRequest("Participant name cannot be empty");

        Event? evt = EventManager.Events.FirstOrDefault(e => e.Id == id);
        if (evt == null) return NotFound(new { message = "Event not found" });

        evt.RemoveParticipant(participant);
        EventManager.SaveEvents();
        return Ok(new { message = "Participant removed" });
    }

    [HttpPost("{id}/objective")]
    public IActionResult AddObjective(int id, [FromBody] string name) {
        if (string.IsNullOrWhiteSpace(name)) return BadRequest("Objective name cannot be empty");

        Event? evt = EventManager.Events.FirstOrDefault(e => e.Id == id);
        if (evt == null) return NotFound(new { message = "Event not found" });

        evt.AddObjective(name);
        EventManager.SaveEvents();
        return Ok(new { message = "Objective added" });
    }

    [HttpDelete("{id}/objective/{objectiveId}")]
    public IActionResult RemoveObjective(int id, int objectiveId) {
        Event? evt = EventManager.Events.FirstOrDefault(e => e.Id == id);
        if (evt == null) return NotFound(new { message = "Event not found" });

        evt.RemoveObjective(objectiveId);
        EventManager.SaveEvents();
        return Ok(new { message = "Objective removed" });
    }
}