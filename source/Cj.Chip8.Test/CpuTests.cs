﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        private Mock<IRandomizer> _randomizer;
        private Mock<IKeyboard> _keyboard;
        private Mock<IBcdConverter> _bcdConverter;
        private Mock<IInstructionDecoder> _instructionDecoder;
        private Mock<ITimerClock> _timerClock;

        [SetUp]
        public void SetUp()
        {
            _display = new Mock<IDisplay>();
            _randomizer = new Mock<IRandomizer>();
            _keyboard = new Mock<IKeyboard>();
            _bcdConverter = new Mock<IBcdConverter>();
            _instructionDecoder = new Mock<IInstructionDecoder>();
            _timerClock = new Mock<ITimerClock>();

            _cpu = new Chip8Cpu(_display.Object, _randomizer.Object, _keyboard.Object, _bcdConverter.Object, _instructionDecoder.Object, _timerClock.Object);
        }

        [Test]
        public void Should_advance_program_counter_by_one_instruction_when_executing_instructions()
        {
            _bcdConverter.Setup(x => x.ConvertToBcd(It.IsAny<byte>())).Returns(new byte[] { 1, 2, 3 });

            AssertProgramCounter(x => x.Cls());
            AssertProgramCounter(x => x.Ret());
            AssertProgramCounter(x => x.Drw(0x00, 0x01, 0x02));
            AssertProgramCounter(x => x.Lddt(0x00));
            AssertProgramCounter(x => x.Ldk(0x00));
            AssertProgramCounter(x => x.SetDt(0x00));
            AssertProgramCounter(x => x.SetSt(0x00));
            AssertProgramCounter(x => x.AddI(0x00));
            AssertProgramCounter(x => x.Ldf(0x00));
            AssertProgramCounter(x => x.Ldb(0x00));
            AssertProgramCounter(x => x.CopyRegisters(0x00));
            AssertProgramCounter(x => x.CopyMemory(0x00));
        }

        private void AssertProgramCounter(Expression<Action<Chip8Cpu>> instructionExecutor)
        {
            const short initialProgramCounter = 0x200;
            _cpu.State.ProgramCounter = initialProgramCounter;
            
            _cpu.State.StackPointer = 1;
            _cpu.State.Stack[0] = initialProgramCounter;

            var methodCallExpression = instructionExecutor.Body as MethodCallExpression;
            var methodName = methodCallExpression.Method.Name;

            var action = instructionExecutor.Compile();
            action(_cpu);

            _cpu.State.ProgramCounter.Should().Be(initialProgramCounter + 2, "instruction {0} should advance program counter", methodName);
        }

        [Test]
        public void Should_clear_display_on_Cls()
        {
            Execute(x => x.Cls);

            _display.Verify(x => x.Clear());
        }

        [Test]
        public void Should_set_program_counter_to_adress_at_top_of_stack_then_subtract_one_from_stack_pointer_on_Ret()
        {
            _cpu.State.Stack[0] = 4;
            _cpu.State.StackPointer = 1;

            var state = Execute(x => x.Ret);

            ProgramCounter.Should().Be(6);
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
                    state.V[vx].Should().Be((byte)(15 + argument));
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
                state.V[vx].Should().Be((byte)expectedResult);
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

        [Test]
        public void Should_subtract_vy_from_vx_and_store_result_in_vx_and_set_vf_to_1_and_increment_program_counter_on_SUB()
        {
            const int argumentMax = 15;
            TestForAllRegistersAndArgumentRange((vx, vy) =>
            {
                if (vx.IsCarryRegister() || vy.IsCarryRegister() || vx == vy)
                    return;

                _cpu.State.V[vx] = 100;
                _cpu.State.V[vy] = 100;
                _cpu.State.V[argumentMax] = 0;

                const short initalProgramCounter = 4;
                ProgramCounter = initalProgramCounter;

                var state = Execute(x => x.Sub, vx, vy);

                state.ProgramCounter.Should().Be(initalProgramCounter + 2);
                state.V[argumentMax].Should().Be(1);
                state.V[vx].Should().Be(0);
            },
            argumentMax);
        }

        [Test]
        public void Should_subtract_vy_from_vx_and_store_result_in_vx_and_set_vf_to_0_and_increment_program_counter_on_SUB()
        {
            const int argumentMax = 15;
            TestForAllRegistersAndArgumentRange((vx, vy) =>
            {
                if (vx.IsCarryRegister() || vy.IsCarryRegister() || vx == vy)
                    return;

                _cpu.State.V[vx] = 100;
                _cpu.State.V[vy] = 101;
                _cpu.State.V[argumentMax] = 1;

                const short initalProgramCounter = 4;
                ProgramCounter = initalProgramCounter;

                var state = Execute(x => x.Sub, vx, vy);

                state.ProgramCounter.Should().Be(initalProgramCounter + 2);
                state.V[argumentMax].Should().Be(0);
                state.V[vx].Should().Be(255); //Twos complement of 1, or -1 in a byte
            },
            argumentMax);
        }

        [Test]
        public void Should_shift_vx_right_and_set_VF_to_0_and_increment_program_counter_on_SHR()
        {
            TestForAllRegistersExceptVf(vx =>
                {
                    _cpu.State.V[vx] = 2;
                    _cpu.State.V[0x0F] = 1;

                    const short initalProgramCounter = 4;
                    ProgramCounter = initalProgramCounter;

                    var state = Execute(x => x.Shr(vx));

                    state.ProgramCounter.Should().Be(initalProgramCounter + 2);
                    state.V[0x0F].Should().Be(0);
                    state.V[vx].Should().Be(1); // 2 >> 1
                });
        }

        [Test]
        public void Should_shift_vx_right_and_set_VF_to_1_and_increment_program_counter_on_SHR()
        {
            TestForAllRegistersExceptVf(vx =>
            {
                _cpu.State.V[vx] = 3;
                _cpu.State.V[0x0F] = 0;

                const short initalProgramCounter = 4;
                ProgramCounter = initalProgramCounter;

                var state = Execute(x => x.Shr(vx));

                state.ProgramCounter.Should().Be(initalProgramCounter + 2);
                state.V[0x0F].Should().Be(1);
                state.V[vx].Should().Be(1); // 2 >> 1
            });
        }

        [Test]
        public void Should_subtract_vx_from_vy_and_set_vx_to_result_and_vf_to_not_borrow_1_on_SUBN()
        {
            var argumentCombinations = from register in Enumerable.Range(0, 15)
                                       from argument in Enumerable.Range(0, 15)
                                       where register != argument
                                       select new { vx = register, vy = argument };

            foreach (var argumentCombination in argumentCombinations)
            {
                _cpu.State.V[argumentCombination.vx] = 100;
                _cpu.State.V[argumentCombination.vy] = 100;
                _cpu.State.V[0x0F] = 0;

                const short initalProgramCounter = 4;
                ProgramCounter = initalProgramCounter;

                var combination = argumentCombination;
                var state = Execute(x => x.Subn(combination.vx, combination.vy));

                state.ProgramCounter.Should().Be(initalProgramCounter + 2);
                state.V[0x0F].Should().Be(1);
                state.V[argumentCombination.vx].Should().Be(0);

                ResetCpuState();
            }
        }

        [Test]
        public void Should_subtract_vx_from_vy_and_set_vx_to_result_and_vf_to_not_borrow_0_on_SUBN()
        {
            var argumentCombinations = from register in Enumerable.Range(0, 15)
                                       from argument in Enumerable.Range(0, 15)
                                       where register != argument
                                       select new { vx = register, vy = argument };

            foreach (var argumentCombination in argumentCombinations)
            {
                _cpu.State.V[argumentCombination.vx] = 101;
                _cpu.State.V[argumentCombination.vy] = 100;
                _cpu.State.V[0x0F] = 1;

                const short initalProgramCounter = 4;
                ProgramCounter = initalProgramCounter;

                var combination = argumentCombination;
                var state = Execute(x => x.Subn(combination.vx, combination.vy));

                state.ProgramCounter.Should().Be(initalProgramCounter + 2);
                state.V[0x0F].Should().Be(0);
                state.V[argumentCombination.vx].Should().Be(255);

                ResetCpuState();
            }
        }

        [Test]
        public void Should_shift_vx_left_and_set_vf_to_msb_0_and_increment_program_counter_on_SHL()
        {
            var registers = Enumerable.Range(0, 15);

            foreach (var register in registers)
            {
                var vx = (byte)register;
                _cpu.State.V[register] = 0x01;
                _cpu.State.V[0x0F] = 1;

                const short initalProgramCounter = 4;
                ProgramCounter = initalProgramCounter;

                var state = Execute(x => x.Shl(vx));

                state.ProgramCounter.Should().Be(initalProgramCounter + 2);
                state.V[0x0F].Should().Be(0);
                state.V[vx].Should().Be(2);

                ResetCpuState();
            }
        }

        [Test]
        public void Should_shift_vx_left_and_set_vf_to_msb_1_and_increment_program_counter_on_SHL()
        {
            var registers = Enumerable.Range(0, 15);

            foreach (var register in registers)
            {
                var vx = (byte)register;
                _cpu.State.V[register] = 0x81;
                _cpu.State.V[0x0F] = 0;

                const short initalProgramCounter = 4;
                ProgramCounter = initalProgramCounter;

                var state = Execute(x => x.Shl(vx));

                state.ProgramCounter.Should().Be(initalProgramCounter + 2);
                state.V[0x0F].Should().Be(1);
                state.V[vx].Should().Be(2);

                ResetCpuState();
            }
        }
        //9xy0 - SNE Vx, Vy
        //Skip next instruction if Vx != Vy.

        //The values of Vx and Vy are compared, and if they are not equal, the program counter is increased by 2.
        [Test]
        public void Should_not_skip_next_instruction_when_vx_equals_vy_on_Sne()
        {
            var argumentCombinations = from register in Enumerable.Range(0, 16)
                                       from argument in Enumerable.Range(0, 16)
                                       select new { vx = (byte)register, vy = (byte)argument };

            foreach (var argumentCombination in argumentCombinations)
            {
                _cpu.State.V[argumentCombination.vx] = 100;
                _cpu.State.V[argumentCombination.vy] = 100;

                const short initalProgramCounter = 4;
                ProgramCounter = initalProgramCounter;

                var combination = argumentCombination;
                var state = Execute(x => x.Sne(combination.vx, combination.vy));

                state.ProgramCounter.Should().Be(initalProgramCounter + 2);

                ResetCpuState();
            }
        }

        [Test]
        public void Should_skip_next_instruction_when_vx_does_not_equal_vy_on_Sne()
        {
            var argumentCombinations = from register in Enumerable.Range(0, 16)
                                       from argument in Enumerable.Range(0, 16)
                                       where register != argument
                                       select new { vx = (byte)register, vy = (byte)argument };

            foreach (var argumentCombination in argumentCombinations)
            {
                _cpu.State.V[argumentCombination.vx] = 100;
                _cpu.State.V[argumentCombination.vy] = 101;

                const short initalProgramCounter = 4;
                ProgramCounter = initalProgramCounter;

                var combination = argumentCombination;
                var state = Execute(x => x.Sne(combination.vx, combination.vy));

                state.ProgramCounter.Should().Be(initalProgramCounter + 4);

                ResetCpuState();
            }
        }

        [Test]
        public void Should_set_value_of_I_to_value_on_LD()
        {
            var arguments = Enumerable.Range(0, 4096).Select(x => (short)x);

            foreach (var argument in arguments)
            {
                const short initalProgramCounter = 4;
                ProgramCounter = initalProgramCounter;

                var address = argument;
                var state = Execute(x => x.Ldi(address));

                state.ProgramCounter.Should().Be(initalProgramCounter + 2);
                state.I.Should().Be(argument);

                ResetCpuState();
            }
        }

        [Test]
        public void Should_set_program_counter_to_v0_plus_nnn_on_Jump()
        {
            var arguments = Enumerable.Range(0, 4096).Select(x => (short)x);

            foreach (var argument in arguments)
            {
                _cpu.State.V[0x00] = 4;

                var address = argument;
                var state = Execute(x => x.JumpV0Offset(address));

                state.ProgramCounter.Should().Be((short)(4 + argument));

                ResetCpuState();
            }
        }

        [Test]
        public void Should_set_vx_to_random_byte_anded_with_kk_on_RND()
        {
            var argumentCombinations = from register in Enumerable.Range(0, 16)
                                       from argument in Enumerable.Range(0, byte.MaxValue + 1)
                                       where register != argument
                                       select new { vx = (byte)register, kk = (byte)argument };

            const byte randomValue = 0xF1;
            _randomizer.Setup(x => x.GetNext()).Returns(randomValue);

            foreach (var argumentCombination in argumentCombinations)
            {
                const short initialProgramCounter = 4;
                ProgramCounter = initialProgramCounter;

                var combination = argumentCombination;
                var state = Execute(x => x.Rnd(combination.vx, combination.kk));

                state.ProgramCounter.Should().Be(initialProgramCounter + 2);
                state.V[combination.vx].Should().Be((byte)(randomValue & argumentCombination.kk));

                ResetCpuState();
            }
        }

        [Test]
        public void DRW_should_draw_sprite_at_i_with_coordinates_vx_vy_and_height_n()
        {
            var argumentCombinations = from vx in Enumerable.Range(0, 16)
                                       from vy in Enumerable.Range(0, 16)
                                       from height in Enumerable.Range(0, 16)
                                       where vx != vy
                                       select new { vx = (byte)vx, vy = (byte)vy, height = (byte)height };

            var fullSprite = Enumerable.Range(1, 16).Select(x => (byte)x).ToArray();

            foreach (var argumentCombination in argumentCombinations)
            {
                for (var i = 0; i < 16; i++)
                {
                    _cpu.State.Memory[0x200 + i] = (byte)(i + 1);
                }

                _cpu.State.I = 0x200;
                _cpu.State.V[argumentCombination.vx] = 0x1F;
                _cpu.State.V[argumentCombination.vy] = 0xF1;

                var actualSprite = new byte[200];
                _display.Setup(x => x.Draw(0x1F, 0xF1, It.IsAny<byte[]>()))
                    .Callback((byte x, byte y, byte[] sprite) => actualSprite = sprite);

                var combination = argumentCombination;
                var state = Execute(x => x.Drw(combination.vx, combination.vy, combination.height));

                state.V[0x0F].Should().Be(0);
                actualSprite.Length.Should().Be(argumentCombination.height);
                actualSprite.Should().ContainInOrder(fullSprite.Take(argumentCombination.height).ToList());

                ResetCpuState();
            }
        }

        [Test]
        public void DRW_should_set_vf_to_display_draw_return_0()
        {
            _display.Setup(x => x.Draw(It.IsAny<byte>(), It.IsAny<byte>(), It.IsAny<byte[]>())).Returns(0x00);

            _cpu.State.V[0x0F] = 1;

            var state = Execute(x => x.Drw(0x00, 0x00, 0x00));

            state.V[0x0F].Should().Be(0);
        }

        [Test]
        public void DRW_should_set_vf_to_display_draw_return_1()
        {
            _display.Setup(x => x.Draw(It.IsAny<byte>(), It.IsAny<byte>(), It.IsAny<byte[]>())).Returns(0x01);

            _cpu.State.V[0x0F] = 0;

            var state = Execute(x => x.Drw(0x00, 0x00, 0x00));

            state.V[0x0F].Should().Be(1);
        }

        [Test]
        [TestCase(2, false)]
        [TestCase(4, true)]
        public void SKP_should_skip_next_instruction_if_key_in_vx_is_pressed(short expectedProgramCounterIncrease, bool isKeyDown)
        {
            var combinations = from register in Enumerable.Range(0, 16)
                               from key in Enumerable.Range(0, 16)
                               select new { vx = (byte)register, key = (byte)key };

            foreach (var argumentCombination in combinations)
            {
                var combination = argumentCombination;

                _keyboard.Setup(x => x.IsKeyDown(combination.key)).Returns(isKeyDown);

                const short initalProgramCounter = 4;
                ProgramCounter = initalProgramCounter;

                var state = Execute(x => x.Skp(combination.vx));

                state.ProgramCounter.Should().Be((short)(initalProgramCounter + expectedProgramCounterIncrease));

                ResetCpuState();
            }
        }

        [Test]
        [TestCase(2, true)]
        [TestCase(4, false)]
        public void SKPN_should_skip_next_instruction_if_key_in_vx_is_not_pressed(short expectedProgramCounterIncrease, bool isKeyDown)
        {
            var combinations = from register in Enumerable.Range(0, 16)
                               from key in Enumerable.Range(0, 16)
                               select new { vx = (byte)register, key = (byte)key };

            foreach (var argumentCombination in combinations)
            {
                var combination = argumentCombination;

                _keyboard.Setup(x => x.IsKeyDown(combination.key)).Returns(isKeyDown);

                const short initalProgramCounter = 4;
                ProgramCounter = initalProgramCounter;

                var state = Execute(x => x.Sknp(combination.vx));

                state.ProgramCounter.Should().Be((short)(initalProgramCounter + expectedProgramCounterIncrease));

                ResetCpuState();
            }
        }

        [Test]
        public void DT_should_put_the_value_of_dt_into_vx()
        {
            var argumentCombinations = from register in Enumerable.Range(0, 16)
                                       from delay in Enumerable.Range(0, byte.MaxValue + 1)
                                       select new { vx = (byte)register, delay = (byte)delay };

            foreach (var argumentCombination in argumentCombinations)
            {
                _cpu.State.DelayTimer = argumentCombination.delay;

                var combination = argumentCombination;
                var state = Execute(x => x.Lddt(combination.vx));

                state.V[argumentCombination.vx].Should().Be(argumentCombination.delay);

                ResetCpuState();
            }
        }

        [Test]
        public void K_should_store_the_value_of_key_press_in_vx()
        {
            var argumentCombinations = from register in Enumerable.Range(0, 16)
                                       from key in Enumerable.Range(0, 16)
                                       select new { vx = (byte)register, key = (byte)key };

            foreach (var argumentCombination in argumentCombinations)
            {
                _keyboard.Setup(x => x.WaitForKeyPress()).Returns(argumentCombination.key);

                var combination = argumentCombination;
                var state = Execute(x => x.Ldk(combination.vx));

                state.V[argumentCombination.vx].Should().Be(argumentCombination.key);

                ResetCpuState();
            }
        }

        [Test]
        public void Set_Dt_should_set_DT_to_the_value_of_vx()
        {
            var registers = Enumerable.Range(0, 16).Select(x => (byte)x).ToList();

            foreach (var register in registers)
            {
                _cpu.State.V[register] = 211;
                _cpu.State.DelayTimer = 0;

                var localRegister = register;
                var state = Execute(x => x.SetDt(localRegister));

                state.DelayTimer.Should().Be(211);

                ResetCpuState();
            }
        }

        [Test]
        public void Set_St_should_set_ST_to_the_value_of_vx()
        {
            var registers = Enumerable.Range(0, 16).Select(x => (byte)x).ToList();

            foreach (var register in registers)
            {
                _cpu.State.V[register] = 211;
                _cpu.State.SoundTimer = 0;

                var localRegister = register;
                var state = Execute(x => x.SetSt(localRegister));

                state.SoundTimer.Should().Be(211);

                ResetCpuState();
            }
        }

        [Test]
        public void Add_I_should_add_the_value_of_vx_to_i()
        {
            var registers = Enumerable.Range(0, 16).Select(x => (byte)x).ToList();

            foreach (var register in registers)
            {
                _cpu.State.I = 512;
                _cpu.State.V[register] = 211;
                _cpu.State.SoundTimer = 0;

                var localRegister = register;
                var state = Execute(x => x.AddI(localRegister));

                state.I.Should().Be(723);

                ResetCpuState();
            }
        }

        [Test]
        public void LDF_sprite_address_corresponding_to_sprite_in_vx_should_be_stored_in_I()
        {
            var argumentCombinations = from register in Enumerable.Range(0, 16)
                                       from sprite in Enumerable.Range(0, 16)
                                       select new { vx = (byte)register, sprite = (byte)sprite };

            foreach (var argumentCombination in argumentCombinations)
            {
                _cpu.State.I = 512;
                _cpu.State.V[argumentCombination.vx] = argumentCombination.sprite;

                var localRegister = argumentCombination.vx;
                var state = Execute(x => x.Ldf(localRegister));

                state.I.Should().Be((short)(argumentCombination.sprite * 5));

                ResetCpuState();
            }
        }

        [Test]
        public void LDB_converts_value_in_vx_to_bcd()
        {
            var argumentCombinations = from register in Enumerable.Range(0, 16)
                                       from value in Enumerable.Range(0, 256)
                                       select new { vx = (byte)register, value = (byte)value };

            foreach (var argumentCombination in argumentCombinations)
            {
                _cpu.State.V[argumentCombination.vx] = argumentCombination.value;
                _bcdConverter.Setup(x => x.ConvertToBcd(It.IsAny<byte>())).Returns(new byte[] { 1, 2, 3 });

                var localRegister = argumentCombination.vx;
                Execute(x => x.Ldb(localRegister));

                var combination = argumentCombination;
                _bcdConverter.Verify(x => x.ConvertToBcd(combination.value));

                ResetCpuState();
            }
        }

        [Test]
        public void LDB_stores_value_in_I()
        {
            var argumentCombinations = from i in Enumerable.Range(0, 0xFFF - 3)
                                       select (short)i;

            foreach (var i in argumentCombinations)
            {
                _cpu.State.I = i;

                const byte valueToConvert = (byte)123;

                _cpu.State.V[0] = valueToConvert;
                _bcdConverter.Setup(x => x.ConvertToBcd(valueToConvert)).Returns(new byte[] { 1, 2, 3 });

                var state = Execute(x => x.Ldb(0));

                state.Memory[i + 0].Should().Be(1);
                state.Memory[i + 1].Should().Be(2);
                state.Memory[i + 2].Should().Be(3);

                ResetCpuState();
            }
        }

        [Test]
        public void LDI_should_store_registers_v0_to_vx_in_memory_at_i()
        {
            var argumentCombinations = from register in Enumerable.Range(0, 16)
                                       select (byte)register;

            foreach (var register in argumentCombinations)
            {
                for (var i = 0; i < 16; i++)
                {
                    _cpu.State.V[i] = (byte)(100 + i);
                }

                _cpu.State.I = 0x200;

                var register1 = register;
                var state = Execute(x => x.CopyRegisters(register1));

                for (var i = 0; i <= register; i++)
                {
                    state.Memory[0x200 + i].Should().Be((byte) (100 + i));
                }

                ResetCpuState();
            }
        }

        [Test]
        public void Copy_memory_into_registers_should_copy_from_memory_into_registers()
        {
            var argumentCombinations = from register in Enumerable.Range(0, 16)
                                       select (byte)register;

            foreach (var register in argumentCombinations)
            {
                for (var i = 0; i < 16; i++)
                {
                    _cpu.State.Memory[0x200 + i] = (byte)(100 + i);
                }

                _cpu.State.I = 0x200;

                var register1 = register;
                var state = Execute(x => x.CopyMemory(register1));

                for (var i = 0; i <= register; i++)
                {
                    state.V[i].Should().Be((byte)(100 + i));
                }

                ResetCpuState();
            }
        }

        [Test]
        public void Emulate_cycle_should_read_instruction_from_memory_and_call_instruction_decoder()
        {
            _cpu.State.Memory[0x400] = 0x43;
            _cpu.State.Memory[0x401] = 0x65;
            _cpu.State.ProgramCounter = 0x400;

            _cpu.EmulateCycle();

            const int instruction = 0x4365;
            _instructionDecoder.Verify(x => x.DecodeAndExecute(instruction, _cpu));
        }

        [Test]
        public void Emulate_cycle_should_not_decrement_timers_if_1_60th_has_not_passed()
        {
            _cpu.State.SoundTimer = 2;
            _cpu.State.DelayTimer = 3;

            var times = new List<double> {0, 1/70.0};
            
            _timerClock.SetupGet(x => x.ElapsedSeconds).Returns(times.AsValueFunction());
            _cpu.EmulateCycle();
            _cpu.EmulateCycle();

            _cpu.State.SoundTimer.Should().Be(2);
            _cpu.State.DelayTimer.Should().Be(3);
        }

        [Test]
        public void Emulate_cycle_should_decrement_timers_if_1_60th_has_passed()
        {
            _cpu.State.SoundTimer = 2;
            _cpu.State.DelayTimer = 3;

            var times = new List<double> { 0, 1 / 50.0 };

            _timerClock.SetupGet(x => x.ElapsedSeconds).Returns(times.AsValueFunction());
            _cpu.EmulateCycle();
            _cpu.EmulateCycle();

            _cpu.State.SoundTimer.Should().Be(1);
            _cpu.State.DelayTimer.Should().Be(2);
        }

        [Test]
        public void Emulate_cycle_should_reset_timer_when_it_has_passed_60Hz()
        {
            _cpu.State.Memory[0x400] = 0x43;
            _cpu.State.Memory[0x401] = 0x65;
            _cpu.State.ProgramCounter = 0x400;

            _timerClock.SetupGet(x => x.ElapsedSeconds).Returns(1/50.0);
            _cpu.EmulateCycle();

            _timerClock.Verify(x => x.Reset());
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

        private void TestForAllRegistersExceptVf(Action<byte> asserter)
        {
            TestForAllRegistersExceptVfWithArgumentRange((x1, x2) => asserter(x1), 1);
        }

        private void TestForAllRegistersExceptVfWithArgumentRange(Action<byte, byte> asserter, byte argumentRange)
        {
            const int registers = 16;
            for (var register = 0; register < registers - 1; register++)
            {
                for (var i = 0; i <= argumentRange; i++)
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

        private CpuState Execute(Action<Chip8Cpu> instructionGetter)
        {
            instructionGetter(_cpu);
            return _cpu.State;
        }
    }

    public static class SequenceExtensions
    {
        public static Func<double> AsValueFunction(this IEnumerable<double> list)
        {
            var enumerator = list.GetEnumerator();
            enumerator.MoveNext();

            return () =>
                {
                    var current = enumerator.Current;
                    enumerator.MoveNext();
                    return current;
                };
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