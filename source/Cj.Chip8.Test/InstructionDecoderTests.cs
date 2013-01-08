using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Cj.Chip8.Cpu;
using Moq;
using NUnit.Framework;

namespace Cj.Chip8.Test
{
    [TestFixture]
    public class InstructionDecoderTests
    {
        private Mock<IChip8Cpu> _cpu;
        private InstructionDecoder _instructionDecoder;

        [SetUp]
        public void SetUp()
        {
            _cpu = new Mock<IChip8Cpu>(MockBehavior.Strict);

            _instructionDecoder = new InstructionDecoder();
        }

        [Test]
        public void Should_call_CLS_when_decoding_00E0()
        {
            const short instruction = 0x00E0;

            ExecuteAndVerify(instruction, x => x.Cls());
        }

        [Test]
        public void Should_call_RET_when_decoding_00EE()
        {
            const short instruction = 0x00EE;

            ExecuteAndVerify(instruction, x => x.Ret());
        }

        [Test]
        public void Should_call_JMP_when_decoding_1nnn()
        {
            var instructions = CreateRange(0xFFF)
                .Select(x => (short)x)
                .ToList();

            foreach (var targetAddress in instructions)
            {
                var address = targetAddress;

                var instruction = (short)(0x1000 | targetAddress);

                ExecuteAndVerify(instruction, x => x.Jump(address));
            }
        }

        [Test]
        public void Should_call_CALL_when_decoding_2nnn()
        {
            var instructions = CreateRange(0xFFF)
                .Select(x => (short)x)
                .ToList();

            foreach (var targetAddress in instructions)
            {
                var address = targetAddress;

                var instruction = (short)(0x2000 | targetAddress);

                ExecuteAndVerify(instruction, x => x.Call(address));
            }
        }

        private void ExecuteAndVerify(short instruction, Expression<Action<IChip8Cpu>> verifier)
        {
            _cpu.Setup(verifier);

            _instructionDecoder.DecodeAndExecute(instruction, _cpu.Object);

            _cpu.Verify(verifier);
        }

        private IEnumerable<int> CreateRange(int maxValue)
        {
            return Enumerable.Range(0, maxValue + 1);
        }
    }
}