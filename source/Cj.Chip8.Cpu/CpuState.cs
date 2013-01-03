namespace Cj.Chip8.Cpu
{
    public class CpuState
    {
        public readonly byte[] Memory = new byte[4096];
        public short ProgramCounter = 0;
        public byte StackPointer;

        public short[] Stack = new short[16];
        public byte[] V = new byte[16];
        public short I;
    }
}