using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace BeatMeGameModel.IOWorkers
{
    [Serializable]
    public class EditorSettings
    {
        public int TextSize { get; set; } = 10;

        public EditorSettings()
        {
        }

        public static void Serialize(EditorSettings settings)
        {
            using (var fileStream = new FileStream("EditorSettings.txt", FileMode.OpenOrCreate))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(fileStream, settings);
            }
        }

        public static EditorSettings Deserialize()
        {
            if (!File.Exists("EditorSettings.txt")) return new EditorSettings();
            using (var fileStream = new FileStream("EditorSettings.txt", FileMode.OpenOrCreate))
            {
                var formatter = new BinaryFormatter();
                return (EditorSettings)formatter.Deserialize(fileStream);
            }
        }
    }
}
