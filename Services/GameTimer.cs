public class GameTimer {
    private static GameTimer? _instance;
    public static GameTimer Instance => _instance ??= new GameTimer();
    private Timer _timer;
    public int TimeElapsed { get; set; } = 0;
    public event Action<int>? OnTick;
    public GameTimer() => _timer = new Timer(Tick, null, 1000, 1000);
    private void Tick(object? state) => OnTick?.Invoke(++TimeElapsed);
    public void SetTimer(int dueTime, int period) => _timer.Change(dueTime, period);
    public void StopTimer() => Stop();
    public void Start(){ if (TimeElapsed == 0) _timer.Change(0, 3600000); }
    public void Stop() => _timer.Change(Timeout.Infinite, Timeout.Infinite);
    public void Reset() => TimeElapsed = 0;
    public void Dispose() => _timer.Dispose();
}
