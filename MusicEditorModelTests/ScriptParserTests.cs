using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BeatMeGameModel.EitorModels;
using BeatMeGameModel.Exceptions;
using BeatMeGameModel.GameModels;
using NUnit.Framework;

namespace MusicEditorModelTests
{
    [TestFixture]
    class ScriptParserTests
    {
        [Test]
        public void ParseCorrectFile()
        {
            var file =
                @"[Main]
Variables
{
    int: a,b,c;
    double: d,e;
}

Program
{
    a = b; 
    roma;
    for(14){
a;
b;
}
roma;
}
";
            var intDictionary = new Dictionary<string, int>(){["a"] = 0,["b"] = 0, ["c"] = 0};
            var doubleDictionary = new Dictionary<string, double>(){ ["d"] = 0, ["e"] = 0};
            var script = new GameObjectScript(file.Split('\n'));
            CompareVariables(intDictionary, doubleDictionary, script);
            CompareProgramBlocksLength(7, script);
        }

        [Test]
        public void ParesEmptyScript()
        {
            var file =
                @"[Main]
Variables{}
Program{}";
            var intDictionary = new Dictionary<string, int>();
            var doubleDictionary = new Dictionary<string, double>();
            var script = new GameObjectScript(file.Split('\n'));
            CompareVariables(intDictionary, doubleDictionary, script);
            CompareProgramBlocksLength(0, script);
        }

        [Test]
        public void ParseNotFullVariableBlock()
        {
            var file =
                @"[Main]
Variables
{int:a;
double:b;}
Program{}";
            var intDictionary = new Dictionary<string, int>(){["a"]= 0};
            var doubleDictionary = new Dictionary<string, double>() { ["b"] = 0 };
            var script = new GameObjectScript(file.Split('\n'));
            CompareVariables(intDictionary, doubleDictionary, script);
        }

