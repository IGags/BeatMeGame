using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatMeGameModel.Exceptions
{
    public class VariableBlockException : Exception
    {
        public override string Message { get; }

        public VariableBlockException(string message)
        {
            Message = message;
        }
    }
}
