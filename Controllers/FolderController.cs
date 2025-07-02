using Microsoft.AspNetCore.Mvc;
using GodotRestAPI.Models;

[ApiController]
[Route("api/game/folders")]
public class FolderController : ControllerBase {
    private readonly FolderManager folderManager = FolderManager.Instance;

    [HttpGet]
    public IActionResult GetFolders() => Ok(folderManager.GetAllFolders());

    [HttpGet("{id}")]
    public IActionResult GetFolderById(int id) {
        var folder = folderManager.GetFolderById(id);
        return folder != null ? Ok(folder) : NotFound("Folder not found");
    }

    [HttpPost]
    public IActionResult AddFolder([FromBody] Folder folder) {
        folderManager.AddFolder(folder);
        return Ok("Folder added successfully");
    }

    [HttpPut("{id}")]
    public IActionResult UpdateFolder(int id, [FromBody] Folder updatedFolder) {
        var result = folderManager.UpdateFolder(id, updatedFolder);
        return result ? Ok("Folder updated successfully") : NotFound("Folder not found");
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteFolder(int id) {
        var result = folderManager.DeleteFolder(id);
        return result ? Ok("Folder deleted successfully") : NotFound("Folder not found");
    }

    [HttpGet("{folderId}/suspects")]
    public IActionResult GetSuspects(int folderId) {
        var suspects = folderManager.GetSuspects(folderId);
        return suspects != null ? Ok(suspects) : NotFound("Folder not found");
    }

    [HttpPost("{folderId}/suspects")]
    public IActionResult AddSuspect(int folderId, [FromBody] Suspect suspect) {
        var result = folderManager.AddSuspect(folderId, suspect);
        return result ? Ok("Suspect added successfully") : NotFound("Folder not found");
    }

    [HttpPut("{folderId}/suspects/{suspectId}")]
    public IActionResult UpdateSuspect(int folderId, int suspectId, [FromBody] Suspect updatedSuspect) {
        var result = folderManager.UpdateSuspect(folderId, suspectId, updatedSuspect);
        return result ? Ok("Suspect updated successfully") : NotFound("Folder or suspect not found");
    }

    [HttpDelete("{folderId}/suspects/{suspectId}")]
    public IActionResult DeleteSuspect(int folderId, int suspectId) {
        var result = folderManager.DeleteSuspect(folderId, suspectId);
        return result ? Ok("Suspect deleted successfully") : NotFound("Folder or suspect not found");
    }

    [HttpPost("vote")]
    public IActionResult RegisterVote([FromBody] SuspectVote vote) {
        folderManager.RegisterVote(vote);
        return Ok("Vote registered");
    }

    [HttpGet("votes/{suspectId}")]
    public IActionResult GetVotesForSuspect(int suspectId) {
        return Ok(folderManager.GetVotesForSuspect(suspectId));
    }

    [HttpGet("results")]
    public IActionResult GetVoteResults() {
        return Ok(folderManager.GetVoteResults());
    }
}
