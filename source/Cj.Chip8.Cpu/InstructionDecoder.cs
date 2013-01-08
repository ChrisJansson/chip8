namespace Cj.Chip8.Cpu
{
    public class InstructionDecoder
    {
        public void DecodeAndExecute(short instruction, IChip8Cpu cpu)
        {
            switch (instruction & 0xF000)
            {
                case 0x0000:
                    switch (instruction)
                    {
                        case 0x00E0:
                            cpu.Cls();
                            break;
                        case 0x00EE:
                            cpu.Ret();
                            break;
                    }
                    break;
                case 0x1000:
                    var jump = (short)(instruction & 0x0FFF);
                    cpu.Jump(jump);
                    break;
                case 0x2000:
                    var call = (short) (instruction & 0x0FFF);
                    cpu.Call(call);
                    break;
            }
        }
    }
}