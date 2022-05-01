using SoundEngineLibrary;

namespace BeatMeGame
{
    interface ISoundProvider
    {
        SoundEngine GetSfxEngine();
        SoundEngine GetMusicEngine();
    }
}
