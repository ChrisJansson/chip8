using Cj.Chip8.Cpu;
using FluentAssertions;
using NUnit.Framework;

namespace Cj.Chip8.Test
{
    [TestFixture]
    public class CpuStateTests
    {
        private CpuState _cpuState;

        [SetUp]
        public void SetUp()
        {
            _cpuState = new CpuState();
        }

        [Test]
        public void Should_have_4_kilo_bytes_of_memory()
        {
            _cpuState.Memory.Should().BeOfType<byte[]>();
            _cpuState.Memory.Should().HaveCount(4096);
        }

        [Test]
        public void Should_have_16_bit_program_counter()
        {
            var type = _cpuState.ProgramCounter.GetType();
            type.Should().Be(typeof (short));
        }

        [Test]
        public void Should_have_8_bit_stack_pointer()
        {
            _cpuState.StackPointer.GetType().Should().Be(typeof (byte));
        }

        [Test]
        public void Should_have_a_stack_with_16_values_16_bits_each()
        {
            _cpuState.Stack.Should().HaveCount(16);
            _cpuState.Stack.Should().BeOfType<short[]>();
        }

        [Test]
        public void Should_have_16_8byte_registers()
        {
            _cpuState.Vx.Should().HaveCount(16);
            _cpuState.Vx.Should().BeOfType<byte[]>();
        }
    }
}