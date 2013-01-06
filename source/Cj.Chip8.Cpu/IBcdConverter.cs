namespace Cj.Chip8.Cpu
{
    public interface IBcdConverter
    {
        byte[] ConvertToBcd(byte value);
    }
}