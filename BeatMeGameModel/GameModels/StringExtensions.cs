using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatMeGameModel.EitorModels;
using BeatMeGameModel.Exceptions;

namespace BeatMeGameModel.GameModels
{
    public enum TokenType
    {
        Action,
        Brackets,
        Variable,
        Function
    }
    public static class StringExtensions
    {
        private static readonly char[] TrimConstants = new[] { '\n', '\r', '\t', ' ' };
        public static string RemoveSplitters(this string input)
        {
            var outString = new StringBuilder();
            foreach (var symbol in input.Where(symbol => !TrimConstants.Contains(symbol)))
            {
                outString.Append(symbol);
            }

            return outString.ToString();
        }


        private static string BuildBracketToken(string rawString)
        {
            var bracketStack = new Stack<bool>();
            var bracketBuilder = new StringBuilder();
            foreach (var symbol in rawString)
            {
                switch (symbol)
                {
                    case '(':
                        bracketStack.Push(true);
                        break;
                    case ')':
                        bracketStack.Pop();
                        break;
                }

                bracketBuilder.Append(symbol);
                if (!bracketStack.Any()) return bracketBuilder.ToString();
            }

            throw new BracketException("Incorrect bracket expression");
        }

        private static bool CompareStringStart(int startIndex, string rawString, string[] toCompare)
        {
            var skippedString = string.Concat(rawString.Skip(startIndex + 1));
            return toCompare.Any(token => skippedString.StartsWith(token));
        }
    }
}
