using Cj.Chip8.Cpu;
using NUnit.Framework;
using FluentAssertions;

namespace Cj.Chip8.Test
{
    [TestFixture]
    public class BcdConverterTests
    {
        private BcdConverter _converter;

        [SetUp]
        public void SetUp()
        {
            _converter = new BcdConverter();
        }

        [Test]
        public void Should_convert_value_correctly()
        {
            var result = _converter.ConvertToBcd(255);

            result.Length.Should().Be(3);
            result[0].Should().Be(2);
            result[1].Should().Be(5);
            result[2].Should().Be(5);
        }

        [Test]
        public void Should_convert_value_correctly_2()
        {
            var result = _converter.ConvertToBcd(123);

            result.Length.Should().Be(3);
            result[0].Should().Be(1);
            result[1].Should().Be(2);
            result[2].Should().Be(3);
        }

        [Test]
        public void Should_convert_one_digit_value()
        {
            var result = _converter.ConvertToBcd(3);

            result.Length.Should().Be(3);
            result[0].Should().Be(0);
            result[1].Should().Be(0);
            result[2].Should().Be(3);
        }

        [Test]
        public void Should_convert_two_digit_value()
        {
            var result = _converter.ConvertToBcd(15);

            result.Length.Should().Be(3);
            result[0].Should().Be(0);
            result[1].Should().Be(1);
            result[2].Should().Be(5);
        }
    }
}