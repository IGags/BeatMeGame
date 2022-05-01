using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatMeGame
{
    public static class SizeExtensions
    {
        public static readonly Size[] DefaultSizes = new Size[]
        {
            new Size(1920, 1080),
            new Size(1366, 768),
            new Size(1536, 864),
            new Size(1440, 900),
            new Size(1280, 720),
            new Size(1600, 900)
        };

        public static bool TryParse(string value, out Size outParameter)
        {
            outParameter = Size.Empty;
            if (value[0] != '{' || value[value.Length - 1] != '}') return false;
            value = value.Substring(1).Remove(value.Length - 2);
            var resolution = value.Split(new[] { ", ", "Width=", "Height=" }, StringSplitOptions.RemoveEmptyEntries);
            var isParsed = int.TryParse(resolution[0], out var width) & int.TryParse(resolution[1], out var height);
            if (!isParsed) return false;
            outParameter = new Size(width, height);
            return true;
        }
    }
}

