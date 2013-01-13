namespace Cj.Chip8.Cpu
{
    public class Display : IDisplay
    {
        private readonly byte[] _pixels = new byte[2048];
        public byte[] Pixels
        {
            get { return _pixels; }
        }

        public void Clear()
        {
            for (var i = 0; i < Pixels.Length; i++)
            {
                Pixels[i] = 0;
            }
        }

        public byte Draw(byte x, byte y, byte[] sprite)
        {
            byte ereasedPixel = 0;

            for (var i = 0; i < sprite.Length; i++)
            {
                var rowStartOffset = x + (y + i) * 32;

                for (var j = 0; j < 8; j++)
                {
                    var pixelOffset = rowStartOffset + j;

                    var oldPixelValue = Pixels[pixelOffset];
                    var newPixelValue = (sprite[i] >> (7 - j)) & 0x01;

                    if (oldPixelValue == 1 && newPixelValue == 1)
                        ereasedPixel = 1;

                    Pixels[pixelOffset] = (byte) (newPixelValue ^ Pixels[pixelOffset]);
                }
            }

            return ereasedPixel;
        }
    }
}