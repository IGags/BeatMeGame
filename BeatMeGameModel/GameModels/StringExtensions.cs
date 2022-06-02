using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatMeGameModel.EitorModels;
using BeatMeGameModel.Exceptions;

namespace BeatMeGameModel.GameModels
{
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

        public static Tag ParseTag(this string tag)
        {
            switch (tag)
            {
                case "ship": return Tag.Ship;
                case "bullet": return Tag.Bullet;
                case "laser": return Tag.Laser;
                default: throw new ArgumentException("Invalid Object Tag");
            }
        }
    }
}
