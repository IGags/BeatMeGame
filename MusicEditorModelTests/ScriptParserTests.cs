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
    dodik;
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
            CompareProgramBlocksLength(8, script);
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
{string:a;
bool:b;}
Program{}";
            var intDictionary = new Dictionary<string, int>();
            var doubleDictionary = new Dictionary<string, double>();
            var script = new GameObjectScript(file.Split('\n'));
            CompareVariables(intDictionary, doubleDictionary, script);
        }

        [Test]
        public void ParseReservedVariableBlock()
        {
            var file =
                @"[Main]
Variables{
string: self;
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
            var game = new Game(script, new Dictionary<string, GameObjectScript>());
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
            var game = new Game(script, new Dictionary<string, GameObjectScript>());
            script.Start(TimeSpan.Zero, game);
            script.Interpret(new TimeSpan(0, 0, 0, 1));
            var type = typeof(GameObjectScript);
            var intField = (Dictionary<string, int>)(type
                .GetField("intVariablesDictionary", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(script));
            Assert.AreEqual(80, intField["a"]);
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
            var game = new Game(script, new Dictionary<string, GameObjectScript>(){["roma"] = new GameObjectScript(roma.Split('\n'))});
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
    spawn(0, 0, 1, 1, roma, move)
}";
            var move = @"[object]
Variables{}
Program
{
    move(100, 100, 0)
}";
            var script = new GameObjectScript(file.Split('\n'));
            var game = new Game(script,
                new Dictionary<string, GameObjectScript>() { ["move"] = new GameObjectScript(move.Split('\n')) });
            game.GameStart();
            game.GetGameStateByTime(new TimeSpan(0, 0, 0, 1));
            game.GetGameStateByTime(new TimeSpan(0, 0, 0, 1, 1));
            var type = typeof(Game);
            var objects = (List<GameObject>)type
                .GetField("gameObjects", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(game);
            Assert.AreEqual(1, objects.Count);
            Assert.AreEqual(100, objects[0].XPosition);
            Assert.AreEqual(100, objects[0].YPosition);
        }

        [Test]
        public void CheckMoveFunctionInParametricEquation()
        {
            var file = @"[Main]
Variables{}
Program
{
    spawn(0, 0, 1, 1, roma, move)
}";
            var move = @"[object]
Variables{}
Program
{
    move(100 * t, 100 * t, 1)
}";
            var script = new GameObjectScript(file.Split('\n'));
            var game = new Game(script,
                new Dictionary<string, GameObjectScript>() { ["move"] = new GameObjectScript(move.Split('\n')) });
            game.GameStart();
            game.GetGameStateByTime(new TimeSpan(0, 0, 0, 1));
            game.GetGameStateByTime(new TimeSpan(0, 0, 0, 1, 1));
            var type = typeof(Game);
            var objects = (List<GameObject>)type
                .GetField("gameObjects", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(game);
            Assert.AreEqual(1, objects.Count);
            Assert.AreEqual(100, objects[0].XPosition);
            Assert.AreEqual(100, objects[0].YPosition);
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
