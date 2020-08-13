using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using YACLAP;
using YACLAP.Extensions;

namespace Plait
{
    class Program
    {
        static void Main(string[] args)
        {
            var clap = new SimpleParser(args);
            var inputs = clap.Arguments;
            var options = GetOptions(clap);
            options = CheckOptions(inputs, options);

            Console.WriteLine("Plaiting...");
            ShowOptions(options, inputs);

            var bitmaps = ReadImages(inputs);


            var output = Plait(bitmaps, options);

            bitmaps.Dispose();

            SaveImage(output, options.OutputFilename);
        }

        private static OutputFormat GetOptions(SimpleParser clap)
        {
            var outputFormat = new OutputFormat {A = "a1", R = "r1", G = "g1", B = "b1"};

            if (clap.HasOption("alpha")) outputFormat.A = clap.Option("alpha");
            if (clap.HasOption("red")) outputFormat.R = clap.Option("red");
            if (clap.HasOption("green")) outputFormat.G = clap.Option("green");
            if (clap.HasOption("blue")) outputFormat.B = clap.Option("blue");
            if (clap.HasOption("output")) outputFormat.OutputFilename = clap.Option("output");
            
            return outputFormat;
        }

        private static OutputFormat CheckOptions(string[] inputs, OutputFormat outputFormat)
        {
            var originalExtension = Path.GetExtension(inputs[0]);
            if (string.IsNullOrEmpty(outputFormat.OutputFilename))
            {
                outputFormat.OutputFilename = Path.GetFullPath(inputs[0])
                    .Replace(originalExtension, "-plaited" + originalExtension);
            }

            if (Path.GetExtension(outputFormat.OutputFilename) != originalExtension)
            {
                outputFormat.OutputFilename = Path.ChangeExtension(outputFormat.OutputFilename, originalExtension);
            }

            return outputFormat;
        }

        private static List<Bitmap> ReadImages(string[] inputs)
        {
            var bitmaps = ReadAllImages(inputs);

            CheckForConsistentImageSizes(inputs, bitmaps);

            return bitmaps;
        }

        private static List<Bitmap> ReadAllImages(string[] inputs)
        {
            var bitmaps = new List<Bitmap>();
            foreach (var input in inputs)
            {
                if (!File.Exists(input)) throw new FileNotFoundException($"Image file not found: {input}", input);

                bitmaps.Add(new Bitmap(input));
            }

            return bitmaps;
        }

        private static void CheckForConsistentImageSizes(string[] inputs, List<Bitmap> bitmaps)
        {
            var groupBy = bitmaps.GroupBy(b => $"{b.Width}x{b.Height}");
            if (groupBy.Count() != 1)
            {
                for (var i = 0; i < inputs.Length; i++)
                {
                    Console.WriteLine($"  {Path.GetFileName(inputs[i])} : {bitmaps[i].Width}px x {bitmaps[i].Height}px");
                }

                throw new ArgumentException("Inconsistent image sizes");
            }
        }

        private static Bitmap Plait(List<Bitmap> bitmaps, OutputFormat outputFormat)
        {
            var width = bitmaps[0].Width;
            var height = bitmaps[0].Height;
            var output = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            for (var y = 0; y <= width - 1; y++)
            {
                for (var x = 0; x <= height - 1; x++)
                {
                    var a = GetChannelValue(bitmaps, x, y, outputFormat.AIndex, outputFormat.AChannel);
                    var r = GetChannelValue(bitmaps, x, y, outputFormat.RIndex, outputFormat.RChannel);
                    var g = GetChannelValue(bitmaps, x, y, outputFormat.GIndex, outputFormat.GChannel);
                    var b = GetChannelValue(bitmaps, x, y, outputFormat.BIndex, outputFormat.BChannel);
                    output.SetPixel(x, y, Color.FromArgb(a, r, g, b));
                }
            }

            return output;
        }

        private static byte GetChannelValue(List<Bitmap> bitmaps, int x, int y, int bitmapIndex, string channel) 
            => byte.TryParse(channel, out var result) 
                ? result 
                : bitmaps[bitmapIndex].GetPixel(x, y).GetChannel(channel);

        private struct OutputFormat
        {
            public string OutputFilename;
            public string A;
            public string R;
            public string G;
            public string B;

            public int AIndex => A.Substring(1).ToInt() - 1;
            public int RIndex => R.Substring(1).ToInt() - 1;
            public int GIndex => G.Substring(1).ToInt() - 1;
            public int BIndex => B.Substring(1).ToInt() - 1;
            public string AChannel => DecodeChannel(A);
            public string RChannel => DecodeChannel(R);
            public string GChannel => DecodeChannel(G);
            public string BChannel => DecodeChannel(B);

            private string DecodeChannel(string channel) 
                => int.TryParse(channel, out var result) 
                    ? channel 
                    : channel.Substring(0, 1);
        }

        private static void ShowOptions(OutputFormat options, string[] inputs)
        {
            Console.WriteLine(ChannelValue(options.AChannel, options.AIndex, inputs, "Alpha"));
            Console.WriteLine(ChannelValue(options.RChannel, options.RIndex, inputs, "Red"));
            Console.WriteLine(ChannelValue(options.GChannel, options.GIndex, inputs, "Green"));
            Console.WriteLine(ChannelValue(options.BChannel, options.BIndex, inputs, "Blue"));
        }

        private static string ChannelValue(string channelCode, int index, string[] inputs, string channelName)
            => int.TryParse(channelCode, out var a)
                ? $"{channelName,-5}: {a} (0x{a:X2})"
                : $"{channelName,-5}: ...\\{Path.GetFileName(inputs[index])}";

        private static void SaveImage(Bitmap bitmap, string outputFile)
        {
            var fullPath = Path.GetFullPath(outputFile);
            Console.WriteLine($"Output: {fullPath}");
            bitmap.Save(fullPath, bitmap.RawFormat);
        }
    }
}