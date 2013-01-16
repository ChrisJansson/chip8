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
        public void Program_counter_should_be_0x200_from_the_start()
        {
            _cpuState.ProgramCounter.Should().Be(0x200);
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
            type.Should().Be(typeof(short));
        }

        [Test]
        public void Should_have_8_bit_stack_pointer()
        {
            _cpuState.StackPointer.GetType().Should().Be(typeof(byte));
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
            _cpuState.V.Should().HaveCount(16);
            _cpuState.V.Should().BeOfType<byte[]>();
        }

        [Test]
        public void Should_have_address_register()
        {
            _cpuState.I.GetType().Should().Be(typeof(short));
        }

        [Test]
        public void Should_have_delay_timer()
        {
            _cpuState.DelayTimer.GetType().Should().Be(typeof(byte));
        }

        [Test]
        public void Should_have_sound_timer()
        {
            _cpuState.DelayTimer.GetType().Should().Be(typeof(byte));
        }

        [Test]
        public void Should_have_digit_sprites_in_memory()
        {
            var zero = new byte[] { 0xF0, 0x90, 0x90, 0x90, 0xF0 };
            AssertHexadecimalDigit(zero, 0);

            var one = new byte[] { 0x20, 0x60, 0x20, 0x20, 0x70 };
            AssertHexadecimalDigit(one, 5);

            var two = new byte[] { 0xF0, 0x10, 0xF0, 0x80, 0xF0 };
            AssertHexadecimalDigit(two, 10);

            var three = new byte[] { 0xF0, 0x10, 0xF0, 0x10, 0xF0 };
            AssertHexadecimalDigit(three, 15);

            var four = new byte[] { 0x90, 0x90, 0xF0, 0x10, 0x10 };
            AssertHexadecimalDigit(four, 20);

            var five = new byte[] { 0xF0, 0x80, 0xF0, 0x10, 0xF0 };
            AssertHexadecimalDigit(five, 25);

            var six = new byte[] { 0xF0, 0x80, 0xF0, 0x90, 0xF0 };
            AssertHexadecimalDigit(six, 30);

            var seven = new byte[] { 0xF0, 0x10, 0x20, 0x40, 0x40 };
            AssertHexadecimalDigit(seven, 35);

            var eight = new byte[] { 0xF0, 0x90, 0xF0, 0x90, 0xF0 };
            AssertHexadecimalDigit(eight, 40);

            var nine = new byte[] { 0xF0, 0x90, 0xF0, 0x10, 0xF0 };
            AssertHexadecimalDigit(nine, 45);

            var a = new byte[] { 0xF0, 0x90, 0xF0, 0x90, 0x90 };
            AssertHexadecimalDigit(a, 50);

            var b = new byte[] { 0xE0, 0x90, 0xE0, 0x90, 0xE0 };
            AssertHexadecimalDigit(b, 55);

            var c = new byte[] { 0xF0, 0x80, 0x80, 0x80, 0xF0 };
            AssertHexadecimalDigit(c, 60);

            var d = new byte[] { 0xE0, 0x90, 0x90, 0x90, 0xE0 };
            AssertHexadecimalDigit(d, 65);

            var e = new byte[] { 0xF0, 0x80, 0xF0, 0x80, 0xF0 };
            AssertHexadecimalDigit(e, 70);

            var f = new byte[] { 0xF0, 0x80, 0xF0, 0x80, 0x80 };
            AssertHexadecimalDigit(f, 75);
        }

        private void AssertHexadecimalDigit(byte[] digit, int memoryLocation)
        {
            var state = new CpuState();

            for (var i = 0; i < digit.Length; i++)
            {
                var offset = (short)(memoryLocation + i);
                state.Memory[offset].Should().Be(digit[i]);
            }
        }
    }
}