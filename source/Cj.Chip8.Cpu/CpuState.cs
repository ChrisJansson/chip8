using System;

namespace Cj.Chip8.Cpu
{
    public class CpuState
    {
        public CpuState()
        {
            AddHexaDecimalDigit(new byte[] { 0xF0, 0x90, 0x90, 0x90, 0xF0 }, 0);
            AddHexaDecimalDigit(new byte[] { 0x20, 0x60, 0x20, 0x20, 0x70 }, 5);
            AddHexaDecimalDigit(new byte[] { 0xF0, 0x10, 0xF0, 0x80, 0xF0 }, 10);
            AddHexaDecimalDigit(new byte[] { 0xF0, 0x10, 0xF0, 0x10, 0xF0 }, 15);
            AddHexaDecimalDigit(new byte[] { 0x90, 0x90, 0xF0, 0x10, 0x10 }, 20);
            AddHexaDecimalDigit(new byte[] { 0xF0, 0x80, 0xF0, 0x10, 0xF0 }, 25);
            AddHexaDecimalDigit(new byte[] { 0xF0, 0x80, 0xF0, 0x90, 0xF0 }, 30);
            AddHexaDecimalDigit(new byte[] { 0xF0, 0x10, 0x20, 0x40, 0x40 }, 35);
            AddHexaDecimalDigit(new byte[] { 0xF0, 0x90, 0xF0, 0x90, 0xF0 }, 40);
            AddHexaDecimalDigit(new byte[] { 0xF0, 0x90, 0xF0, 0x10, 0xF0 }, 45);
            AddHexaDecimalDigit(new byte[] { 0xF0, 0x90, 0xF0, 0x90, 0x90 }, 50);
            AddHexaDecimalDigit(new byte[] { 0xE0, 0x90, 0xE0, 0x90, 0xE0 }, 55);
            AddHexaDecimalDigit(new byte[] { 0xF0, 0x80, 0x80, 0x80, 0xF0 }, 60);
            AddHexaDecimalDigit(new byte[] { 0xE0, 0x90, 0x90, 0x90, 0xE0 }, 65);
            AddHexaDecimalDigit(new byte[] { 0xF0, 0x80, 0xF0, 0x80, 0xF0 }, 70);
            AddHexaDecimalDigit(new byte[] { 0xF0, 0x80, 0xF0, 0x80, 0x80 }, 75);
        }

        private void AddHexaDecimalDigit(byte[] digit, int memoryOffset)
        {
            Array.Copy(digit, 0, Memory, memoryOffset, digit.Length);
        }

        public readonly byte[] Memory = new byte[4096];
        public short ProgramCounter = 0;
        public byte StackPointer;

        public short[] Stack = new short[16];
        public byte[] V = new byte[16];
        public short I;

        public byte SoundTimer;
        public byte DelayTimer;
    }
}