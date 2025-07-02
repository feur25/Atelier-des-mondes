using GodotRestAPI.Models;
public static class InteractionManager {
    private static Dictionary<string, Interaction> playerInteractions = new Dictionary<string, Interaction>();

    public static Interaction GetCurrentInteraction(string playerPseudo) {
        if (!playerInteractions.TryGetValue(playerPseudo, out Interaction? value)) {
            value = new Interaction { Message = "Undefined", Player = new Player { Pseudo = playerPseudo } };
            playerInteractions[playerPseudo] = value;
        }
        return value;
    }

    public static void SetInteraction(string playerPseudo, Interaction interaction) => playerInteractions[playerPseudo] = interaction;

    public static List<Interaction> Interactions { get; private set; } = [];

    public static void AddInteraction(Interaction interaction) => Interactions.Add(interaction);

    public static void SetOption(string playerPseudo, int option) {
        if (playerInteractions.TryGetValue(playerPseudo, out Interaction? value)) value.Option = option;
    }

    public static int GetOption(string playerPseudo) {
        if (playerInteractions.TryGetValue(playerPseudo, out Interaction? value)) return value.Option;

        return -1;
    }
}