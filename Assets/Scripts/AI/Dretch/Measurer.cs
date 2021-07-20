using System;
using System.Diagnostics;

namespace Dretch
{
    public struct Measurer : IDisposable
    {
        Stopwatch stopwatch;
        public Measurer(Stopwatch stopwatch)
        {
            stopwatch.Start();
            this.stopwatch = stopwatch;
        }
        public void Dispose()
        {
            stopwatch.Stop();
        }
    }
}
