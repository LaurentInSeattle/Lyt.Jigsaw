namespace Lyt.Jigsaw.Model.Utilities;

public sealed class TimeoutTimer
{
    private readonly Action onTimeout;
    private readonly TimeSpan dueTime;

    private Timer? timer;

    public TimeoutTimer(Action onTimeout, int timeoutMilliseconds = 1042)
    {
        if (timeoutMilliseconds < 0 || timeoutMilliseconds > 24 * 60 * 60 * 1_000)
        {
            throw new ArgumentOutOfRangeException(nameof(timeoutMilliseconds));
        }

        this.onTimeout = onTimeout;
        this.dueTime = TimeSpan.FromMilliseconds(timeoutMilliseconds);
    }

    public bool IsRunning { get; private set; }

    public void Start() => this.StartTimer();   

    /// <summary> Stops the timer, no callbacks any longer. </summary>
    public void Stop() => this.StopTimer();

    /// <summary> Resets the timer period: timer is stopped and then started again. </summary>
    public void ResetTimeout()
    {
        if (!this.IsRunning)
        {
            return;
        }

        this.Stop();
        this.Start();
    }

    /// <summary> NOT Invoked on the UI thread ! </summary>
    private void OnTimerTick(object? _)
    {
        this.Stop();
        this.onTimeout();
        this.Start();
    }

    private void StartTimer()
    {
        this.StopTimer(); 
        this.timer = new Timer(new TimerCallback(this.OnTimerTick));
        this.timer.Change(this.dueTime, TimeSpan.FromDays(13));
        this.IsRunning = true;
    }

    private void StopTimer()
    {
        this.timer?.Dispose();
        this.timer = null;

        this.IsRunning = false;
    }
}
