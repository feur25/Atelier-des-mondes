public static class MessageManager {
    public static string CurrentMessage { get; private set; } = "";
    public static void SetMessage(string message) => CurrentMessage = message;
}
