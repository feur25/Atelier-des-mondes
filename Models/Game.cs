namespace GodotRestAPI.Models{
    public class Game {
        private static Game? _instance;
        public static Game Instance => _instance ??= new Game();
        public GameStatus Status { get; set; } = GameStatus.STOPPED;
        public Event? CurrentEvent { get; set; }
        public GameTimer Timer { get; set; } = new GameTimer( );
        public Player? player { get; set; }
        public Game() { Timer.OnTick += OnTick; }

        public void AddPlayer(Player player) => this.player = player;

        public void StartGame() { if (Status == GameStatus.STOPPED) Status = GameStatus.PLAYING; Timer.Start(); }

        public void ResumeGame() { if (Status == GameStatus.PAUSED) Status = GameStatus.PLAYING; Timer.Start(); }

        public void PauseGame() { if (Status == GameStatus.PLAYING) Status = GameStatus.PAUSED; Timer.Stop(); }

        public void StopGame() {
            if (Status != GameStatus.STOPPED) {
                Status = GameStatus.STOPPED;
                Timer.Stop(); Timer.Reset();
            }
        }

        public void SetCurrentEvent(Event evt) => CurrentEvent = evt;

        public void OnTick(int timeElapsed) {
            if (CurrentEvent != null && CurrentEvent.HasObjectives()) {
                foreach (Objective obj in CurrentEvent.Objectives) continue;
            }
        }
    }
    public class Event {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Orded { get; set; }
        public EventType Type { get; set; }
        public int MaxParticipants { get; set; }
        public List<Objective> Objectives { get; set; } = [];
        public List<string> Participants { get; set; } = [];
        public Event() {
            Name = string.Empty;
            Description = string.Empty;
        }

        public Event(string name, string description, int orded, int maxParticipants, EventType type) {
            Name = name;
            Description = description;
            Orded = orded;
            MaxParticipants = maxParticipants;
            Type = type;
        }

        public void AddParticipant(string participant) { if (Participants.Count < MaxParticipants) Participants.Add(participant); }

        public void RemoveParticipant(string participant) => Participants.Remove(participant);

        public void AddObjective(string name) => Objectives.Add(new Objective { Id = Objectives.Count + 1, Name = name, Progress = 0 });

        public void RemoveObjective(int id) {
            Objective? obj = Objectives.FirstOrDefault(o => o.Id == id);
            if (obj != null) Objectives.Remove(obj);
        }

        public void UpdateObjective(int id, int progress) {
            Objective? obj = Objectives.FirstOrDefault(o => o.Id == id);
            if (obj != null) obj.Progress = progress;
        }

        public void CompleteEvent() { foreach (Objective obj in Objectives) obj.Progress = 100; }

        public bool IsParticipant(string participant) => Participants.Contains(participant);

        public bool IsObjectiveCompleted(int id) {
            Objective? obj = Objectives.FirstOrDefault(o => o.Id == id);
            return obj != null && (int)obj.Progress == 100;
        }
        public bool IsEventFull() => Participants.Count >= MaxParticipants;
        public bool HasObjectives() => Objectives.Count > 0;
        public bool HasParticipants() => Participants.Count > 0;
        public bool HasParticipant(string participant) => Participants.Contains(participant);
        public bool HasObjective(int id) => Objectives.Any(o => o.Id == id);
        public bool HasObjective(string name) => Objectives.Any(o => o.Name == name);
        public bool HasObjectiveCompleted(int id) => Objectives.Any(o => o.Id == id && (int)o.Progress == 100);
        public bool HasObjectiveCompleted(string name) => Objectives.Any(o => o.Name == name && (int)o.Progress == 100);
        public bool HasObjectiveInProgress(int id) => Objectives.Any(o => o.Id == id && (int)o.Progress > 0 && (int)o.Progress < 100);
        public bool HasObjectiveInProgress(string name) => Objectives.Any(o => o.Name == name && (int)o.Progress > 0 && (int)o.Progress < 100);
        public bool HasObjectiveNotStarted(int id) => Objectives.Any(o => o.Id == id && (int)o.Progress == 0);
        public bool HasObjectiveNotStarted(string name) => Objectives.Any(o => o.Name == name && (int)o.Progress == 0);
        public String GetEventType() => Type.ToString();
    }
}
public enum EventType {
    BROADCAST_TEXT,
    VIDEO,
    IMAGE,
    SOUND,
    BROADCAST_COMMUNICATION
}
public enum GameStatus {
    PLAYING,
    PAUSED,
    STOPPED
}