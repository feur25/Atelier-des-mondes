namespace GodotRestAPI.Models {
    
    public class Objective {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required object Progress { get; set; }
        public int Orded { get; set; }
    }
}
