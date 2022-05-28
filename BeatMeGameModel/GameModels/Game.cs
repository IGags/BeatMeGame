using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatMeGameModel.EitorModels;

namespace BeatMeGameModel.GameModels
{
    public class Game
    {
        private readonly Stopwatch inGameTimeStopwatch = new Stopwatch();
        private GameObjectScript mainScript;
        private readonly Dictionary<string, GameObjectScript> scripts;
        private List<GameObject> gameObjects;
        private List<GameObjectScript> activeScripts;
        public Game(GameObjectScript mainScript, Dictionary<string, GameObjectScript> scripts)
        {
            this.mainScript = mainScript;
            inGameTimeStopwatch.Restart();
            this.scripts = scripts;
        }

    }
}
