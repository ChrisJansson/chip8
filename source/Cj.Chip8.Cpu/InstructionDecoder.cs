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
                case 0x3000:
                    cpu.SeConstant((byte) ((instruction >> 8) & 0xF), (byte) (instruction & 0xFF));
                    break;
                case 0x4000:
                    cpu.SneConstant((byte) ((instruction >> 8) & 0xF), (byte) (instruction & 0xFF));
                    break;
                case 0x5000:
                    cpu.Se((byte)((instruction >> 8) & 0xF), (byte)((instruction >> 4) & 0xF));
                    break;
                case 0x6000:
                    cpu.LdConstant((byte)((instruction >> 8) & 0xF), (byte)(instruction & 0xFF));
                    break;
                case 0x7000:
                    cpu.AddConstant((byte)((instruction >> 8) & 0xF), (byte)(instruction & 0xFF));
                    break;
                case 0x8000:
                    switch (instruction & 0x000F)
                    {
                        case 0x0000:
                            cpu.Ld((byte)((instruction >> 8) & 0xF), (byte)((instruction >> 4) & 0xF));
                            break;
                        case 0x0001:
                            cpu.Or((byte)((instruction >> 8) & 0xF), (byte)((instruction >> 4) & 0xF));
                            break;
                        case 0x0002:
                            cpu.And((byte)((instruction >> 8) & 0xF), (byte)((instruction >> 4) & 0xF));
                            break;
                        case 0x0003:
                            cpu.Xor((byte)((instruction >> 8) & 0xF), (byte)((instruction >> 4) & 0xF));
                            break;
                        case 0x0004:
                            cpu.AddCarry((byte)((instruction >> 8) & 0xF), (byte)((instruction >> 4) & 0xF));
                            break;
                        case 0x0005:
                            cpu.Sub((byte)((instruction >> 8) & 0xF), (byte)((instruction >> 4) & 0xF));
                            break;
                        case 0x0006:
                            cpu.Shr((byte)((instruction >> 8) & 0xF));
                            break;
                        case 0x0007:
                            cpu.Subn((byte)((instruction >> 8) & 0xF), (byte)((instruction >> 4) & 0xF));
                            break;
                    }
                    break;
                case 0x9000:
                    cpu.Sne((byte)((instruction >> 8) & 0xF), (byte)((instruction >> 4) & 0xF));
                    break;
                case 0xA000:
                    cpu.Ldi((short) (instruction & 0x0FFF));
                    break;
                case 0xB000:
                    cpu.JumpV0Offset((short)(instruction & 0x0FFF));
                    break;
                case 0xC000:
                    cpu.Rnd((byte) ((instruction >> 8) & 0xF), (byte) (instruction & 0xFF));
                    break;
            }
        }
    }
}