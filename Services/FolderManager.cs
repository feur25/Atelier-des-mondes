using GodotRestAPI.Models;
using System.Text.Json;

public abstract class FolderManager {
    
    protected static List<Folder> Folders { get; set; } = [];
    protected static List<SuspectVote> Votes { get; set; } = [];

    private static readonly string FilePath = Path.Combine(AppContext.BaseDirectory, "folders.json");

    public static FolderManager Instance => new ConcreteFolderManager();

    private class ConcreteFolderManager : FolderManager { }
    static FolderManager() => LoadFromJson();
    private static void LoadFromJson() {
        if (File.Exists(FilePath)) {
            var json = File.ReadAllText(FilePath);
            Folders = JsonSerializer.Deserialize<List<Folder>>(json, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            }) ?? [];
        }
    }

    public virtual void AddFolder(Folder folder) => Folders.Add(folder);
    public virtual Folder? GetFolderById(int id) => Folders.FirstOrDefault(f => f.Id == id);
    public virtual List<Folder> GetAllFolders() => Folders;
    
    public virtual void RegisterVote(SuspectVote vote) => Votes.Add(vote);
    public virtual List<SuspectVote> GetVotesForSuspect(int suspectId) => [.. Votes.Where(v => v.SuspectId == suspectId)];
    public virtual Dictionary<int, int> GetVoteResults() => Votes.GroupBy(v => v.SuspectId).ToDictionary(g => g.Key, g => g.Count());

    internal bool UpdateFolder(int id, Folder updatedFolder) {
        this[id] = updatedFolder;
        return GetFolderById(id) != null;
    }

    internal bool DeleteFolder(int id) {
        var folder = GetFolderById(id);
        return folder != null && Folders.Remove(folder);
    }

    internal List<Suspect> GetSuspects(int folderId) => GetFolderById(folderId)?.Suspects ?? [];

    internal bool AddSuspect(int folderId, Suspect suspect) {
        if (this[folderId] == null) return false;
        var folder = this[folderId] ?? new Folder { Id = folderId, CaseTitle = string.Empty, CaseDescription = string.Empty, Suspects = [] };

        folder.Suspects.Add(suspect);
        return true;
    }
    internal bool DeleteSuspect(int folderId, int suspectId) {
        if (this[folderId] == null) return false;
        var folder = this[folderId] ?? new Folder { Id = folderId, CaseTitle = string.Empty, CaseDescription = string.Empty, Suspects = [] };

        if (this[folder, suspectId] == null) return false;

        var suspectToRemove = this[folder, suspectId];
        return suspectToRemove != null && folder.Suspects.Remove(suspectToRemove);
    }

    internal bool UpdateSuspect(int folderId, int suspectId, Suspect updatedSuspect) {
        if (this[folderId] == null) return false;
        var folder = this[folderId] ?? new Folder { Id = folderId, CaseTitle = string.Empty, CaseDescription = string.Empty, Suspects = [] };

        if (this[folder, suspectId] == null) return false;

        var suspect = this[folder, suspectId];
        if (suspect != null) {
            (suspect.Name, suspect.VideoUrl, suspect.Description, suspect.PsychologicalSigns) =
                (updatedSuspect.Name, updatedSuspect.VideoUrl, updatedSuspect.Description, updatedSuspect.PsychologicalSigns);
        }
        return true;
    }

    public Suspect? this[Folder folder, int suspectId] =>
        folder.Suspects.FirstOrDefault(s => s.Id == suspectId);

    public Folder? this[int folderId] {
        get => GetFolderById(folderId);
        set {
            var folder = GetFolderById(folderId);
            if (folder == null || value == null) return;

            (folder.CaseTitle, folder.CaseDescription, folder.Suspects) =
                (value.CaseTitle, value.CaseDescription, value.Suspects);
        }
    }
}
