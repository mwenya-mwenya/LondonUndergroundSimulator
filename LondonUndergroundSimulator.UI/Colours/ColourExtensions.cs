using System;

namespace LondonUndergroundSimulator.Colours
{
    using Microsoft.UI;
    using Windows.UI;

    public static class ColorExtensions
    {
        public static Color FromHex(string hex)
        {
            hex = hex.Replace("#", "");

            byte a = 255;
            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);

            // If hex includes alpha (#AARRGGBB)
            if (hex.Length == 8)
            {
                a = r;
                r = g;
                g = b;
                b = Convert.ToByte(hex.Substring(6, 2), 16);
            }

            return ColorHelper.FromArgb(a, r, g, b);
        }
    }
}
