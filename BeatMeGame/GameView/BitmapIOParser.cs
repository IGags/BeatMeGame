using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatMeGame.GameView
{
    public static class BitmapIOParser
    {
        public static Dictionary<string, Bitmap> ParseSavedBitmaps(string levelName)
        {
            var path = $"Levels\\{levelName}\\Assets";
            var outDictionary = new Dictionary<string, Bitmap>();
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            foreach (var file in Directory.GetFiles(path, "*.png", SearchOption.AllDirectories))
            {
                outDictionary[file.Split('\\').Last().Split('.').First()] = new Bitmap(file);
            }

            return outDictionary;
        }
    }
}
