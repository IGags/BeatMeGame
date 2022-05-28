using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatMeGameModel.EitorModels;

namespace BeatMeGameModel.GameModels
{
    public class CommandWrapper
    {
        public string CommandName { get; }
        public List<object> ParamsList { get; }

        public CommandWrapper(string commandName, List<object> paramsList)
        {
            CommandName = commandName;
            ParamsList = paramsList;
        }
    }
}
