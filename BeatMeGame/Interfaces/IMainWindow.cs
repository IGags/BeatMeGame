using BeatMeGameModel;

namespace BeatMeGame
{
    interface IMainWindow
    {
        Settings GetSettings();
        void SetSettings(Settings settings);
        void RunEditor(LevelSave save);
    }
}
