namespace Cj.Chip8.Cpu
{
    public class InstructionPattern
    {
        public InstructionPattern(ushort opCode, ushort opCodePattern, ushort argumentPattern)
        {
            OpCode = opCode;
            OpCodePattern = opCodePattern;
            ArgumentPattern = argumentPattern;
        }

        public readonly ushort OpCode;
        public readonly ushort OpCodePattern;
        public readonly ushort ArgumentPattern;
    }
}