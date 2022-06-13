using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatMeGameModel.Exceptions
{
    public class DeathException : Exception
    {
        public override string Message { get; } = "Suddenly player died";
    }
}
