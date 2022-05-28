using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatMeGameModel.Exceptions
{
    public class TagException : Exception
    {
        public override string Message { get; }

        public TagException(string message)
        {
            Message = message;
        }
    }
}
