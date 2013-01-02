using System;
using Cj.Chip8.Cpu;
using Moq;
using NUnit.Framework;
using FluentAssertions;

namespace Cj.Chip8.Test
{
    [TestFixture]
    public class DataTypeTests
    {
        [Test]
        public void Short_should_be_2_bytes()
        {
            var size = sizeof(short);
            size.Should().Be(2);
        }
    }

    [TestFixture]
    public class CpuTests
    {
        private Chip8Cpu _cpu;
        private Mock<IDisplay> _display;

        [SetUp]
        public void SetUp()
        {
            _display = new Mock<IDisplay>();

            _cpu = new Chip8Cpu(_display.Object);
        }

        [Test]
        public void Should_clear_display_on_Cls()
        {
            const short initialProgramCounter = 4;
            ProgramCounter = initialProgramCounter;

            Execute(x => x.Cls);

            _display.Verify(x => x.Clear());
            ProgramCounter.Should().Be(initialProgramCounter + 2);
        }

        [Test]
        public void Should_set_program_counter_to_adress_at_top_of_stack_then_subtract_one_from_stack_pointer_on_Ret()
        {
            _cpu.State.Stack[0] = 4;
            _cpu.State.StackPointer = 1;

            var state = Execute(x => x.Ret);

            ProgramCounter.Should().Be(4);
            state.StackPointer.Should().Be(0);
        }

        [Test]
        public void Should_set_program_counter_to_jump_argument()
        {
            for (short argument = 0; argument <= 0xFFF; argument++)
            {
                Execute(x => x.Jump, argument);
                ProgramCounter.Should().Be(argument);
            }
        }

        [Test]
        public void Should_store_current_add_program_counter_on_stack_then_increase_stack_pointer_and_set_program_counter_to_argument_on_Call()
        {
            for (short argument = 0; argument <= 0xFFF; argument++)
            {
                const short initialProgramCounter = 0x600;
                ProgramCounter = initialProgramCounter;

                var state = Execute(x => x.Call, argument);

                ProgramCounter.Should().Be(argument);
                state.StackPointer.Should().Be(1);
                state.Stack[0].Should().Be(initialProgramCounter);

                ResetCpuState();
            }
        }

        [Test]
        public void Should_increment_program_counter_by_two_instructions_when_Vx_is_equal_to_argument_on_SE()
        {
            const int argumentMax = 255;
            TestForAllRegistersAndArgumentRange((register, argument) =>
            {
                _cpu.State.V[register] = argument;

                const short initialProgramCounter = 4;
                ProgramCounter = initialProgramCounter;

                var state = Execute(x => x.SeConstant, register, argument);

                state.ProgramCounter.Should().Be(initialProgramCounter + 4);
            },
            argumentMax);
        }

        [Test]
        public void Should_increment_program_counter_by_one_instructions_when_Vx_is_not_equal_to_argument_on_SE()
        {
            const int argumentMax = 255;
            TestForAllRegistersAndArgumentRange((register, argument) =>
                {
                    _cpu.State.V[register] = (byte)FindValueNotEqualTo(argument, argumentMax);

                    const short initialProgramCounter = 4;
                    ProgramCounter = initialProgramCounter;

                    var state = Execute(x => x.SeConstant, register, argument);

                    state.ProgramCounter.Should().Be(initialProgramCounter + 2);
                },
                argumentMax);
        }

        [Test]
        public void Should_increment_program_counter_by_one_when_Vx_is_equal_to_argument_on_SNE()
        {
            const int argumentMax = 255;
            TestForAllRegistersAndArgumentRange((register, argument) =>
            {
                _cpu.State.V[register] = argument;

                const short initialProgramCounter = 4;
                ProgramCounter = initialProgramCounter;

                var state = Execute(x => x.SneConstant, register, argument);

                state.ProgramCounter.Should().Be(initialProgramCounter + 2);
            },
                argumentMax);
        }

        [Test]
        public void Should_increment_program_counter_by_two_when_Vx_is_not_equal_to_argument_on_SNE()
        {
            const int argumentMax = 255;
            TestForAllRegistersAndArgumentRange((register, argument) =>
            {
                _cpu.State.V[register] = (byte)FindValueNotEqualTo(argument, argumentMax);

                const short initialProgramCounter = 4;
                ProgramCounter = initialProgramCounter;

                var state = Execute(x => x.SneConstant, register, argument);

                state.ProgramCounter.Should().Be(initialProgramCounter + 4);
            },
                argumentMax);
        }

