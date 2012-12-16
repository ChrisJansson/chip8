﻿using System;
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
            SetProgramCounterTo(initialProgramCounter);

            _cpu.Cls();

            _display.Verify(x => x.Clear());
            ProgramCounter.Should().Be(initialProgramCounter + 2);
        }

        [Test]
        public void Should_set_program_counter_to_adress_at_top_of_stack_then_subtract_one_from_stack_pointer_on_Ret()
        {
            _cpu.State.Stack[0] = 4;
            _cpu.State.StackPointer = 1;

            _cpu.Ret();

            ProgramCounter.Should().Be(4);
            _cpu.State.StackPointer.Should().Be(0);
        }

        [Test]
        public void Should_set_program_counter_to_jump_argument()
        {
            for (short argument = 0; argument <= 0xFFF; argument++)
            {
                _cpu.Jump(argument);
                ProgramCounter.Should().Be(argument);
            }
        }

        [Test]
        public void Should_store_current_add_program_counter_on_stack_then_increase_stack_pointer_and_set_program_counter_to_argument_on_Call()
        {
            for (short argument = 0; argument <= 0xFFF; argument++)
            {
                const short initialProgramCounter = 0x600;
                SetProgramCounterTo(initialProgramCounter);

                _cpu.Call(argument);

                ProgramCounter.Should().Be(argument);
                _cpu.State.StackPointer.Should().Be(1);
                _cpu.State.Stack[0].Should().Be(initialProgramCounter);

                ResetCpuState();
            }
        }

        [Test]
        public void Should_increment_program_counter_by_two_instructions_when_Vx_is_equal_to_argument_on_SE()
        {
            for (short register = 0; register < 16; register++)
            {
                for (short i = 0; i < 256; i++)
                {
                    _cpu.State.Vx[register] = i;

                    const short initialProgramCounter = 4;
                    SetProgramCounterTo(initialProgramCounter);

                    short argument = (short)((register << 8) | i);
                    _cpu.Se(argument);

                    _cpu.State.ProgramCounter.Should().Be(initialProgramCounter + 4);
                    
                    ResetCpuState();
                }
            }
        }

        [Test]
        public void Should_not_increment_program_counter_by_two_instructions_when_Vx_is_not_equal_to_argument_on_SE()
        {
            for (short register = 0; register < 16; register++)
            {
                for (short i = 0; i < 256; i++)
                {
                    _cpu.State.Vx[register] = i;

                    const short initialProgramCounter = 4;
                    SetProgramCounterTo(initialProgramCounter);

                    short argument = (short)((register << 8) | (i+1) & 0xFF);
                    _cpu.Se(argument);

                    _cpu.State.ProgramCounter.Should().Be(initialProgramCounter + 2);

                    ResetCpuState();
                }
            }
        }

        private void SetProgramCounterTo(short value)
        {
            _cpu.State.ProgramCounter = value;
        }

        private short ProgramCounter
        {
            get { return _cpu.State.ProgramCounter; }
        }

        private void ResetCpuState()
        {
            _cpu.State = new CpuState();
        }
    }
}