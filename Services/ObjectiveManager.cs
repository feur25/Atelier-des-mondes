using System.Text.Json;

using GodotRestAPI.Models;
public static class ObjectiveManager {
    private static string FilePath = "objectives.json";
    public static List<Objective> Objectives { get; private set; } = [];

    public static void UpdateObjectiveProgress(int index, int progress) {
        var objective = Objectives.Find(i => i.Id == index);
        if (objective != null) objective.Progress = progress;
    }

    static ObjectiveManager() => LoadObjectives();
    public static void LoadObjectives() {
        try {
            if (!File.Exists(FilePath)) File.WriteAllText(FilePath, "[]");

            string jsonText = File.ReadAllText(FilePath);

            if (string.IsNullOrWhiteSpace(jsonText)) Objectives = [];
            else Objectives = JsonSerializer.Deserialize<List<Objective>>(jsonText) ?? [];

        } catch (Exception ex) {
            Console.WriteLine("Erreur lors du chargement des objectifs: " + ex.Message);
            Objectives = [];
        }
    }

    public static void RemoveAllObjectives() { Objectives.Clear(); SaveObjectives(); }

    public static List<Objective> GetAllObjectives() { return Objectives; }

    public static bool RemoveObjective(int id) {
        var obj = Objectives.FirstOrDefault(o => o.Id == id);
        
        if (obj == null) return false;

        Objectives.Remove(obj); SaveObjectives(); return true;
    }

    public static void AddObjective(Objective obj) { Objectives.Add(obj); SaveObjectives(); }

    public static void SaveObjectives() {
        try { File.WriteAllText(FilePath, JsonSerializer.Serialize(Objectives, new JsonSerializerOptions { WriteIndented = true })); }
        catch (Exception ex) { Console.WriteLine("Erreur lors de l'enregistrement des objectifs: " + ex.Message); }
    }
}
