using System.Text.Json;
using GodotRestAPI.Models;

public class VoteManagerImpl : VoteManager {
    private const string FilePath = "vote.json";

    public override int CurrentId => GetVotes().LastOrDefault()?.Id ?? 0;

    public override void SaveVote(Vote vote) {
        if (vote == null) throw new ArgumentNullException(nameof(vote));
        if (vote.Options == null || vote.Options.Count == 0) throw new ArgumentException("Vote options cannot be null or empty.", nameof(vote.Options));
        if (string.IsNullOrWhiteSpace(vote.Content)) throw new ArgumentException("Vote content cannot be null or empty.", nameof(vote.Content));
        
        var votes = GetVotes();
        var existingVote = votes.FirstOrDefault(v => v.Id == vote.Id);

        if (existingVote != null) votes.Remove(existingVote);
        if (vote.Id == 0) vote.Id = CurrentId + 1;

        votes.Add(vote);
        SaveVotesToFile(votes);
    }

    public override void UpdateVote(Vote vote) => SaveVote(vote);

    public override void DeleteVote(Vote vote) => SaveVotesToFile(GetVotes().Where(v => v.Id != vote.Id).ToList());

    public override void DeleteAllVotes() => SaveVotesToFile([]);

    public override List<Vote> GetVotes() {
        if (!File.Exists(FilePath)) return [];

        var json = File.ReadAllText(FilePath);

        var options = new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };

        return JsonSerializer.Deserialize<List<Vote>>(json, options) ?? [];
    }

    public override Vote GetVoteById(int id) =>
        GetVotes().FirstOrDefault(v => v.Id == id) ?? new Vote { Options = [], Content = string.Empty };

    private void SaveVotesToFile(List<Vote> votes) {
        File.WriteAllText(FilePath, JsonSerializer.Serialize(votes));
    }
}
