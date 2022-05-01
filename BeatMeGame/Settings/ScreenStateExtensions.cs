using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BeatMeGame
{
    public static class ScreenStateExtensions
    {
        public static bool TryParse(string value, out ScreenState outParameter)
        {
            switch (value)
            {
                case "None":
                    outParameter = ScreenState.None;
                    return true;
                case "FullScreen":
                    outParameter = ScreenState.FullScreen;
                    return true;
                default:
                    outParameter = ScreenState.None;
                    return false;
            }
        }
    }
}
