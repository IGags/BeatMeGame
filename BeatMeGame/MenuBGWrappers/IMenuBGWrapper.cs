using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoundEngineLibrary;

namespace BeatMeGame.MenuBGWrappers
{
    interface IMenuBGWrapper
    {
        void Initialize(int x, int y, SoundEngine engine, string treadName);
        void Resize(int x, int y);
        void Redraw(Graphics g);
    }
}
