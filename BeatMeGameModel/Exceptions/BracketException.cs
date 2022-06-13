using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatMeGameModel.Exceptions
{
    public class BracketException : Exception
    {
        public override string Message { get; }

        public BracketException(string message)
        {
            Message = message;
        }
    }
}
