namespace Cj.Chip8.Cpu
{
    public interface ITimerClock
    {
        double ElapsedSeconds { get; }
        void Reset();
    }
}