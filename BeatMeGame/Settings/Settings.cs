using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace BeatMeGame
{
    public enum ScreenState
    {
        None,
        FullScreen
    }
    internal class Settings
    {
        public int SfxVolume { get; set; } = 50;
        public int MusicVolume { get; set; } = 50;
        public ScreenState ScreenState { get; set; } = ScreenState.None;
        public Size FormSize { get; set; } = new Size(1280, 720);

        private const string FilePath = "Settings.txt";

        public Settings() { }

        public Settings(int sfxVolume, int musicVolume, Size formSize, ScreenState screenState)
        {
            SfxVolume = sfxVolume;
            MusicVolume = musicVolume;
            FormSize = formSize;
            ScreenState = screenState;
        }

        public static void SaveSettingFile(Settings configuration)
        {
            WriteSettingFile(configuration);
        }

        public static Settings ParseSettingFile()
        {
            if (!File.Exists(FilePath)) return CreateDefaultSettingsFile();
            var reader = new StreamReader(new FileStream(FilePath, FileMode.Open), Encoding.UTF8);
            var text = reader.ReadToEnd().Split('\n');
            if (text.Length != 4) CreateDefaultSettingsFile();
            var isParsed = int.TryParse(text[0], out var sfxVolume)
                           & int.TryParse(text[1], out var musicVolume)
                           & SizeExtensions.TryParse(text[3], out var formSize)
                           & ScreenStateExtensions.TryParse(text[2], out var borderStyle);
            if (!isParsed || sfxVolume > 100 || musicVolume > 100 || !SizeExtensions.DefaultSizes.Contains(formSize))
                return CreateDefaultSettingsFile();
            return new Settings(sfxVolume, musicVolume, formSize, borderStyle);
        }


        private static Settings CreateDefaultSettingsFile()
        {
            var settings = new Settings();
            WriteSettingFile(settings);
            return settings;
        }

        private static void WriteSettingFile(Settings file)
        {
            var fileStream = new FileStream(FilePath, FileMode.Create);
            var fileContent = $"{file.SfxVolume}\n{file.MusicVolume}\n{file.ScreenState}\n{file.FormSize}";
            var bytes = new UTF8Encoding(true).GetBytes(fileContent);
            fileStream.Write(bytes, 0, bytes.Length);
            fileStream.Close();
        }
    }
}

