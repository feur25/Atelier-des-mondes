namespace GodotRestAPI.Models {
    public class Suspect {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string VideoUrl { get; set; }
        public required string Description { get; set; }
        public List<string> PsychologicalSigns { get; set; } = [];
    }

    public class Folder {
        public int Id { get; set; }
        public required string CaseTitle { get; set; }
        public required string CaseDescription { get; set; }
        public required List<Suspect> Suspects { get; set; }
    }

    public class SuspectVote {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public int SuspectId { get; set; }
        public VoteChoice Choice { get; set; }
    }

    public enum VoteChoice {
        HeLies,
        HeHidesSomething,
        HeIsSincere
    }
}
