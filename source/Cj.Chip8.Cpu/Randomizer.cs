using System;

namespace Cj.Chip8.Cpu
{
    public class Randomizer : IRandomizer
    {
        private readonly Random _random = new Random();
        
        public byte GetNext()
        {
            return (byte) _random.Next(0, 256);
        }
    }
}