using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatMeGameModel.Exceptions
{
    public class InterpretException : Exception
    {
        public override string Message { get; }

        public InterpretException(string message)
        {
            Message = message;
        }
    }
}
