using Cj.Chip8.Cpu;
using NUnit.Framework;
using FluentAssertions;

namespace Cj.Chip8.Test
{
    [TestFixture]
    public class DisplayTests
    {
        private Display _display;

        [SetUp]
        public void SetUp()
        {
            _display = new Display();
        }

        [Test]
        public void Has_64_x_32_pixel_display()
        {
            _display.Pixels.Length.Should().Be(64 * 32);
            _display.Pixels.Should().BeOfType<byte[]>();
        }

        [Test]
        public void Clear_should_clear_display()
        {
            for (var i = 0; i < _display.Pixels.Length; i++)
            {
                _display.Pixels[i] = 255;
            }

            _display.Clear();

            _display.Pixels.Should().OnlyContain(b => b == 0);
        }

        [Test]
        public void Draw_should_draw_sprite_starting_at_x_y()
        {
            var sprite = new byte[]
                {
                    0x01,
                    0x02,
                    0x03,
                    0x04
                };

            _display.Draw(4, 2, sprite);

            for (var i = 0; i < sprite.Length; i++)
            {
                AssertRow(4, 2 + i, sprite[i]);
            }
        }

        [Test]
        public void Draw_should_return_0_when_not_turning_off_pixels()
        {
            var sprite = new byte[]
                {
                    0x01,
                };

            var result = _display.Draw(4, 2, sprite);

            result.Should().Be(0);
        }

        [Test]
        public void Draw_should_xor_already_visible_pixels()
        {
            for (var i = 0; i < 8; i++)
            {
                _display.Pixels[i] = 1;
            }

            var sprite = new byte[]
                {
                    0x11
                };

            _display.Draw(0, 0, sprite);

            AssertRow(0, 0, 0xEE);
        }

        [Test]
        public void Draw_should_return_1_when_ereasing_pixels()
        {
            for (var i = 0; i < 8; i++)
            {
                _display.Pixels[i] = 1;
            }

            var sprite = new byte[]
                {
                    0x11
                };

            var result = _display.Draw(0, 0, sprite);

            result.Should().Be(1);
        }

        private void AssertRow(int x, int y, byte sprite)
        {
            var offset = x + y * 32;

            for (var i = 0; i < 8; i++)
            {
                var pixel = _display.Pixels[offset + i];
                var pixelValue = (sprite >> (7 - i)) & 0x01;

                pixel.Should().Be((byte)pixelValue);
            }
        }
    }
}