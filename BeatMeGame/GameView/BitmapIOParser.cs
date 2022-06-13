using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatMeGameModel;
using BeatMeGameModel.IOWorkers;

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
                using (var bmpFromFile = (Bitmap)Image.FromFile(file))
                {
                    outDictionary[file.Split('\\').Last().Split('.').First()] = new Bitmap(bmpFromFile);
                }
            }

            return outDictionary;
        }

        public static void PackBitmapDictionary(Dictionary<string, Bitmap> bitmaps, LevelSave save)
        {
            var path = $"Levels\\{save.LevelName}\\Assets\\";
            LevelFolderWorker.ClearLevelFolder($"{save.LevelName}\\Assets");
            foreach (var key in bitmaps.Keys)
            {
                bitmaps[key].Save(path + key + ".png");
            }
        }
    }
}
