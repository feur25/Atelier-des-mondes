using System.Text.Json;

using GodotRestAPI.Models;
public static class ObjectiveManager {
    private static string FilePath = "objectives.json";
    public static List<Objective> Objectives { get; private set; } = [];

    public static void UpdateObjectiveProgress(int index, int progress) {
        var objective = Objectives.Find(i => i.Id == index);
        if (objective != null) objective.Progress = progress;
        SaveObjectives();
    }

    static ObjectiveManager() => LoadObjectives();
    public static void LoadObjectives() {
        try {
            if (!File.Exists(FilePath)) File.WriteAllText(FilePath, "[]");

            string jsonText = File.ReadAllText(FilePath);

            if (string.IsNullOrWhiteSpace(jsonText)) Objectives = [];
            else Objectives = JsonSerializer.Deserialize<List<Objective>>(jsonText) ?? [];

            EnsureUniqueIds();

        } catch (Exception ex) {
            Console.WriteLine("Erreur lors du chargement des objectifs: " + ex.Message);
            Objectives = [];
        }
    }

    private static void EnsureUniqueIds() {
        var usedIds = new HashSet<int>();
        var duplicates = new List<Objective>();

        foreach (var obj in Objectives) {
            if (!usedIds.Add(obj.Id)) {
                duplicates.Add(obj);
            }
        }

        foreach (var obj in duplicates) {
            int newId = Objectives.Max(o => o.Id) + 1;
            obj.Id = newId;
            usedIds.Add(newId);
        }

        if (duplicates.Count > 0) {
            SaveObjectives();
            Console.WriteLine($"Corrected {duplicates.Count} duplicate IDs");
        }
    }

    public static int GetNextId() {
        return Objectives.Count > 0 ? Objectives.Max(o => o.Id) + 1 : 1;
    }
    public static void RemoveAllObjectives() { Objectives.Clear(); SaveObjectives(); }

    public static List<Objective> GetAllObjectives() { return Objectives; }

    public static bool RemoveObjective(int id) {
        var obj = Objectives.FirstOrDefault(o => o.Id == id);
        
        if (obj == null) return false;

        Objectives.Remove(obj); SaveObjectives(); return true;
    }

    public static void AddObjective(Objective obj) { 
        if (Objectives.Any(o => o.Name == obj.Name)) {
            Console.WriteLine($"Warning: An objective with name '{obj.Name}' already exists");
        }
        
        obj.Id = GetNextId();
        
        Objectives.Add(obj); 
        SaveObjectives(); 
    }

    public static void SaveObjectives() {
        try { File.WriteAllText(FilePath, JsonSerializer.Serialize(Objectives, new JsonSerializerOptions { WriteIndented = true })); }
        catch (Exception ex) { Console.WriteLine("Erreur lors de l'enregistrement des objectifs: " + ex.Message); }
    }
}
