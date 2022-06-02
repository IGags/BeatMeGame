using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatMeGameModel.GameModels;

namespace BeatMeGameModel.IOWorkers
{
    public static class LevelAssetsParser
    {
        public static Dictionary<string, string[]> ParseScripts(string levelName)
        {
            var path = $"Levels\\{levelName}\\Scripts";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return new Dictionary<string, string[]>() { ["Main"] = @"[Main] Variables{} Program{}".Split(' ') };
            }

            if (!File.Exists(path + "\\Main.txt"))
                return new Dictionary<string, string[]>() { ["Main"] = @"[Main] Variables{} Program{}".Split(' ') };
            var outDictionary = new Dictionary<string, string[]>();
            foreach (var file in Directory.GetFiles(path))
            {
                var fileName = file.Split('.')[0].Split('\\');
                outDictionary[fileName[fileName.Length - 1]] = File.ReadAllText(file).Split('\n');
            }

            return outDictionary;
        }

        public static void PackScripts(Dictionary<string, string[]> scripts, string levelName)
        {
            foreach (var key in scripts.Keys)
            {
                File.WriteAllLines($"Levels\\{levelName}\\Scripts\\{key}.txt", scripts[key]);
            }
        }
    }
}
