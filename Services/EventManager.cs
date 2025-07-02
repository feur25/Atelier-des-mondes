using System.Text.Json;
using GodotRestAPI.Models;

namespace GodotRestAPI.Managers {
    public static class EventManager {
        private static readonly string _filePath = "events.json";
        public static List<Event> Events { get; private set; } = LoadEvents();

        private static List<Event> LoadEvents() =>
            File.Exists(_filePath)
                ? JsonSerializer.Deserialize<List<Event>>(File.ReadAllText(_filePath)) ?? []
                : [];

        public static void SaveEvents() =>
            File.WriteAllText(_filePath, JsonSerializer.Serialize(Events, new JsonSerializerOptions { WriteIndented = true }));

        public static void RemoveAllEvents() => Events.Clear();

        public static void AddEvent(Event evt) => Events.Add(evt);

        public static bool RemoveEvent(int id) =>
            Events.RemoveAll(e => e.Id == id) > 0;
    }
}
