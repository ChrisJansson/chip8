namespace Cj.Chip8.Cpu
{
    public interface IDisplay
    {
        void Clear();
        byte Draw(byte x, byte y, byte[] sprite);
    }
}