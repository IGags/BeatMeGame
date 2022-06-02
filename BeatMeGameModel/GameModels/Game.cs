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
    public class Game //проверить исполение на большом тайминге(починить для редактора костылём)
    {
        public IPlayer Player1 { get; private set; }
        public IPlayer Player2 { get; private set; }
        public bool IsPaused { get; private set; }
        public bool IsPlayerCollisionEnabled { get; set; }
        public event Action PlayerDeath;

        public const double PlayerSize = 50;
        public const double PlayerHitBoxSize = 7;
        public const double GameWidth = 1000;
        public const double GameHeight = 1000;

        private readonly Stopwatch inGameTimeStopwatch = new Stopwatch();
        private GameObjectScript mainScript;
        private readonly Dictionary<string, GameObjectScript> scripts;
        private List<GameObject> gameObjects = new List<GameObject>();
        private List<GameObjectScript> activeScripts = new List<GameObjectScript>();
        private TimeSpan lastAccessTime;
        private long id;

        public Game(GameObjectScript mainScript, Dictionary<string, GameObjectScript> scripts, IPlayer player1, IPlayer player2 = null)
        {
            Player1 = player1;
            Player2 = player2;
            this.mainScript = mainScript;
            inGameTimeStopwatch.Restart();
            this.scripts = scripts;
        }

        //DeepCopy
        private Game(IPlayer player1, IPlayer player2, bool isPaused, bool isPlayerCollisionEnabled,
            GameObjectScript mainScript, Dictionary<string, GameObjectScript> scripts, List<GameObject> gameObjects,
            List<GameObjectScript> activeScripts, TimeSpan lastAccessTime, long id)
        {
            Player1 = player1.Copy();
            Player2 = player2.Copy();
            IsPaused = isPaused;
            IsPlayerCollisionEnabled = isPlayerCollisionEnabled;
            this.mainScript = mainScript.Copy();
            this.scripts = scripts.ToDictionary(key => key.Key, value => value.Value);
            this.gameObjects = gameObjects.Select(obj => obj.Copy()).ToList();
            this.activeScripts = activeScripts.Select(scr => scr.Copy()).ToList();
            this.lastAccessTime = lastAccessTime;
            this.id = id;
        }

        public void Spawn(double x, double y, double xSize, double ySize, double angle, string name, string scriptName, Tag tag)
        {
            var gameObject = new GameObject(x, y, angle, xSize, ySize, name, id.ToString(), tag);
            id++;
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
            GetGameState(inGameTimeStopwatch.Elapsed);
        }

        public void Pause()
        {
            IsPaused = true;
            inGameTimeStopwatch.Stop();
        }

        public void Resume()
        {
            IsPaused = false;
            inGameTimeStopwatch.Start();
        }

        public void GetGameStateByTime(TimeSpan time)
        {
            GetGameState(time);
        }

        public void CheckPlayerEnemyProjectilesCollision()
        {
            if(!IsPlayerCollisionEnabled) return;
            CheckPlayerEnemyBulletCollision(Player1);
            if(Player2 != null)CheckPlayerEnemyBulletCollision(Player2);
        }

        public Game Copy()
        {
            return new Game(Player1, Player2, IsPaused, IsPlayerCollisionEnabled, mainScript, scripts, gameObjects,
                activeScripts, lastAccessTime, id);
        }

        public void PlayerMove(bool isSecondPlayer, Directions direction)
        {
            var player = isSecondPlayer ? Player2 : Player1;
            if(Player2 == null) return;
            switch (direction)
            {
                case Directions.Up:
                    if (player.Y + PlayerSize / 2 + player.Velocity > GameHeight)
                        player.Y = GameHeight - PlayerSize / 2;
                    else player.Y += player.Velocity;
                    break;
                case Directions.Down:
                    if (player.Y - PlayerSize / 2 - player.Velocity < 0)
                        player.Y = 0 + PlayerSize / 2;
                    else player.Y -= player.Velocity;
                    break;
                case Directions.Right:
                    if (player.X + PlayerSize / 2 + player.Velocity > GameWidth)
                        player.X = GameWidth - PlayerSize / 2;
                    else player.X += player.Velocity;
                    break;
                case Directions.Left:
                    if (player.X - PlayerSize / 2 - player.Velocity < 0)
                        player.X = 0 + PlayerSize / 2;
                    else player.X -= player.Velocity;
                    break;
            }
        }

        private void GetGameState(TimeSpan time)
        {
            if (IsPaused) return;
            activeScripts = activeScripts.Where(scr => !scr.IsEnded).ToList();
            gameObjects = gameObjects.Where(obj => obj.ReferenceCount > 0).ToList();
            mainScript.Interpret(time);
            foreach (var script in activeScripts)
            {
                script.Interpret(time);
            }
            CheckPlayerEnemyProjectilesCollision();
            lastAccessTime = time;
        }

        private void CheckPlayerEnemyBulletCollision(IPlayer player)
        {
            foreach (var obj in gameObjects)
            {
                if (!CollideCheck(obj.XPosition - obj.XSize / 2, player.X - PlayerHitBoxSize / 2,
                        obj.YPosition - obj.YSize / 2, player.Y - PlayerHitBoxSize / 2) || !CollideCheck(
                        obj.XPosition + obj.XSize / 2, player.X + PlayerHitBoxSize / 2,
                        obj.YPosition + obj.YSize / 2, player.Y + PlayerHitBoxSize / 2)) continue;
                inGameTimeStopwatch.Stop();
                PlayerDeath?.Invoke();
                return;
            }
        }

        private bool CollideCheck(double r1Min, double r2Min, double r1Max, double r2Max)
        {
            return r2Min < r1Min && r1Min <= r2Max || r1Min < r2Min && r2Min <= r1Max || Math.Abs(r1Min - r2Min) < 1e-5;
        }
    }
}
