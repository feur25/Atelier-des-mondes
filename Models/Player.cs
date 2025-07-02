namespace GodotRestAPI.Models {
    public class Player {
        public int Id { get; set; }
        public required string Pseudo { get; set; }
        public int Icons { get; set; }
        public int Score { get; set; }
    }
}