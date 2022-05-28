using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using BeatMeGameModel.Attributes;
using BeatMeGameModel.EitorModels;
using BeatMeGameModel.Exceptions;
using BeatMeGameModel.GameModels;
using AngouriMath;
using AngouriMath.Extensions;

[assembly: InternalsVisibleTo("ModelTests")]
namespace BeatMeGameModel.GameModels
{
    public class GameObjectScript
    {
        private enum AccessLevel
        {
            ObjectScript,
            MainScript
        }

        public GameObject TargetObject { get; set; }
        public bool IsEnded { get; private set; }

        [Variable] private readonly Dictionary<string, int> intVariablesDictionary = new Dictionary<string, int>();

        [Variable] private readonly Dictionary<string, double> doubleVariablesDictionary =
            new Dictionary<string, double>();

        private Dictionary<int, (int, int)> brackets = new Dictionary<int, (int, int)>();
        private readonly Stack<int> cycleIterationStack = new Stack<int>();
        private readonly int variableTypeCount = 2;
        private double currentCommandPerforamcePrecent;
        private TimeSpan startTime;
        private TimeSpan executionTime;
        private int commandPointerPosition;
        private AccessLevel accessLevel;
        private readonly string[] commandSequence;
        private string[] extractedCommands;
        private bool isStarted;

        public GameObjectScript(string[] commandSequence)
        {
            this.commandSequence = commandSequence;
            Initialize(commandSequence);
        }

        public void Start(TimeSpan time, GameObject targetObject = null)
        {
            if (accessLevel == AccessLevel.ObjectScript && targetObject == null)
                throw new ArgumentException("Invalid Object Initialization");
            startTime = time;
            isStarted = true;
            TargetObject = targetObject;
        }

        public void Interrupt()
        {
            isStarted = false;
        }

        public void Resume()
        {
            if (isStarted) return;
            if (TargetObject == null && accessLevel == AccessLevel.ObjectScript)
                throw new ArgumentException("CannotResume");
            isStarted = true;
        }

        public void Interpret(TimeSpan time)
        {
            if (time < executionTime) throw new InterpretException("queryTimeLessThanInterpretTime");
            if(accessLevel == AccessLevel.MainScript) MainInterpret(time);
            else ObjectInterpret(time);
        }

        private void MainInterpret(TimeSpan time)
        {
            while (executionTime < time)
            {
                //ParseAttributes
                if (commandPointerPosition >= extractedCommands.Length)
                {
                    IsEnded = true;
                    return;
                }

                var currentCommand = extractedCommands[commandPointerPosition].RemoveSplitters()
                    .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[0];

                if (currentCommand.Contains('='))
                {
                    //if attribute parsed throw new exception
                    var parts = currentCommand.Split('=');
                    if (parts.Length != 2) throw new InterpretException("Evaluation Is incorrect");
                    if (doubleVariablesDictionary.ContainsKey(parts[0]))
                    {
                        doubleVariablesDictionary[parts[0]] = Evaluate(parts[1]);
                    }
                    else if(intVariablesDictionary.ContainsKey(parts[0]))
                    {
                        intVariablesDictionary[parts[0]] = (int)Evaluate(parts[1]);
                    }

                    commandPointerPosition++;
                    
                    continue;
                }

                if (currentCommand.StartsWith("for"))
                {
                    if (brackets.ContainsKey(commandPointerPosition))
                    {
                        if (brackets[commandPointerPosition].Item2 == 0)
                        {
                            commandPointerPosition = brackets[commandPointerPosition].Item1 + 1;
                            brackets.Remove(brackets[commandPointerPosition - 1].Item1);
                            brackets.Remove(commandPointerPosition);
                            continue;
                        }
                    }
                    else InitializeFor(currentCommand);
                    if (currentCommand.Contains('{')) currentCommand = currentCommand.Split('{')[2];
                }

                ParseCommand
            }
        }

        private void ObjectInterpret(TimeSpan time) {}

