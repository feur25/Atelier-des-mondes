namespace GodotRestAPI.Models {
    public class Interaction {
        public int Option { get; set; }
        public required string Message { get; set; }
        public required Player Player { get; set; }
    }
}