using System;
using Cj.Chip8.Cpu;
using FluentAssertions;
using NUnit.Framework;

namespace Cj.Chip8.Test
{
    public class InstructionDecoderTests
    {
        private InstructionDecoder _decoder;

        [SetUp]
        public void SetUp()
        {
            _decoder = new InstructionDecoder();    
        }

        [Test]
        public void Should_not_decode_SYS()
        {
            const ushort instruction = 0x0000;
            Action act = () => _decoder.Decode(instruction);

            act.ShouldThrow<NotImplementedException>();
        }

        [Test]
        public void Should_decode_CLS()
        {
            const ushort instruction = 0x00E0;
            var decodedInstruction = _decoder.Decode(instruction);

            decodedInstruction.OpCode.Should().Be(instruction);
            decodedInstruction.Argument.Should().Be(0);
        }

        [Test]
        public void Should_decode_RET()
        {
            const ushort instruction = 0x00EE;
            var decodedInstruction = _decoder.Decode(instruction);

            decodedInstruction.OpCode.Should().Be(instruction);
            decodedInstruction.Argument.Should().Be(0);
        }

        [Test]
        public void Should_decode_Jump()
        {
            const ushort opCode = 0x1000;

            for (ushort argument = 0; argument < 0xFFF; argument++)
            {
                var instruction = (ushort) (opCode | argument);
                var decodedInstruction = _decoder.Decode(instruction);

                decodedInstruction.OpCode.Should().Be(opCode);
                decodedInstruction.Argument.Should().Be(argument);
            }
        }

        [Test]
        public void Should_decode_Call()
        {
            const ushort opCode = 0x2000;

            for (ushort argument = 0; argument < 0xFFF; argument++)
            {
                var instruction = (ushort)(opCode | argument);
                var decodedInstruction = _decoder.Decode(instruction);

                decodedInstruction.OpCode.Should().Be(opCode);
                decodedInstruction.Argument.Should().Be(argument);
            }
        }

        [Test]
        public void Should_decode_Se()
        {
            const ushort opCode = 0x3000;

            for (ushort argument = 0; argument < 0xFFF; argument++)
            {
                var instruction = (ushort)(opCode | argument);
                var decodedInstruction = _decoder.Decode(instruction);

                decodedInstruction.OpCode.Should().Be(opCode);
                decodedInstruction.Argument.Should().Be(argument);
            }
        }

        [Test]
        public void Should_decode_Sne()
        {
            const ushort opCode = 0x4000;

            for (ushort argument = 0; argument < 0xFFF; argument++)
            {
                var instruction = (ushort)(opCode | argument);
                var decodedInstruction = _decoder.Decode(instruction);

                decodedInstruction.OpCode.Should().Be(opCode);
                decodedInstruction.Argument.Should().Be(argument);
            }
        }
    }
}