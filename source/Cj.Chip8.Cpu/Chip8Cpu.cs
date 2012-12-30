﻿namespace Cj.Chip8.Cpu
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

        public void Jump(short argument)
        {
            State.ProgramCounter = argument;
        }

        public void Call(short argument)
        {
            State.Stack[State.StackPointer++] = State.ProgramCounter;
            State.ProgramCounter = argument;
        }

        public void Se(short argument)
        {
            var register = (0xF00 & argument) >> 8;
            short registerValue = State.Vx[register];
            short valueToCompare = (short) (0xFF & argument);

            if (registerValue == valueToCompare)
                State.ProgramCounter += 4;
            else
                State.ProgramCounter += 2;
        }

        public void Ret()
        {
            State.ProgramCounter = State.Stack[--State.StackPointer];
        }
    }
}