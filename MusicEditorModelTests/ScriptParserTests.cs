using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
