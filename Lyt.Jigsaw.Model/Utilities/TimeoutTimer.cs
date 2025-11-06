namespace Lyt.Jigsaw.Model.Utilities;

public sealed class TimeoutTimer
{
    private readonly Timer dispatcherTimer;
    private readonly Action onTimeout;

    public TimeoutTimer(Action onTimeout, int timeoutMilliseconds = 1042)
    {
        if (timeoutMilliseconds < 0 || timeoutMilliseconds > 24 * 60 * 60 * 1_000)
        {
            throw new ArgumentOutOfRangeException(nameof(timeoutMilliseconds));
        }

        //this.onTimeout = onTimeout;
        //this.dispatcherTimer = new Timer()
        //{
        //    Interval = TimeSpan.FromMilliseconds(timeoutMilliseconds),
        //    IsEnabled = false,
        //};
        //this.dispatcherTimer.Tick += this.OnDispatcherTimerTick;
    }

    public bool IsRunning { get; private set; }

    public void Start()
    {
        // Setting IsEnabled to false when the timer is started stops the timer.
        // Setting IsEnabled to true when the timer is stopped starts the timer.
        // Start sets IsEnabled to true.
        // Start resets the timer Interval.  <=== Meh ! 
        // this.dispatcherTimer.Start();
        this.IsRunning = true;
    }

    /// <summary> Stops the timer, no callbacks any longer. </summary>
    public void Stop() => this.StopTimer();

    /// <summary> Changes the timer period, timer is stopped and needs to be started again </summary>
    public void Change(int timeoutMilliseconds = 1042)
    {
        if (timeoutMilliseconds < 0 || timeoutMilliseconds > 24 * 60 * 60 * 1_000)
        {
            throw new ArgumentOutOfRangeException(nameof(timeoutMilliseconds));
        }

        this.StopTimer();
        // this.dispatcherTimer.Interval = TimeSpan.FromMilliseconds(timeoutMilliseconds);
    }

    /// <summary> Resets the timer period: timer is stopped and then started again. </summary>
    public void Reset()
    {
        if (!this.IsRunning)
        {
            return;
        }

        // Calling Start again will reset the timer, they say...
        // But: https://github.com/MicrosoftDocs/feedback/issues/1723
        // Stop and Start fixes it 
        this.Stop();
        this.Start();
    }

    /// <summary> Invoked on the UI thread ! </summary>
    private void OnDispatcherTimerTick(object? sender, EventArgs e) => this.onTimeout();

    private void StopTimer()
    {
        // this.dispatcherTimer.Stop();
        this.IsRunning = false;
    }
}
