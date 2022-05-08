using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BeatMeGame.MenuView
{
    public class RedirectionButton : Button
    {
        public State Direction { get; }

        public RedirectionButton()
        {
            Direction = State.MenuList;
        }

        public RedirectionButton(State direction)
        {
            Direction = direction;
        }
    }
}
