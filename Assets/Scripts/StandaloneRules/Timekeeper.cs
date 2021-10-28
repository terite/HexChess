using System.Diagnostics;
using System.Threading;

public class Timekeeper
{
    public ReaderWriterLock rwl {get; private set;} = new ReaderWriterLock();
    public float elapsed {get; private set;} = 0;
    public delegate void Elapsed();
    public Elapsed onTimerElapsed;

    public bool paused {get; private set;} = true;
    
    private Thread thread;
    public float? duration;

    public Timekeeper(float? duration = null)
    {
        this.duration = duration;
        thread = new Thread(() => Timer());
        thread.IsBackground = true;
        thread.Start();
    }

    public void Pause() => paused = true;
    public void Play() => paused = false;
    public void Stop()
    {
        if(thread != null && thread.IsAlive)
        {
            try{
                thread.Abort();
            }catch{}
        }
    }

    public void Timer()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        double lastElapsedVal = 0;

        while(true)
        {
            if(!paused)
            {
                double deltaTime = stopwatch.Elapsed.TotalSeconds - lastElapsedVal;
                rwl.AcquireWriterLock(1);
                try{
                    elapsed += (float)deltaTime;
                }finally{
                    rwl.ReleaseWriterLock();
                }
                lastElapsedVal = stopwatch.Elapsed.TotalSeconds;

                rwl.AcquireReaderLock(1);
                if(duration.HasValue && elapsed >= duration.Value)
                {
                    onTimerElapsed?.Invoke();
                    rwl.ReleaseReaderLock();
                    break;
                }
                else
                    rwl.ReleaseReaderLock();
            }
            else
                lastElapsedVal = stopwatch.Elapsed.TotalSeconds;

            Thread.Sleep(100);
        }
        
        stopwatch.Stop();
    }

    public void SetTime(float seconds)
    {
        rwl.AcquireWriterLock(5);
        try{
            elapsed = seconds;
        }finally{
            rwl.ReleaseWriterLock();
        }
    }
}