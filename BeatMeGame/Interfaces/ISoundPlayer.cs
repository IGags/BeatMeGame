using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatMeGame.MenuView;

namespace BeatMeGame.Interfaces
{
    interface ISoundPlayer
    {
        MenuSoundEngine MusicEngine { get; }
    }
}
