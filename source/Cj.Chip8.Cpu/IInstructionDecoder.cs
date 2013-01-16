namespace Cj.Chip8.Cpu
{
    public interface IInstructionDecoder
    {
        void DecodeAndExecute(short instruction, IChip8Cpu cpu);
    }
}