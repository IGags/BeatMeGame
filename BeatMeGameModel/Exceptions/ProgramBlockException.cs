using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatMeGameModel.Exceptions
{
    public class ProgramBlockException : Exception
    {
        public override string Message { get; }

        public ProgramBlockException(string message)
        {
            Message = message;
        }
    }
}