        private void Initialize(string[] commands)
        {
            var tagPosition = ParseAccessLevel();
            var variableContainerEndPosition = ParseVariables(tagPosition);
            var programBlockEdges = FindAndValidateProgramBlock(variableContainerEndPosition + 1);
            for (int i = programBlockEdges.Item1; i <= programBlockEdges.Item2; i++)
                try
                {
                    CheckStringBracketExpression(commandSequence[i], '(', ')', 0);
                }
                catch (BracketException e)
                {
                    throw new BracketException($"Line {i + 1} : {e.Message}");
                }

            var rawExtractedCommands = commandSequence.Skip(programBlockEdges.Item1 + 1)
                .Take(programBlockEdges.Item2 - programBlockEdges.Item1).Select(value => value.RemoveSplitters()).Skip(1)
                .ToArray();
            if(!rawExtractedCommands.Any())
            {
                extractedCommands = rawExtractedCommands;
                return;
            }

            rawExtractedCommands = rawExtractedCommands.SkipWhile(value => value.RemoveSplitters() == "").ToArray();
            if (rawExtractedCommands[0][0] == '{') rawExtractedCommands[0] = rawExtractedCommands[0].Skip(1).ToString();
            if (rawExtractedCommands[rawExtractedCommands.Length - 1] == "}")
                rawExtractedCommands = rawExtractedCommands.Take(rawExtractedCommands.Length - 1).ToArray();
            else if (rawExtractedCommands[rawExtractedCommands.Length - 1][
                         rawExtractedCommands[rawExtractedCommands.Length - 1].Length - 1] == '}')
                rawExtractedCommands[rawExtractedCommands.Length - 1] =
                    rawExtractedCommands[rawExtractedCommands.Length - 1].Substring(0,
                        rawExtractedCommands[rawExtractedCommands.Length - 1].Length - 2);
            extractedCommands = rawExtractedCommands;
        }

        private int ParseAccessLevel()
        {
            for (int i = 0; i < commandSequence.Length; i++)
            {
                var commandString = commandSequence[i].RemoveSplitters();
                if (commandString == "") continue;
                switch (commandString)
                {
                    case "[object]":
                        accessLevel = AccessLevel.ObjectScript;
                        return i;
                    case "[Main]":
                        accessLevel = AccessLevel.MainScript;
                        return i;

                    default:
                        throw new TagException("Incorrect Tag");
                }
            }

            throw new TagException("Tag doesn't exists");
        }

        private int ParseVariables(int tagPosition)
        {
            for (int i = tagPosition; i < commandSequence.Length; i++)
            {
                var commandString = commandSequence[i].RemoveSplitters();
                if (commandString == "") continue;
                if (!commandString.StartsWith("Variables")) continue;
                var variableBlock = AssembleVariableBlock(i, 9);
                i = MakeSequenceShift(i,
                    commandSequence[i].IndexOf("Variables", StringComparison.CurrentCultureIgnoreCase) + 9,
                    variableBlock.Length);
                variableBlock = string.Concat(variableBlock.Where(value => value != '{' && value != '}'));
                CheckLastVariableBlockString(variableBlock);
                FillVariableDictionaries(variableBlock);
                return i;
            }

            throw new VariableBlockException("Integrity of the variable block has been violated");
        }

        private string AssembleVariableBlock(int startString, int startStringPosition)
        {
            var isFirstQuoter = false;
            var charEnumerable = commandSequence.Skip(startString).SelectMany(value => value.RemoveSplitters())
                .Skip(startStringPosition);
            var variableBlockBuilder = new StringBuilder();
            foreach (var symbol in charEnumerable)
            {
                switch (isFirstQuoter)
                {
                    case false when symbol != '{':
                    case true when symbol == '{':
                        throw new VariableBlockException("Integrity of the variable block has been violated");
                }

                if (symbol == '{') isFirstQuoter = true;
                if (isFirstQuoter && symbol == '}')
                {
                    variableBlockBuilder.Append(symbol);
                    return variableBlockBuilder.ToString();
                }

                variableBlockBuilder.Append(symbol);
            }

            throw new VariableBlockException("Integrity of the variable block has been violated");
        }

