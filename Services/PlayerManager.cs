using GodotRestAPI.Models;
public abstract class PlayerManager {
    protected static List<Player> Players { get; set; } = [];
    public static PlayerManager Instance => new ConcretePlayerManager();
    private class ConcretePlayerManager : PlayerManager { }
    public virtual int CurrentCountPlayer() { return Players.Count; }
    public virtual void AddPlayer(Player player) { Players.Add(player); }
    public virtual void RemovePlayer(Player player) { Players.Remove(player); }
    public virtual void RemoveAllPlayers() { Players.Clear(); }
    public virtual List<Player> GetAllPlayers() { return Players; }
    public virtual Player GetPlayerById(int id) { return Players.FirstOrDefault(p => p.Id == id) ?? new Player { Pseudo = "Lorenzo" }; }
    public virtual Player GetPlayerByPseudo(string pseudo) { return Players.FirstOrDefault(p => p.Pseudo == pseudo) ?? new Player { Pseudo = "Lorenzo" }; }
}
