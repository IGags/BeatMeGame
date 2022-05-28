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
        public static string[] ReservedVariables = new[] { "X", "Y", "Angle", "SizeX", "SizeY", "this", "async" };

        public static string[] Actions = new[]
            { "++", "--", "^", "%", "*", "/", "+", "-", "<", ">", ">=", "<=", "==", "!=", "&&", "||", "=" };

        public static char[] ReservedChars = new[]
        {
            '+', '-', ':', '(', ')', '{', '}', '[', ']', '\"', '\'', '*', '^', '&', '!', '?', '$', '/', '\\', '=', '%',
            ',', '.', '#', '№', '@'
        };

        public static string[] BaseModifiersList = new[] { "async" };
        public static string[] MainModifiersList = Array.Empty<string>();
        public static string[] ObjectModifiersList = Array.Empty<string>();

        public static string[] BaseCommandList = new[]
        {
            "move","rotate","scale","spawn","delay","execute",
        };
        public static string[] MainCommandList = new[] { "kill", "freeze", "back" };
        public static string[] ObjectCommandList = Array.Empty<string>();

        public static int GetActionPriority(string action)
        {
            switch (action)
            {
                case "++":
                case "--": return 10;
                case "^": return 9;
                case "%":
                case "*":
                case "/": return 8;
                case "+":
                case "-": return 7;
                case "<":
                case ">":
                case ">=":
                case "<=": return 6;
                case "==":
                case "!=": return 5;
                case "&&": return 4;
                case "||": return 3;
                case "=": return 2;
                default: return 0;
            }
        }

        public static bool GetActionAccess<T1, T2>(T1 first, T2 second, string action)
        {
            if (first.GetType() != second.GetType()) return false;
            switch (action)
            {
                case "++":
                case "--": return (typeof(T1) == typeof(int) | typeof(T1) == typeof(double));
                case "^": 
                case "%": return (typeof(T1) == typeof(double) | typeof(T1) == typeof(int)) & typeof(T2) == typeof(int);
                case "*":
                case "/": 
                case "+":
                case "-":
                    return (typeof(T1) == typeof(double) | typeof(T1) == typeof(int)) &
                           (typeof(T2) == typeof(double) | typeof(T2) == typeof(int));
                case "<":
                case ">":
                case ">=":
                case "<=":
                case "==":
                case "!=":
                case "&&":
                case "||":
                case "=": return true;
                default: return false;
            }
        }
    }
}
