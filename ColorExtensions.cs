using System;
using System.Collections.Generic;
using System.Drawing;

public static class ColorExtensions
{
    public static byte GetChannel(this Color color, string channel)
    {
        switch (channel.ToLower())
        {
            case "a":
                return color.A;
            case "r":
                return color.R;
            case "g":
                return color.G;
            case "b":
                return color.B;
            default:
                throw new ArgumentException($"Invalid channel id: {channel}");
        }
    }
}

public static class BitmapExtensions
{
    public static void Dispose(this IEnumerable<Bitmap> bitmaps)
    {
        foreach (var bitmap in bitmaps)
        {
            bitmap.Dispose();
        }
    }


}