namespace Cj.Chip8.Cpu
{
    public class Instruction
    {
        public readonly ushort OpCode;
        public readonly ushort Argument;

        public Instruction(ushort opCode, ushort argument)
        {
            OpCode = opCode;
            Argument = argument;
        }

        public Instruction(ushort opCode) : this(opCode, 0) {}
    }
}