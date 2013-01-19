using System.Diagnostics;

namespace Cj.Chip8.Cpu
{
    public class TimerClock : ITimerClock
    {
        private readonly Stopwatch _stopwatch;

        public TimerClock()
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        public double ElapsedSeconds
        {
            get { return _stopwatch.Elapsed.TotalSeconds; }
        }

        public void Reset()
        {
            _stopwatch.Restart();
        }
    }
}