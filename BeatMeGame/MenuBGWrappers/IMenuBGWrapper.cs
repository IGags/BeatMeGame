using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatMeGame.MenuView;
using SoundEngineLibrary;

namespace BeatMeGame.MenuBGWrappers
{
    interface IMenuBGWrapper
    {
        void Initialize(int x, int y, MenuSoundEngine engine, string treadName);
        void Resize(int x, int y);
        void Redraw(Graphics g);
    }
}