        [Test]
        public void Should_increment_program_counter_by_two_when_Vx_is_equal_to_Vy_on_SE()
        {
            const int argumentMax = 15;
            TestForAllRegistersAndArgumentRange((vx, vy) =>
                {
                    const byte equalValue = 255;

                    _cpu.State.V[vx] = equalValue;
                    _cpu.State.V[vy] = equalValue;

                    const short initialProgramCounter = 4;
                    ProgramCounter = initialProgramCounter;

                    var state = Execute(x => x.Se, vx, vy);

                    state.ProgramCounter.Should().Be(initialProgramCounter + 4);
                },
                argumentMax);
        }

        [Test]
        public void Should_increment_program_counter_by_one_when_Vx_is_not_equal_to_Vy_on_SE()
        {
            const int argumentMax = 15;
            TestForAllRegistersAndArgumentRange((vx, vy) =>
                {
                    if (vx == vy)
                        return;

                    const byte firstValue = 255;
                    byte otherValue = (byte)FindValueNotEqualTo(firstValue, byte.MaxValue);

                    _cpu.State.V[vx] = firstValue;
                    _cpu.State.V[vy] = otherValue;

                    const short initialProgramCounter = 4;
                    ProgramCounter = initialProgramCounter;

                    var state = Execute(x => x.Se, vx, vy);

                    state.ProgramCounter.Should().Be(initialProgramCounter + 2);
                },
                argumentMax);
        }

        [Test]
        public void Should_load_value_into_register_vx_and_increment_program_counter_on_LD()
        {
            const int argumentMax = byte.MaxValue;
            TestForAllRegistersAndArgumentRange((vx, argument) =>
                {
                    const short initialProgramCounter = 4;
                    ProgramCounter = initialProgramCounter;

                    var state = Execute(x => x.LdConstant, vx, argument);

                    state.ProgramCounter.Should().Be(initialProgramCounter + 2);
                    state.V[vx].Should().Be(argument);
                },
                argumentMax);
        }

        [Test]
        public void Should_add_value_to_vx_and_increment_program_counter_on_ADD()
        {
            const int argumentMax = byte.MaxValue;
            TestForAllRegistersAndArgumentRange((vx, argument) =>
                {
                    _cpu.State.V[vx] = 15;
                    
                    const short initialProgramCounter = 4;
                    ProgramCounter = initialProgramCounter;

                    var state = Execute(x => x.AddConstant, vx, argument);

                    state.ProgramCounter.Should().Be(initialProgramCounter + 2);
                    state.V[vx].Should().Be((byte) (15 + argument));
                }, 
                argumentMax);
        }

        [Test]
        public void Should_load_vy_into_vx_and_increment_program_counter_on_LD()
        {
            const int argumentMax = 15;
            TestForAllRegistersAndArgumentRange((vx, vy) =>
                {
                    _cpu.State.V[vx] = 0;
                    _cpu.State.V[vy] = 255;

                    const short initalProgramCounter = 4;
                    ProgramCounter = initalProgramCounter;

                    var state = Execute(x => x.Ld, vx, vy);

                    state.ProgramCounter.Should().Be(initalProgramCounter + 2);
                    state.V[vx].Should().Be(255);
                }, 
            argumentMax);
        }

        [Test]
        public void Should_bitwise_or_vx_and_vy_then_store_result_in_vx_and_increment_program_counter_on_OR()
        {
            const int argumentMax = 15;
            TestForAllRegistersAndArgumentRange((vx, vy) =>
            {
                _cpu.State.V[vx] = 0x0A; //00001010
                _cpu.State.V[vy] = 0x0C; //00001100
                var expectedResult = _cpu.State.V[vx] | _cpu.State.V[vy];

                const short initalProgramCounter = 4;
                ProgramCounter = initalProgramCounter;

                var state = Execute(x => x.Or, vx, vy);

                state.ProgramCounter.Should().Be(initalProgramCounter + 2);
                state.V[vx].Should().Be((byte) expectedResult);
            },
            argumentMax);
        }

        [Test]
        public void Should_bitwise_and_vx_and_vy_then_store_result_in_vx_and_increment_program_counter_on_AND()
        {
            const int argumentMax = 15;
            TestForAllRegistersAndArgumentRange((vx, vy) =>
            {
                _cpu.State.V[vx] = 0x0A; //00001010
                _cpu.State.V[vy] = 0x0C; //00001100
                var expectedResult = _cpu.State.V[vx] & _cpu.State.V[vy];

                const short initalProgramCounter = 4;
                ProgramCounter = initalProgramCounter;

                var state = Execute(x => x.And, vx, vy);

                state.ProgramCounter.Should().Be(initalProgramCounter + 2);
                state.V[vx].Should().Be((byte)expectedResult);
            },
            argumentMax);
        }

