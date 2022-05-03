using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using BeatMeGameModel.BeatVertexes;

namespace BeatMeGameModel.IOWorkers
{
    public class LevelFolderWorker
    {
        private const string Path = "Levels";

        public static string[] FindLevels()
        {
            if (!Directory.Exists(Path)) Directory.CreateDirectory(Path);
            return Directory.GetDirectories(Path).Select(value => value.Split('\\')[1]).ToArray();
        }

        public static bool TryCreateLevelFolder(string name)
        {
            var path = Path + "\\" + name;
            if (Directory.Exists(path)) return false;
            Directory.CreateDirectory(path);
            return true;
        }

        public static void DeleteLevel(string name)
        {
            var path = Path + "\\" + name;
            if (!Directory.Exists(path)) return;
            Directory.Delete(path, true);
        }

        public static bool TryRenameLevel(string name, string newName)
        {
            var path = Path + "\\" + name;
            var newPath = Path + "\\" + newName;
            if (Directory.Exists(newPath)) return false;
            Directory.Move(path, newPath);
            return true;
        }

        public static bool TryReadFile(string levelName, out string outString, string fileName)
        {
            var path = Path + "\\" + levelName + "manifest.txt";
            if (!Directory.Exists(path))
            {
                outString = null;
                return false;
            }

            outString = File.ReadAllText(path);
            return true;
        }

        public static void ClearLevelFolder(string levelName, params string[] protectedValues)
        {
            var path = Path + "\\" + levelName;
            foreach (var name in Directory.EnumerateFileSystemEntries(path))
            {
                if(protectedValues.Contains(name)) continue;
                File.Delete(name);
            }
        }

        public static bool CheckFileExist(string levelName, string fileName)
        {
            var path = Path + "\\" + levelName + "\\" + fileName;
            return File.Exists(path);
        }

        public static void CopyMp3ToLevelFolder(string path, string fileName)
        {
            File.Copy(path, Path + "\\" + fileName);
        }
        
        public static void DeleteFile(string levelName, string fileName)
        {
            var path = Path + "\\" + levelName + "\\" + fileName;
            if(!File.Exists(path)) return;
            File.Delete(path);
        }

        public static void SaveFile(string levelName, string fileName, string content)
        {
            var path = Path + "\\" + levelName + "\\" + fileName;
            File.WriteAllText(path, content);
        }
    }
}
