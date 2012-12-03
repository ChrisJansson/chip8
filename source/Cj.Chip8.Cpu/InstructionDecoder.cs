using System;
using System.Collections.Generic;

namespace Cj.Chip8.Cpu
{
    public class InstructionDecoder
    {
        private readonly List<InstructionPattern> _instructionMapping;

        public InstructionDecoder()
        {
            _instructionMapping = new List<InstructionPattern>
                                      {
                                          new InstructionPattern(0x00E0, 0xFFFF, 0),
                                          new InstructionPattern(0x00EE, 0xFFFF, 0),
                                          new InstructionPattern(0x1000, 0xF000, 0x0FFF),
                                          new InstructionPattern(0x2000, 0xF000, 0x0FFF),
                                          new InstructionPattern(0x3000, 0xF000, 0x0FFF),
                                          new InstructionPattern(0x4000, 0xF000, 0x0FFF),
                                      };
        }

        public Instruction Decode(ushort instruction)
        {
            var instructionPattern = GetInstructionPattern(instruction);

            var opCode = instructionPattern.OpCode;
            var argumentPattern = instructionPattern.ArgumentPattern;

            var argument = (ushort)(argumentPattern & instruction);
            return new Instruction(opCode, argument);
        }

        private InstructionPattern GetInstructionPattern(ushort instruction)
        {
            foreach (var func in _instructionMapping)
            {
                var opCodePattern = func.OpCodePattern;
                var opCode = func.OpCode;
                var matches = (instruction & opCodePattern) == opCode;

                if (matches)
                {
                    return func;
                }
            }

            throw new NotImplementedException();
        }
    }
}