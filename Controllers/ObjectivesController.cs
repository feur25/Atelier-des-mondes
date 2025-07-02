using Microsoft.AspNetCore.Mvc;
using GodotRestAPI.Models;
using System.Text.Json;

[ApiController]
[Route("api/game/objectives")]
public class ObjectivesController : ControllerBase {

    [HttpPost]
    public IActionResult AddObjective([FromBody] Objective obj) {
        if (obj == null) return BadRequest("Objective cannot be null");
        obj.Id = ObjectiveManager.GetNextId();

        ObjectiveManager.AddObjective(obj);
        return Ok(new { message = "Objective added" });
    }


    [HttpPost("progress/{id}")]
    public IActionResult UpdateProgress(int id, [FromBody] int progress) {
        if (progress < 0) return BadRequest("Progress cannot be negative");

        // Objective? objective = ObjectiveManager.Objectives.FirstOrDefault(o => o.Id == id);
        // if (objective == null) return NotFound(new { message = "Objective not found" });

        // objective.Progress = progress;
        ObjectiveManager.UpdateObjectiveProgress(id, progress);

        return Ok(new { message = "Progress updated" });
    }

    [HttpPost("{id}")]
    public IActionResult UpdateObjectivep(int id, [FromBody] Objective obj) {
        if (obj == null) return BadRequest("Objective cannot be null");

        Objective? objective = ObjectiveManager.Objectives.FirstOrDefault(o => o.Id == id);
        if (objective == null) return NotFound(new { message = "Objective not found" });

        objective.Name = obj.Name;
        objective.Progress = obj.Progress;
        objective.Orded = obj.Orded;

        ObjectiveManager.SaveObjectives();
        return Ok(new { message = "Objective updated" });
    }

    [HttpPost("Orded")]
    public IActionResult UpdateObjectivesOrder([FromBody] List<int> objectiveIds)  {
        if (objectiveIds == null || objectiveIds.Count == 0) return BadRequest("Objective IDs cannot be empty");

        for (int i = 0; i < objectiveIds.Count; i++) {
            Objective? obj = ObjectiveManager.Objectives.FirstOrDefault(o => o.Id == objectiveIds[i]);
            if (obj != null) obj.Orded = i + 1;
        }

        ObjectiveManager.SaveObjectives();
        return Ok(new { message = "Objectives order updated" });
    }

    [HttpGet]
    public IActionResult GetObjectives() => Ok(ObjectiveManager.Objectives);

    [HttpGet("all")]
    public IActionResult GetAllObjectives() {
        var objectives = ObjectiveManager.GetAllObjectives();
        Console.WriteLine(JsonSerializer.Serialize(objectives));
        return Ok(objectives);
    }

    [HttpDelete]
    public IActionResult RemoveAllObjectives() {
        var success = ObjectiveManager.Objectives.Count > 0;
        ObjectiveManager.RemoveAllObjectives();

        if (!success) return NotFound(new { message = "No objectives to remove" });

        return Ok(new { message = "All objectives removed" });
    }

    [HttpDelete("{id}")]
    public IActionResult RemoveObjective(int id) {
        var result = ObjectiveManager.RemoveObjective(id);
        if (!result) return NotFound(new { message = "Objective not found" });

        return Ok(new { message = "Objective removed" });
    }

    [HttpGet("canvas/pipeline")]
    public IActionResult GetCanvasPipeline() =>
        Ok(ObjectiveManager.Objectives.Select(o => o.Name));

    [HttpGet("canvas/pipeline/{id}")]
    public IActionResult GetCanvasPipelineById(int id) {
        var obj = ObjectiveManager.Objectives.FirstOrDefault(o => o.Id == id);
        return obj == null ? NotFound(new { message = "Objective not found" }) : Ok(obj.Name);
    }

    [HttpGet("canvas/pipeline/progress/{progressStatus}")]
    public IActionResult GetCanvasPipelineByProgress(bool progressStatus) {
        var canvasNames = ObjectiveManager.Objectives
            .Where(o => {
                if (o.Progress is bool boolValue)
                    return boolValue == progressStatus;
                else if (o.Progress is JsonElement jsonElement)
                    return jsonElement.ValueKind == JsonValueKind.True ? true : false == progressStatus;
                else if (o.Progress is int intValue)
                    return (intValue != 0) == progressStatus;
                else
                    return Convert.ToBoolean(o.Progress?.ToString()) == progressStatus;
            })
            .Select(o => o.Name)
            .ToList();

        return Ok(canvasNames);
    }

    [HttpPost("canvas/pipeline/batch")]
    public IActionResult AddCanvasPipelineBatch([FromBody] List<string> canvasNames) {
        if (canvasNames == null || canvasNames.Count == 0) 
            return BadRequest("Canvas names cannot be empty");
        
        var existingNames = ObjectiveManager.Objectives.Select(o => o.Name).ToHashSet();
        var newNames = canvasNames.Where(name => !existingNames.Contains(name)).ToList();
        
        if (newNames.Count == 0) return Ok(new List<Objective>());
        
        var newObjectives = new List<Objective>();
        
        foreach (var name in newNames) {
            var newObjective = new Objective {
                Id = ObjectiveManager.Objectives.Count + newObjectives.Count + 1,
                Name = name,
                Progress = false,
                Orded = ObjectiveManager.Objectives.Count + newObjectives.Count + 1
            };
            newObjectives.Add(newObjective);
        }
        
        ObjectiveManager.Objectives.AddRange(newObjectives);
        ObjectiveManager.SaveObjectives();
        
        return Ok(newObjectives);
    }

    [HttpDelete("canvas/pipeline/{id}")]
    public IActionResult RemoveCanvasPipeline(int id) {
        var result = ObjectiveManager.RemoveObjective(id);
        if (!result) return NotFound(new { message = "Objective not found" });
        return Ok(new { message = "Objective removed" });
    }

    [HttpPost("canvas/pipeline")]
    public IActionResult AddCanvasPipeline([FromBody] string canvasName) {
        if (string.IsNullOrWhiteSpace(canvasName)) return BadRequest("Canvas name cannot be empty");

        var newObjective = new Objective {
            Id = ObjectiveManager.Objectives.Count > 0 ? ObjectiveManager.Objectives.Max(o => o.Id) + 1 : 1,
            Name = canvasName,
            Progress = false,
            Orded = ObjectiveManager.Objectives.Count + 1
        };

        ObjectiveManager.AddObjective(newObjective);
        return Ok(new { message = "Canvas pipeline added", objective = newObjective });
    }

}