        [Test]
        public void Should_bitwise_xor_vx_and_vy_then_store_result_in_vx_and_increment_program_counter_on_XOR()
        {
            const int argumentMax = 15;
            TestForAllRegistersAndArgumentRange((vx, vy) =>
            {
                _cpu.State.V[vx] = 0x0A; //00001010
                _cpu.State.V[vy] = 0x0C; //00001100
                var expectedResult = _cpu.State.V[vx] ^ _cpu.State.V[vy];

                const short initalProgramCounter = 4;
                ProgramCounter = initalProgramCounter;

                var state = Execute(x => x.Xor, vx, vy);

                state.ProgramCounter.Should().Be(initalProgramCounter + 2);
                state.V[vx].Should().Be((byte)expectedResult);
            },
            argumentMax);
        }

        [Test]
        public void Should_add_vx_and_vy_then_store_result_in_vx_and_set_VF_to_0_and_increment_program_counter_on_ADD_with_carry()
        {
            const int argumentMax = 15;
            TestForAllRegistersAndArgumentRange((vx, vy) =>
            {
                if (vx.IsCarryRegister() || vy.IsCarryRegister())
                    return;

                _cpu.State.V[argumentMax] = 1;
                _cpu.State.V[vx] = 100;
                _cpu.State.V[vy] = 100;

                const short initalProgramCounter = 4;
                ProgramCounter = initalProgramCounter;

                var state = Execute(x => x.AddCarry, vx, vy);

                state.ProgramCounter.Should().Be(initalProgramCounter + 2);
                state.V[argumentMax].Should().Be(0);
                state.V[vx].Should().Be(200);
            },
            argumentMax);
        }

        [Test]
        public void Should_add_vx_and_vy_then_store_result_in_vx_and_set_VF_to_1_and_increment_program_counter_on_ADD_with_carry()
        {
            const int argumentMax = 15;
            TestForAllRegistersAndArgumentRange((vx, vy) =>
            {
                if (vx.IsCarryRegister() || vy.IsCarryRegister())
                    return;

                _cpu.State.V[vx] = 200;
                _cpu.State.V[vy] = 200;
                _cpu.State.V[argumentMax] = 0;

                const short initalProgramCounter = 4;
                ProgramCounter = initalProgramCounter;

                var state = Execute(x => x.AddCarry, vx, vy);

                state.ProgramCounter.Should().Be(initalProgramCounter + 2);
                state.V[argumentMax].Should().Be(1);
                state.V[vx].Should().Be(144);
            },
            argumentMax);
        }

        private delegate void RegisterTestAssertDelegate(byte register, byte argument);

        private void TestForAllRegistersAndArgumentRange(RegisterTestAssertDelegate asserter, int argumentMax)
        {
            const int registers = 16;
            for (var register = 0; register < registers; register++)
            {
                for (var i = 0; i <= argumentMax; i++)
                {
                    asserter((byte)register, (byte)i);
                    ResetCpuState();
                }
            }
        }

        private int FindValueNotEqualTo(int value, int max)
        {
            var random = new Random();

            int newValue;
            while ((newValue = random.Next(max)) == value) { }

            return newValue;
        }

        private short ProgramCounter
        {
            get { return _cpu.State.ProgramCounter; }
            set { _cpu.State.ProgramCounter = value; }
        }

        private void ResetCpuState()
        {
            _cpu.State = new CpuState();
        }

        private CpuState Execute(Func<Chip8Cpu, Action> instructionGetter)
        {
            var action = instructionGetter(_cpu);

            action();
            return _cpu.State;
        }

        private CpuState Execute(Func<Chip8Cpu, Action<short>> instructionGetter, short argument)
        {
            var action = instructionGetter(_cpu);

            action(argument);
            return _cpu.State;
        }

        private CpuState Execute(Func<Chip8Cpu, Action<byte, byte>> instructionGetter, byte register, byte argument)
        {
            var action = instructionGetter(_cpu);

            action(register, argument);
            return _cpu.State;
        }
    }

    public static class CpuTestExtensions
    {
        public static bool IsCarryRegister(this byte register)
        {
            return register == 0x0F;
        }
    }
}