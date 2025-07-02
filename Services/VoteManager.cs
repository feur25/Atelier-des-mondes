using GodotRestAPI.Models;
public abstract class VoteManager {
    public virtual int CurrentId { get; set; } = 0;
    public static VoteManager GetVoteManager() { return new VoteManagerImpl(); }
    public virtual void SaveVote(Vote vote) { }
    public virtual void UpdateVote(Vote vote) { }
    public virtual void DeleteVote(Vote vote) { }
    public virtual void DeleteAllVotes() { }
    public virtual List<Vote> GetVotes() { return []; }
    public virtual Vote GetVoteById(int id) { return new Vote { Options = [], Content = string.Empty }; }
}
