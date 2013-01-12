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

        [Test]
        public void Should_call_cpu_correctly_when_decoding_3xkk()
        {
            var arguments = from register in CreateRange(0xF)
                            from constant in CreateRange(0xFF)
                            select new {register = (byte)register, constant = (byte)constant};

            foreach (var argument in arguments)
            {
                var instruction = 0x3000 | (argument.register << 8) | argument.constant;

                var argument1 = argument;
                ExecuteAndVerify((short)instruction, x => x.SeConstant(argument1.register, argument1.constant));
            }
        }

        [Test]
        public void Should_call_cpu_correctly_when_decoding_4xkk()
        {
            var arguments = from register in CreateRange(0xF)
                            from constant in CreateRange(0xFF)
                            select new { register = (byte)register, constant = (byte)constant };

            foreach (var argument in arguments)
            {
                var instruction = 0x4000 | (argument.register << 8) | argument.constant;

                var argument1 = argument;
                ExecuteAndVerify((short)instruction, x => x.SneConstant(argument1.register, argument1.constant));
            }
        }

        [Test]
        public void Should_call_cpu_correctly_when_decoding_5xy0()
        {
            var arguments = from vx in CreateRange(0xF)
                            from vy in CreateRange(0xF)
                            select new { vx = (byte)vx, vy = (byte)vy };

            foreach (var argument in arguments)
            {
                var instruction = 0x5000 | (argument.vx << 8) | (argument.vy << 4);

                var argument1 = argument;
                ExecuteAndVerify((short)instruction, x => x.Se(argument1.vx, argument1.vy));
            }
        }

        [Test]
        public void Should_call_cpu_correctly_when_decoding_6xkk()
        {
            var arguments = from vx in CreateRange(0xF)
                            from kk in CreateRange(0xFF)
                            select new { vx = (byte)vx, kk = (byte)kk };

            foreach (var argument in arguments)
            {
                var instruction = 0x6000 | (argument.vx << 8) | argument.kk;

                var argument1 = argument;
                ExecuteAndVerify((short)instruction, x => x.LdConstant(argument1.vx, argument1.kk));
            }
        }

        [Test]
        public void Should_call_cpu_correctly_when_decoding_7xkk()
        {
            var arguments = from vx in CreateRange(0xF)
                            from kk in CreateRange(0xFF)
                            select new { vx = (byte)vx, kk = (byte)kk };

            foreach (var argument in arguments)
            {
                var instruction = 0x7000 | (argument.vx << 8) | argument.kk;

                var argument1 = argument;
                ExecuteAndVerify((short)instruction, x => x.AddConstant(argument1.vx, argument1.kk));
            }
        }

        [Test]
        public void Should_call_cpu_correctly_when_decoding_8xy0()
        {
            var arguments = from vx in CreateRange(0xF)
                            from vy in CreateRange(0xF)
                            select new { vx = (byte)vx, vy = (byte)vy };

            foreach (var argument in arguments)
            {
                var instruction = 0x8000 | (argument.vx << 8) | (argument.vy << 4);

                var argument1 = argument;
                ExecuteAndVerify((short)instruction, x => x.Ld(argument1.vx, argument1.vy));
            }
        }

        [Test]
        public void Should_call_cpu_correctly_when_decoding_8xy1()
        {
            var arguments = from vx in CreateRange(0xF)
                            from vy in CreateRange(0xF)
                            select new { vx = (byte)vx, vy = (byte)vy };

            foreach (var argument in arguments)
            {
                var instruction = 0x8001 | (argument.vx << 8) | (argument.vy << 4);

                var argument1 = argument;
                ExecuteAndVerify((short)instruction, x => x.Or(argument1.vx, argument1.vy));
            }
        }

        [Test]
        public void Should_call_cpu_correctly_when_decoding_8xy2()
        {
            var arguments = from vx in CreateRange(0xF)
                            from vy in CreateRange(0xF)
                            select new { vx = (byte)vx, vy = (byte)vy };

            foreach (var argument in arguments)
            {
                var instruction = 0x8002 | (argument.vx << 8) | (argument.vy << 4);

                var argument1 = argument;
                ExecuteAndVerify((short)instruction, x => x.And(argument1.vx, argument1.vy));
            }
        }

        [Test]
        public void Should_call_cpu_correctly_when_decoding_8xy3()
        {
            var arguments = from vx in CreateRange(0xF)
                            from vy in CreateRange(0xF)
                            select new { vx = (byte)vx, vy = (byte)vy };

            foreach (var argument in arguments)
            {
                var instruction = 0x8003 | (argument.vx << 8) | (argument.vy << 4);

                var argument1 = argument;
                ExecuteAndVerify((short)instruction, x => x.Xor(argument1.vx, argument1.vy));
            }
        }

        [Test]
        public void Should_call_cpu_correctly_when_decoding_8xy4()
        {
            var arguments = from vx in CreateRange(0xF)
                            from vy in CreateRange(0xF)
                            select new { vx = (byte)vx, vy = (byte)vy };

            foreach (var argument in arguments)
            {
                var instruction = 0x8004 | (argument.vx << 8) | (argument.vy << 4);

                var argument1 = argument;
                ExecuteAndVerify((short)instruction, x => x.AddCarry(argument1.vx, argument1.vy));
            }
        }

        [Test]
        public void Should_call_cpu_correctly_when_decoding_8xy5()
        {
            var arguments = from vx in CreateRange(0xF)
                            from vy in CreateRange(0xF)
                            select new { vx = (byte)vx, vy = (byte)vy };

            foreach (var argument in arguments)
            {
                var instruction = 0x8005 | (argument.vx << 8) | (argument.vy << 4);

                var argument1 = argument;
                ExecuteAndVerify((short)instruction, x => x.Sub(argument1.vx, argument1.vy));
            }
        }

        [Test]
        public void Should_call_cpu_correctly_when_decoding_8xy6()
        {
            var arguments = from vx in CreateRange(0xF)
                            from vy in CreateRange(0xF)
                            select new { vx = (byte)vx, vy = (byte)vy };

            foreach (var argument in arguments)
            {
                var instruction = 0x8006 | (argument.vx << 8) | (argument.vy << 4);

                var argument1 = argument;
                ExecuteAndVerify((short)instruction, x => x.Shr(argument1.vx));
            }
        }

        [Test]
        public void Should_call_cpu_correctly_when_decoding_8xy7()
        {
            var arguments = from vx in CreateRange(0xF)
                            from vy in CreateRange(0xF)
                            select new { vx = (byte)vx, vy = (byte)vy };

            foreach (var argument in arguments)
            {
                var instruction = 0x8007 | (argument.vx << 8) | (argument.vy << 4);

                var argument1 = argument;
                ExecuteAndVerify((short)instruction, x => x.Subn(argument1.vx, argument1.vy));
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