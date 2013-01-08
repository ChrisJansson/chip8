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
            _cpu = new Mock<IChip8Cpu>();

            _instructionDecoder = new InstructionDecoder();
        }

        [Test]
        public void Should_call_CLS_when_decoding_00E0()
        {
            
        }
    }
}