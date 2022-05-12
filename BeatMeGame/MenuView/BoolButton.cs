using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BeatMeGame.MenuView
{
    public class BoolButton : Button
    {
        public bool IsActivated { get; private set; }

        public BoolButton(){}

        public BoolButton(bool isActivated)
        {
            IsActivated = isActivated;
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            IsActivated = !IsActivated;
        }
    }
}
