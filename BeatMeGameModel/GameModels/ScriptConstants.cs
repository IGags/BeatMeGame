using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatMeGameModel.EitorModels
{
    public static class ScriptConstants
    {
        public const char EndConstant = ';';

        public static string[] ReservedVariables = { "X", "Y", "Angle", "SizeX", "SizeY", "this", "async", "player1X", "player2X", "player1Y", "player2Y" };

        public static char[] ReservedChars = {
            '+', '-', ':', '(', ')', '{', '}', '[', ']', '\"', '\'', '*', '^', '&', '!', '?', '$', '/', '\\', '=', '%',
            ',', '.', '#', '№', '@'
        };
    }
}
