using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatMeGameModel.EitorModels;
using BeatMeGameModel.Exceptions;

namespace BeatMeGameModel.GameModels
{
    public class Game
    {
        public Player Player1 { get; private set; } = new Player();
        public Player Player2 { get; private set; } = new Player();

        private readonly Stopwatch inGameTimeStopwatch = new Stopwatch();
        private GameObjectScript mainScript;
        private readonly Dictionary<string, GameObjectScript> scripts;
        private List<GameObject> gameObjects = new List<GameObject>();
        private List<GameObjectScript> activeScripts = new List<GameObjectScript>();

        public Game(GameObjectScript mainScript, Dictionary<string, GameObjectScript> scripts)
        {
            this.mainScript = mainScript;
            inGameTimeStopwatch.Restart();
            this.scripts = scripts;
        }

        public void Spawn(double x, double y, double xSize, double ySize, string name, string scriptName)
        {
            var gameObject = new GameObject(x, y, 0, xSize, ySize, name);
            gameObjects.Add(gameObject);
            var gameScript = scripts[scriptName].Copy();
            if (gameScript.ScriptAccessLevel == AccessLevel.MainScript)
                throw new TagException("Access level higher than object access level");
            gameScript.Start(inGameTimeStopwatch.Elapsed, this, gameObject);
            activeScripts.Add(gameScript);
        }

        public void InvokeGameObjectScript(string scriptName, GameObjectScript invoker, GameObject targetObject)
        {
            if (!scripts.ContainsKey(scriptName) || scripts[scriptName].ScriptAccessLevel != invoker.ScriptAccessLevel)
                throw new InvalidOperationException("Cannot invoke script with different access level");
            var script = scripts[scriptName].Copy();
            script.Start(inGameTimeStopwatch.Elapsed, this, targetObject, invoker);
            activeScripts.Add(script);
        }

        public void GameStart()
        {
            inGameTimeStopwatch.Restart();
            mainScript.Start(inGameTimeStopwatch.Elapsed, this);
        }

        public void GameStart(TimeSpan time)
        {
            mainScript.Start(time, this);
        }

        public void GetGameStateByInnerTime()
        {
            foreach (var script in activeScripts)
            {
                script.Interpret(inGameTimeStopwatch.Elapsed);
            }
            mainScript.Interpret(inGameTimeStopwatch.Elapsed);
        }

        public void GetGameStateByTime(TimeSpan time)
        {
            mainScript.Interpret(time);
            foreach (var script in activeScripts)
            {
                script.Interpret(time);
            }
        }
    }
}
