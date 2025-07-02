namespace GodotRestAPI.Models {
    public class Vote {
        public int Id { get; set; }
        public VoteType Type { get; set; }
        public required string Content { get; set; }
        public required List<string> Options { get; set; }
        public int Reward { get; set; }
        public int Penalty { get; set; }
        public int Duration { get; set; }
        public int IndexOfGoodOption { get; set; }
    }
    public enum VoteType {
        Text,
        Video,
        Image
    }
}
