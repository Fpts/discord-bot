﻿using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ColorThiefDotNet;
using DSharpPlus.Entities;

namespace CompatBot.Utils
{
    internal static class ColorGetter
    {
        public static DiscordColor Analyze(byte[] jpg, DiscordColor defaultColor)
        {
            try
            {
                // when running dotnet from the snap, it will segfault on attempt to create a Bitmap
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SNAP")))
                    return defaultColor;

                var analyzer = new ColorThief();
                using (var stream = new MemoryStream(jpg))
                {
                    var bmp = new Bitmap(stream, false);
                    var palette = analyzer.GetPalette(bmp, ignoreWhite: false);
                    var colors = palette
                        .Select(p => new {c = p.Color, hsl = p.Color.ToHsl()})
                        .OrderBy(p => Math.Abs(0.5 - p.hsl.L))
                        .ThenByDescending(p => p.hsl.S)
                        .ToList();
#if DEBUG
                    foreach (var cl in colors)
                        Config.Log.Trace($"{cl.c.ToHexString()}, HSL: {cl.hsl.H:000} {cl.hsl.S:0.00} {cl.hsl.L:0.00}");
#endif
                    var c = colors[0].c;
                    return new DiscordColor(c.R, c.G, c.B);
                }
            }
            catch (Exception e)
            {
                Config.Log.Warn(e, "Failed to extract image palette");
                return defaultColor;
            }
        }
    }
}
