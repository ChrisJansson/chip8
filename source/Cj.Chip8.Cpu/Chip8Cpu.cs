namespace Cj.Chip8.Cpu
{
    public class Chip8Cpu
    {
        private readonly IDisplay _display;

        public Chip8Cpu(IDisplay display)
        {
            _display = display;
            State = new CpuState();
        }

        public CpuState State { get; set; }

        public void Cls()
        {
            _display.Clear();

            State.ProgramCounter += 2;
        }

        public void Jump(short address)
        {
            State.ProgramCounter = address;
        }

        public void Call(short address)
        {
            State.Stack[State.StackPointer++] = State.ProgramCounter;
            State.ProgramCounter = address;
        }

        public void SeConstant(byte register, byte kk)
        {
            var registerValue = State.V[register];
            var valueToCompare = kk;

            if (registerValue == valueToCompare)
                State.ProgramCounter += 4;
            else
                State.ProgramCounter += 2;
        }

        public void SneConstant(byte register, byte kk)
        {
            var registerValue = State.V[register];
            var valueToCompare = kk;

            if (registerValue != valueToCompare)
                State.ProgramCounter += 4;
            else
                State.ProgramCounter += 2;
        }

        public void Ret()
        {
            State.ProgramCounter = State.Stack[--State.StackPointer];
        }

        public void Se(byte vx, byte vy)
        {
            var left = State.V[vx];
            var right = State.V[vy];

            if (left == right)
                State.ProgramCounter += 4;
            else
                State.ProgramCounter += 2;
        }

        public void LdConstant(byte vx, byte argument)
        {
            State.V[vx] = argument;
            State.ProgramCounter += 2;
        }

        public void AddConstant(byte vx, byte argument)
        {
            State.V[vx] += argument;
            State.ProgramCounter += 2;
        }

        public void Ld(byte vx, byte vy)
        {
            State.V[vx] = State.V[vy];
            State.ProgramCounter += 2;
        }

        public void Or(byte vx, byte vy)
        {
            State.V[vx] = (byte) (State.V[vx] | State.V[vy]);
            State.ProgramCounter += 2;
        }
    }
}