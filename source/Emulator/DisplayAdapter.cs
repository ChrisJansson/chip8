﻿using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Cj.Chip8.Cpu;

namespace Gui
{
    public class DisplayAdapter
    {
        public BitmapSource CreateBitmap(IDisplay display)
        {
            var pixels = new byte[64*32];

            for (var y = 0; y < 32; y++)
            {
                for (var x = 0; x < 64; x++)
                {
                    var offset = y * 64 + x;
                    var pixel = display.Pixels[offset];
                    pixels[offset] = pixel;
                }
            }


            var bitmapPalette = new BitmapPalette(new List<Color> {Colors.DarkBlue, Colors.CornflowerBlue});
            return BitmapSource.Create(64, 32, 96, 96, PixelFormats.Indexed8, bitmapPalette, pixels, 64);
        }
    }
}