        private int MakeSequenceShift(int startString, int startStringPosition, int positionCount)
        {
            var isFirst = true;
            for (int i = startString; i < commandSequence.Length; i++)
            {
                for (int j = isFirst ? startStringPosition : 0; j < commandSequence[i].RemoveSplitters().Length; j++)
                {
                    positionCount--;
                    if (positionCount == 0) return i;
                }
                isFirst = false;
            }

            throw new VariableBlockException("Integrity of the block has been violated");
        }

        private void CheckLastVariableBlockString(string blockString)
        {
            var isLastBracket = false;
            foreach (var symbol in blockString.RemoveSplitters())
            {
                if (isLastBracket)
                    throw new VariableBlockException("Integrity of the variable block has been violated");
                if (symbol == '}') isLastBracket = true;
            }
        }

        private void FillVariableDictionaries(string variableBlock)
        {
            var variableStrings = variableBlock.Split(new[] { ScriptConstants.EndConstant },
                StringSplitOptions.RemoveEmptyEntries);
            if (variableStrings.Length > variableTypeCount)
                throw new VariableBlockException("Too many Variable - Types Definitions");
            var usedTypesHashSet = new HashSet<string>();
            foreach (var variable in variableStrings)
            {
                var variableName = variable.Substring(0, variable.IndexOf(':'));
                if (usedTypesHashSet.Contains(variableName)) throw new VariableBlockException("Double type definition");
                usedTypesHashSet.Add(variableName);
                switch (variableName)
                {
                    case "int":
                        FillVariableDictionary(intVariablesDictionary,
                            string.Concat(variable.Skip(variableName.Length + 1)));
                        break;
                    case "double":
                        FillVariableDictionary(doubleVariablesDictionary,
                            string.Concat(variable.Skip(variableName.Length + 1)));
                        break;
                }
            }
        }

        private void FillVariableDictionary<T>(Dictionary<string, T> dictionary, string rawString)
        {
            var variableEnumerable =
                rawString.Substring(rawString.IndexOf(":", StringComparison.InvariantCulture) + 1).Split(',');
            foreach (var variable in variableEnumerable)
            {
                if (variable.Any(value => ScriptConstants.ReservedChars.Contains(value)))
                    throw new VariableBlockException("Variable contains reserved chars");
                if (int.TryParse(variable[0].ToString(), out var empty))
                    throw new VariableBlockException("Variable can't starts with number");
                if (ScriptConstants.ReservedVariables.Contains(variable))
                    throw new VariableBlockException("Variable with reserved name");
                if (intVariablesDictionary.ContainsKey(variable) || doubleVariablesDictionary.ContainsKey(variable)) 
                    throw new VariableBlockException("Double variable definition");
                if (!ScriptExtensions.CheckVariableCorrect(variable)) throw new VariableBlockException("");
                dictionary[variable] = default;
            }
        }

        private void CheckStringBracketExpression(string bracketString, char bracketType, char bracketPair,
            int stringPos)
        {
            var bracketStack = new Stack<bool>();
            for (int j = stringPos; j < bracketString.Length; j++)
            {
                if (bracketString[j] == bracketType) bracketStack.Push(true);
                if (bracketString[j] != bracketPair) continue;
                if (!bracketStack.Any()) throw new BracketException("Missing open bracket");
                bracketStack.Pop();
            }

            if (bracketStack.Any()) throw new BracketException("Missing close bracket");
        }

        private (int, int) FindAndValidateProgramBlock(int startPosition)
        {
            for (int i = startPosition; i < commandSequence.Length; i++)
            {
                var scriptString = commandSequence[i].RemoveSplitters();
                if (scriptString == "") continue;
                if (!scriptString.StartsWith("Program")) continue;
                var programBlockEdges = (i, 0);
                var programBlock = ExtractProgramBlock(i, 6);
                programBlockEdges.Item2 = MakeSequenceShift(i,
                    commandSequence[i].IndexOf("Program", StringComparison.CurrentCultureIgnoreCase) + 7,
                    programBlock.Length);
                return programBlockEdges;
            }

            throw new ProgramBlockException("Program block undefined or defined incorrectly");
        }