        [Test]
        public void ParseReservedVariableBlock()
        {
            var file =
                @"[Main]
Variables{
double: self;
Program{}";
            Assert.Catch<VariableBlockException>(() => new GameObjectScript(file.Split('\n')));
        }

        [Test]
        public void TestLineBreak()
        {
            var file =
                @"


                  [Main]



Variables{double:a, b,c;int:r}



Program    



{}";
            Assert.DoesNotThrow(() => new GameObjectScript(file.Split('\n')));
        }

        [Test]
        public void VariableNameStartsWithInt()
        {
            var file =
                @"
                  [Main]
                  Variables
                  {
                     int: 8roma;
                  }";
            Assert.Catch<VariableBlockException>(() => new GameObjectScript(file.Split('\n')));
        }

        [Test]
        public void DoubleVariableDefinition()
        {
            var file = @"[Main]
Variables{double: a; int: a;}
Program{}";
            Assert.Catch<VariableBlockException>(() => new GameObjectScript(file.Split('\n')));
        }

        [Test]
        public void ParseReservedSymbol()
        {
            var file = @"[Main]
Variables {int: a.}
Program{}";
            Assert.Catch<VariableBlockException>(() => new GameObjectScript(file.Split('\n')));
        }

        [Test]
        public void ParseIncorrectBracketExpressionInFile()
        {
            var file = @"[Main]
Variables{}
Program{{{}{}{{
{{}{}}}}";
            Assert.Catch<BracketException>(() => new GameObjectScript(file.Split('\n')));
        }

        [Test]
        public void ParseIncorrectBracketExpressionInString()
        {
            var file = @"[Main]
Variables{}
Program
{
    ()()))()(((()))
}";

            Assert.Catch<BracketException>(() => new GameObjectScript(file.Split('\n')));
        }

        [Test]
        public void MathEquationInterpret()
        {
            var file = @"[Main]
Variables{int:a, b, c}
Program
{
    a = 1;
    b = 2;
    c = a + b;
}";
            var script = new GameObjectScript(file.Split('\n'));
            var game = new Game(script, new Dictionary<string, GameObjectScript>(), new PlayerStub(0, 0));
            script.Start(TimeSpan.Zero, game);
            script.Interpret(new TimeSpan(0,0,0,1));
            var type = typeof(GameObjectScript);
            var intField = (Dictionary<string, int>)(type
                .GetField("intVariablesDictionary", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(script));
            Assert.AreEqual(3, intField["c"]);
        }

        [Test]
        public void ForCycleInterpret()
        {
            var file = @"[Main]
Variables{int:a, b}
Program
{
a = 0;
b = 2;
for(40)
{
    a = a + b;
}
}";
            var script = new GameObjectScript(file.Split('\n'));
            var game = new Game(script, new Dictionary<string, GameObjectScript>(), new PlayerStub(0, 0));
            script.Start(TimeSpan.Zero, game);
            script.Interpret(new TimeSpan(0, 0, 0, 1));
            var type = typeof(GameObjectScript);
            var intField = (Dictionary<string, int>)(type
                .GetField("intVariablesDictionary", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(script));
            Assert.AreEqual(80, intField["a"]);
        }

        [Test]
        public void DoubleCycleTest()
        {
            var file = @"[Main]
Variables{int:a, b}
Program
{
a = 0;
b = 2;
for(40)
{
    for(2)
    {
        a = a + b;   
    }
}
}";
            var script = new GameObjectScript(file.Split('\n'));
            var game = new Game(script, new Dictionary<string, GameObjectScript>(), new PlayerStub(0, 0));
            script.Start(TimeSpan.Zero, game);
            script.Interpret(new TimeSpan(0, 0, 0, 1));
            var type = typeof(GameObjectScript);
            var intField = (Dictionary<string, int>)(type
                .GetField("intVariablesDictionary", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(script));
            Assert.AreEqual(160, intField["a"]);
        }

        [Test]
        public void ScriptInvocationInterpret()
        {
            var file = @"[Main]
Variables{}
Program
{
    execute(roma)
}";
            var roma = @"[Main]
Variables{}
Program
{
    delay(3)
}";
            var script = new GameObjectScript(file.Split('\n'));
            var game = new Game(script, new Dictionary<string, GameObjectScript>(){["roma"] = new GameObjectScript(roma.Split('\n'))}, new PlayerStub(0, 0));
            game.GameStart(TimeSpan.Zero);
            Assert.AreEqual(false, script.IsEnded);
            var type = typeof(Game);
            var scripts = (List<GameObjectScript>)type
                .GetField("activeScripts", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(game);
            game.GetGameStateByTime(new TimeSpan(0,0,0,1));
            Assert.AreEqual(false, scripts[0].IsEnded);
            Assert.AreEqual(1, scripts.Count);
            game.GetGameStateByTime(new TimeSpan(0,0,0,4));
            game.GetGameStateByTime(new TimeSpan(0, 0, 0, 4, 1));
            Assert.AreEqual(true, script.IsEnded);
            Assert.AreEqual(true, scripts[0].IsEnded);
            
        }

        [Test]
        public void CheckMoveFunction()
        {
            var file = @"[Main]
Variables{}
Program
{
    spawn(0, 0, 1, 1, 0, roma, move)
}";
            var move = @"[object]
Variables{}
Program
{
    move(100, 100, 0)
}";
            CompareObjectState(file, move, 1, 100, 100, 0, 1);
        }

        [Test]
        public void CheckMoveFunctionInParametricEquation()
        {
            var file = @"[Main]
Variables{}
Program
{
    spawn(0, 0, 1, 1, 0, roma, move)
}";
            var move = @"[object]
Variables{}
Program
{
    move(100 * t, 100 * t, 1)
}";
            CompareObjectState(file, move, 1, 100, 100, 0, 1);
        }

        [Test]
        public void CheckObjectReference()
        {
            var file = @"[Main]
Variables{}
Program
{
    spawn(0,0,1,1,0, roma, move)
}";
            var check = @"[object]
Variables{}
Program
{
    move(player1X, player1Y, 0);
}";
            CompareObjectState(file, check, 1, 15, 20, 0, 1, new PlayerStub(15, 20));

        }

        [Test]
        public void CheckMethodSequence()
        {
            var file = @"[Main]
Variables{}
Program
{
    spawn(0,0,1,1,0, roma, move)
}";
            var check = @"[object]
Variables{}
Program
{
    move(20, 20, 0);
    move(10, 11, 0);
}";
            CompareObjectState(file, check, 1, 30, 31, 0, 1, new PlayerStub(15, 20));
        }

        [Test]
        public void CheckNotFullPerformance()
        {
            var file = @"[Main]
Variables{}
Program
{
    spawn(0,0,1,1,0, roma, move)
}";
            var check = @"[object]
Variables{}
Program
{
    move(20, 20, 2);
}";
            CompareObjectState(file, check, 1, 10.01, 10.01, 0, 1, new PlayerStub(15, 20));
        }

        private void CompareObjectState(string main, string objectScript, int expectedCount, double expectedX, double expectedY, double expectedAngle, int waitTime, PlayerStub playerStub = null)
        {
            var script = new GameObjectScript(main.Split('\n'));
            var scriptDictionary = new Dictionary<string, GameObjectScript>()
                { ["move"] = new GameObjectScript(objectScript.Split('\n')) };
            var game = playerStub != null ? new Game(script, scriptDictionary, playerStub) : new Game(script, scriptDictionary, new PlayerStub(0, 0));
            game.GameStart();
            game.GetGameStateByTime(new TimeSpan(0, 0, 0, waitTime));
            game.GetGameStateByTime(new TimeSpan(0, 0, 0, waitTime, 1));
            var type = typeof(Game);
            var objects = (List<GameObject>)type
                .GetField("gameObjects", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(game);
            Assert.AreEqual(expectedCount, objects.Count);
            Assert.AreEqual(expectedX, objects[0].XPosition);
            Assert.AreEqual(expectedY, objects[0].YPosition);
            Assert.AreEqual(expectedAngle, objects[0].Angle);
        }


        private void CompareVariables(Dictionary<string, int> intDictionary,
            Dictionary<string, double> doubleDictionary, GameObjectScript script)
        {
            var type = script.GetType();
            var intField = (Dictionary<string, int>)(type
                .GetField("intVariablesDictionary", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(script));
            Assert.AreEqual(intDictionary, intField);
            var doubleField = (Dictionary<string, double>)(type
                .GetField("doubleVariablesDictionary", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(script));
            Assert.AreEqual(doubleDictionary, doubleField);
        }

        private void CompareProgramBlocksLength(int expectedLength, GameObjectScript script)
        {
            var type = script.GetType();
            var programBlock = (string[])(type
                .GetField("extractedCommands", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(script));
            Assert.AreEqual(expectedLength, programBlock.Length);
        }
    }
}
