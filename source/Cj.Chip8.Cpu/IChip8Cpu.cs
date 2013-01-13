namespace Cj.Chip8.Cpu
{
    public interface IChip8Cpu
    {
        void Cls();
        void Jump(short address);
        void Call(short address);
        void SeConstant(byte register, byte kk);
        void SneConstant(byte register, byte kk);
        void Ret();
        void Se(byte vx, byte vy);
        void LdConstant(byte vx, byte argument);
        void AddConstant(byte vx, byte argument);
        void Ld(byte vx, byte vy);
        void Or(byte vx, byte vy);
        void And(byte vx, byte vy);
        void Xor(byte vx, byte vy);
        void AddCarry(byte vx, byte vy);
        void Sub(byte vx, byte vy);
        void Shr(byte vx);
        void Subn(int vx, int vy);
        void Shl(byte vx);
        void Sne(byte vx, byte vy);
        void Ldi(short address);
        void JumpV0Offset(short address);
        void Rnd(byte vx, byte kk);
        void Drw(byte vx, byte vy, byte height);
        void Skp(byte vx);
        void Sknp(byte vx);
        void Lddt(byte vx);
        void Ldk(byte vx);
        void SetDt(byte vx);
        void SetSt(byte vx);
        void AddI(byte vx);
        void Ldf(byte vx);
        void Ldb(byte vx);
        void CopyRegisters(byte vx);
        void CopyMemory(byte vx);
    }
}