        private string ExtractProgramBlock(int startString, int startCharPosition)
        {
            var bracketStack = new Stack<bool>();
            var charEnumerable = commandSequence.Skip(startString).SelectMany(value => value.RemoveSplitters())
                .Skip(startCharPosition + 1);
            var isFirstBracket = false;
            var programBlock = new StringBuilder();
            foreach (var symbol in charEnumerable)
            {
                switch (isFirstBracket)
                {
                    case false when symbol != '{':
                        throw new BracketException("Program block missing bracket");
                    case false when symbol == '{':
                        isFirstBracket = true;
                        bracketStack.Push(true);
                        break;
                    default:
                    {
                        if (symbol == '{')
                        {
                            bracketStack.Push(true);
                        }

                        break;
                    }
                }

                switch (symbol)
                {
                    case '}' when !bracketStack.Any():
                        throw new BracketException("Missing open bracket");
                    case '}':
                        bracketStack.Pop();
                        break;
                }

                programBlock.Append(symbol);
            }

            if (bracketStack.Any()) throw new BracketException("Too many open brackets");
            return programBlock.ToString();
        }

        private double Evaluate(string expression)
        {
            var preparedExpression = ChangeVariables(expression);
            return preparedExpression.EvalNumerical().RealPart.AsDouble();
        }

        private string ChangeVariables(string rawExpression)
        {
            foreach (var key in intVariablesDictionary.Keys)
            {
                var entry = 0;
                while (entry != -1)
                {
                    entry = rawExpression.IndexOf(key, 0, StringComparison.InvariantCulture);
                    if (entry != -1)
                    {
                        rawExpression = rawExpression.Replace(key, intVariablesDictionary[key].ToString());
                    }
                }
            }

            foreach (var key in doubleVariablesDictionary.Keys)
            {
                var entry = 0;
                while (entry != -1)
                {
                    entry = rawExpression.IndexOf(key, 0, StringComparison.InvariantCulture);
                    if (entry != -1)
                    {
                        rawExpression = rawExpression.Replace(key, doubleVariablesDictionary[key].ToString());
                    }
                }
            }

            return rawExpression;
        }

        private int ParseForCommand(string rawString)
        {
            var openBracketIndex = rawString.IndexOf('(');
            var closeBracketIndex = rawString.IndexOf(')');
            var iterationsCount = int.Parse(rawString.Substring(openBracketIndex + 1, closeBracketIndex - openBracketIndex - 1));
            if (iterationsCount <= 0) throw new InterpretException("Cycle argument should be greater then 0");
            return iterationsCount;
        }

        private void InitializeFor(string currentCommand)
        {
            var finderPointer = commandPointerPosition;
            var argument = ParseForCommand(currentCommand);
            var bracketParseStack = new Stack<int>();
            for (int i = finderPointer; i < extractedCommands.Length; i++)
            {
                if (extractedCommands[finderPointer].Contains('{'))
                {
                    bracketParseStack.Push(finderPointer);
                }

                if (!extractedCommands[finderPointer].Contains('}')) continue;
                if (bracketParseStack.Count == 1)
                {

                    brackets[bracketParseStack.Peek()] = (finderPointer, argument);
                    brackets[finderPointer] = (bracketParseStack.Peek(), argument);
                    break;
                }

                bracketParseStack.Pop();
            }
        }

        private void ParseMainCommand(string currentCommand)
        {

        }

        //private void ValidateAndShift(string command)
        //{
        //    if (command.Contains('}') && brackets.ContainsKey('}'))
        //        commandPointerPosition = brackets[commandPointerPosition];
        //}
    }
}
