namespace Luthetus.Common.RazorLib.Reactives.Models;

public class ThrottleAvailability
{
    private Timer? _throttleTimer;

    public ThrottleAvailability(TimeSpan throttleTimeSpan)
    {
        ThrottleTimeSpan = throttleTimeSpan;
    }

    public TimeSpan ThrottleTimeSpan { get; }
    public Task DelayTask { get; private set; } = Task.CompletedTask;

    public bool CheckAvailability(Func<Task> onBecameAvailableCallback)
    {
        if (DelayTask.IsCompleted)
        {
            DelayTask = Task.Run(async () =>
            {
                await onBecameAvailableCallback.Invoke();
            });

            return true;
        }
        else
        {
            return false;
        }
    }

    //public bool CheckAvailability(Action onBecameAvailableCallback)
    //{
    //    if (_throttleTimer is null)
    //    {
    //        _throttleTimer = new Timer(
    //            callback: _ => 
    //            {
    //                _throttleTimer?.Dispose();
    //                _throttleTimer = null;
    //                onBecameAvailableCallback.Invoke();
    //            },
    //            state: null,
    //            dueTime: ThrottleTimeSpan,
    //            period: Timeout.InfiniteTimeSpan);
    //
    //        return true;
    //    }
    //    else
    //    {
    //        return false;
    //    }
    //}

    /// <summary>
    /// This <see cref="TimeSpan"/> represents '1000ms / 60 = 16.6ms', whether this is equivalent to 60fps is unknown.
    /// </summary>
    public static readonly TimeSpan Sixty_Frames_Per_Second = TimeSpan.FromMilliseconds(16.6);

    /// <summary>
    /// This <see cref="TimeSpan"/> represents '1000ms / 30 = 33.3ms', whether this is equivalent to 30fps is unknown.
    /// </summary>
    public static readonly TimeSpan Thirty_Frames_Per_Second = TimeSpan.FromMilliseconds(33.3);
}