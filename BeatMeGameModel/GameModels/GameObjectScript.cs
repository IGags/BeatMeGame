using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
    public enum AccessLevel
    {
        ObjectScript,
        MainScript
    }
    public class GameObjectScript
    {

        public GameObject TargetObject { get; private set; }
        public bool IsEnded { get; private set; }
        public AccessLevel ScriptAccessLevel { get; private set; }

        [Variable] private readonly Dictionary<string, int> intVariablesDictionary = new Dictionary<string, int>();

        [Variable] private readonly Dictionary<string, double> doubleVariablesDictionary =
            new Dictionary<string, double>();

        private GameObjectScript invoker;
        private Game targetGame;
        private Dictionary<int, (int, int)> brackets = new Dictionary<int, (int, int)>();
        private readonly int variableTypeCount = 2;
        private double currentCommandPerformancePercent;
        private TimeSpan startTime;
        private TimeSpan executionTime;
        private int commandPointerPosition;
        private readonly string[] commandSequence;
        private string[] extractedCommands;
        private bool isStarted;

        public GameObjectScript(string[] commandSequence)
        {
            this.commandSequence = commandSequence;
            Initialize(commandSequence);
        }

        private GameObjectScript(GameObjectScript invoker, Game targetGame, Dictionary<int, (int, int)> brackets,
            double currentCommandPerformancePercent, TimeSpan startTime, TimeSpan executionTime,
            int commandPointerPosition, string[] commandSequence, string[] extractedCommands, bool isStarted,
            Dictionary<string, int> intVariablesDictionary, Dictionary<string, double> doubleVariablesDictionary,
            AccessLevel scriptAccessLevel, GameObject targetObject)
        {
            this.invoker = invoker;
            this.targetGame = targetGame;
            this.brackets = brackets.ToDictionary(key => key.Key, value => value.Value);
            this.currentCommandPerformancePercent = currentCommandPerformancePercent;
            this.startTime = startTime;
            this.executionTime = executionTime;
            this.commandPointerPosition = commandPointerPosition;
            this.commandSequence = commandSequence;
            this.extractedCommands = extractedCommands;
            this.isStarted = isStarted;
            this.intVariablesDictionary = intVariablesDictionary.ToDictionary(key => key.Key, value => value.Value);
            this.doubleVariablesDictionary = doubleVariablesDictionary.ToDictionary(key => key.Key, value => value.Value);
            this.ScriptAccessLevel = scriptAccessLevel;
            this.TargetObject = targetObject;
        }

        public GameObjectScript Copy()
        {
            return new GameObjectScript(invoker, targetGame, brackets, currentCommandPerformancePercent, startTime,
                executionTime, commandPointerPosition, commandSequence, extractedCommands, isStarted,
                intVariablesDictionary, doubleVariablesDictionary, ScriptAccessLevel, TargetObject);
        }

        public void Start(TimeSpan time, Game targetGame, GameObject targetObject = null, GameObjectScript invoker = null)
        {
            if (ScriptAccessLevel == AccessLevel.ObjectScript && targetObject == null)
                throw new ArgumentException("Invalid Object Initialization");
            this.invoker = invoker;
            this.targetGame = targetGame;
            startTime = time;
            isStarted = true;
            TargetObject = targetObject;
            targetObject?.IncreaseReferenceCount();
        }

        public void Interrupt()
        {
            isStarted = false;
        }

        public void Resume()
        {
            if (isStarted) return;
            if (TargetObject == null && ScriptAccessLevel == AccessLevel.ObjectScript)
                throw new ArgumentException("CannotResume");
            isStarted = true;
        }

        public void Resume(TimeSpan time)
        {
            if (isStarted) return;
            if (TargetObject == null && ScriptAccessLevel == AccessLevel.ObjectScript)
                throw new ArgumentException("CannotResume");
            executionTime = time;
            isStarted = true;
        }

        public void Interpret(TimeSpan time)
        {
            if (time < executionTime) throw new InterpretException("queryTimeLessThanInterpretTime");
            while (executionTime < time)
            {
                if (!isStarted)
                {
                    executionTime = time;
                    return;
                }
                if (commandPointerPosition >= extractedCommands.Length)
                {
                    invoker?.Resume(executionTime);
                    IsEnded = true;
                    TargetObject.DecreaseReferenceCount();
                    return;
                }

                var currentCommand = extractedCommands[commandPointerPosition].RemoveSplitters()
                    .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[0];

                if (currentCommand.Contains('='))
                {
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
                            brackets.Remove(commandPointerPosition - 1);
                            continue;
                        }
                    }
                    else InitializeFor(currentCommand);

                    if (!currentCommand.Contains('{'))
                    {
                        commandPointerPosition++;
                        continue;
                        
                    }
                }
                if (currentCommand.Contains('{')) currentCommand = currentCommand.Split('{')[1];
                if (ScriptAccessLevel == AccessLevel.MainScript) ParseMainCommand(currentCommand, time);
                else ParseObjectCommand(currentCommand, time);
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
                        ScriptAccessLevel = AccessLevel.ObjectScript;
                        return i;
                    case "[Main]":
                        ScriptAccessLevel = AccessLevel.MainScript;
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
            rawExpression = ReplaceAllIntersections(rawExpression, "player1X", targetGame.Player1.X);
            rawExpression = ReplaceAllIntersections(rawExpression, "player1Y", targetGame.Player1.Y);
            if (targetGame.Player2 != null)
            {
                rawExpression = ReplaceAllIntersections(rawExpression, "player2Y", targetGame.Player2.Y);
                rawExpression = ReplaceAllIntersections(rawExpression, "player2X", targetGame.Player2.X);
            }
            if (ScriptAccessLevel == AccessLevel.ObjectScript)
            {
                rawExpression = ReplaceAllIntersections(rawExpression, "X", TargetObject.XPosition);
                rawExpression = ReplaceAllIntersections(rawExpression, "Y", TargetObject.YPosition);
                rawExpression = ReplaceAllIntersections(rawExpression, "Angle", TargetObject.Angle);
            }
            foreach (var key in intVariablesDictionary.Keys)
            {
                rawExpression = ReplaceAllIntersections(rawExpression, key, intVariablesDictionary[key]);
            }
            foreach (var key in doubleVariablesDictionary.Keys)
            {
                rawExpression = ReplaceAllIntersections(rawExpression, key, intVariablesDictionary[key]);
            }
            return rawExpression;
        }

        private string ReplaceAllIntersections(string rawString, string entry, double toReplace)
        {
            var indexEntry = rawString.IndexOf(entry, 0, StringComparison.InvariantCulture);
            while (indexEntry != -1)
            {
                rawString = rawString.Replace(entry, toReplace.ToString());
                indexEntry = rawString.IndexOf(entry, 0, StringComparison.InvariantCulture);
            }

            return rawString;
        }

        private int ParseForCommand(string rawString)
        {
            var openBracketIndex = rawString.IndexOf('(');
            var closeBracketIndex = rawString.IndexOf(')');
            var iterationsCount = int.Parse(rawString.Substring(openBracketIndex + 1, closeBracketIndex - openBracketIndex - 1));
            if (iterationsCount <= 0) throw new InterpretException("Cycle argument should be greater than 0");
            return iterationsCount;
        }

        private void InitializeFor(string currentCommand)
        {
            var finderPointer = commandPointerPosition;
            var argument = ParseForCommand(currentCommand);
            var bracketParseStack = new Stack<int>();
            for (int i = finderPointer; i < extractedCommands.Length; i++)
            {
                if (extractedCommands[i].Contains('{'))
                {
                    bracketParseStack.Push(finderPointer);
                }

                if (!extractedCommands[i].Contains('}')) continue;
                if (bracketParseStack.Count == 1)
                {

                    brackets[finderPointer] = (i, argument);
                    brackets[i] = (finderPointer, argument);
                    break;
                }

                bracketParseStack.Pop();
            }
        }

        private void ParseMainCommand(string currentCommand, TimeSpan time)
        {
            var arguments = ParseCommandArguments(currentCommand);
            if (currentCommand.StartsWith("spawn"))
            {
                if (arguments.Length != 8) throw new InvalidEnumArgumentException("Invalid arguments");
                var doubleArguments = arguments.Take(5).ToArray();
                var preparedArguments = new double[5];
                for (int i = 0; i < preparedArguments.Length; i++)
                    preparedArguments[i] = ParseDoubleArgument(doubleArguments[i]);
                targetGame.Spawn(preparedArguments[0], preparedArguments[1], preparedArguments[2], preparedArguments[3], preparedArguments[4],
                    arguments[5], arguments[6], arguments[7].ParseTag());
            }
            else if (currentCommand.StartsWith("delay"))
            {
                if(arguments.Length != 1) throw new InvalidEnumArgumentException("Invalid arguments");
                var doubleArgument = ParseDoubleArgument(arguments[0]);
                var deltaTime = (time - executionTime).TotalSeconds;
                var remainingTime = (1 - currentCommandPerformancePercent) * doubleArgument;
                if (remainingTime > deltaTime)
                {
                    currentCommandPerformancePercent += deltaTime / doubleArgument;
                    executionTime = time;
                }

                if (Math.Abs(remainingTime - deltaTime) < 10e-5)
                {
                    currentCommandPerformancePercent = 0;
                    executionTime = time;
                }

                if (remainingTime < deltaTime)
                {
                    executionTime += new TimeSpan(0, 0, 0, 0,
                        (int)((1 - currentCommandPerformancePercent) * doubleArgument * 1000));
                    currentCommandPerformancePercent = 0;
                }
            }
            else if(currentCommand.StartsWith("execute"))
            {
                if(arguments.Length != 1) throw new InvalidEnumArgumentException("Invalid arguments");
                targetGame.InvokeGameObjectScript(arguments[0], this, TargetObject);
                Interrupt();
            }
            else if (currentCommand.StartsWith("asyncExecute"))
            {
                if (arguments.Length != 1) throw new InvalidEnumArgumentException("Invalid arguments");
                targetGame.InvokeGameObjectScript(arguments[0], null, TargetObject);
            }
            else if (currentCommand == "" || currentCommand == "}") { }
            else
            {
                throw new InvalidCommandException();
            }

            if (currentCommandPerformancePercent != 0) return;
            if (currentCommand.Contains('}') && brackets.ContainsKey(commandPointerPosition))
            {
                brackets[commandPointerPosition] = (brackets[commandPointerPosition].Item1,
                    brackets[commandPointerPosition].Item2 - 1);
                commandPointerPosition = brackets[commandPointerPosition].Item1;
                brackets[commandPointerPosition] = (brackets[commandPointerPosition].Item1,
                    brackets[commandPointerPosition].Item2 - 1);
                return;
            }
            commandPointerPosition++;
        }

        private string[] ParseCommandArguments(string command)
        {
            var brackets = FindPairBrackets(command);
            if (brackets.Item1 == -1) return new string[0];
            var arguments = command.Substring(brackets.Item1 + 1, brackets.Item2 - brackets.Item1 - 1);
            return arguments.Split(',');
        }

        private (int, int) FindPairBrackets(string command)
        {
            var bracketStack = new Stack<int>();
            for (int i = 0; i < command.Length; i++)
            {
                if(command[i] == '(') bracketStack.Push(i);
                if (command[i] == ')')
                {
                    if (bracketStack.Count == 1) return (bracketStack.Pop(), i);
                    bracketStack.Pop();
                }
            }

            if (bracketStack.Count == 0) return (-1, -1);
            throw new BracketException("Incorrect bracket expression");
        }

        private double ParseDoubleArgument(string argument)
        {
            if (doubleVariablesDictionary.ContainsKey(argument))
                return doubleVariablesDictionary[argument];
            if (intVariablesDictionary.ContainsKey(argument))
                return intVariablesDictionary[argument];
            if (double.TryParse(argument, out var output))
                return output;
            return Evaluate(argument);
        }

        private void ParseObjectCommand(string currentCommand, TimeSpan time)
        {
            var arguments = ParseCommandArguments(currentCommand);
            if (currentCommand.StartsWith("move"))
            {
                if(arguments.Length != 3) throw new InvalidEnumArgumentException("Invalid arguments");
                var xEquation = ChangeVariables(arguments[0]);
                var yEquation = ChangeVariables(arguments[1]);
                var doubleArgument = ParseDoubleArgument(arguments[2]);
                var deltaTime = (time - executionTime).TotalSeconds;
                var remainingTime = (1 - currentCommandPerformancePercent) * doubleArgument;
                var workTime = (executionTime - startTime).TotalSeconds;
                if (xEquation.Contains('t') && yEquation.Contains('t'))
                {
                    if (remainingTime > deltaTime)
                    {
                        var endTime = (time - startTime).TotalSeconds;
                        TargetObject.XPosition += ReplaceTimeAndEvaluate(xEquation, endTime) -
                                                 ReplaceTimeAndEvaluate(xEquation, workTime);
                        TargetObject.YPosition += ReplaceTimeAndEvaluate(yEquation, endTime) -
                                                 ReplaceTimeAndEvaluate(yEquation, workTime);
                        currentCommandPerformancePercent += deltaTime / doubleArgument;
                        executionTime = time;
                    }

                    if (Math.Abs(remainingTime - deltaTime) < 10e-5)
                    {
                        var remainingPercent = 1 - currentCommandPerformancePercent;
                        var endTime = workTime + remainingPercent * doubleArgument;
                        TargetObject.XPosition += ReplaceTimeAndEvaluate(xEquation, endTime) - 
                                                 ReplaceTimeAndEvaluate(xEquation, workTime);
                        TargetObject.YPosition += ReplaceTimeAndEvaluate(yEquation, endTime) -
                                                 ReplaceTimeAndEvaluate(yEquation, workTime);
                        currentCommandPerformancePercent = 0;
                        executionTime = time;
                    }

                    if (remainingTime < deltaTime)
                    {
                        var remainingPercent = 1 - currentCommandPerformancePercent;
                        var endTime = workTime + remainingPercent * doubleArgument;
                        TargetObject.XPosition += ReplaceTimeAndEvaluate(xEquation, endTime) - 
                                                 ReplaceTimeAndEvaluate(xEquation, workTime);
                        TargetObject.YPosition += ReplaceTimeAndEvaluate(yEquation, endTime) -
                                                 ReplaceTimeAndEvaluate(yEquation, workTime);
                        executionTime += new TimeSpan(0, 0, 0, 0,
                            (int)((1 - currentCommandPerformancePercent) * doubleArgument * 1000));
                        currentCommandPerformancePercent = 0;
                    }
                }
                else if (!xEquation.Contains('t') && !yEquation.Contains('t'))
                {
                    var xRangeArgument = Evaluate(xEquation);
                    var yRangeArgument = Evaluate(yEquation);
                    if (remainingTime > deltaTime)
                    {
                        var percentChange = deltaTime / doubleArgument;
                        TargetObject.XPosition += percentChange * xRangeArgument;
                        TargetObject.YPosition += percentChange * yRangeArgument;
                        currentCommandPerformancePercent += percentChange;
                        executionTime = time;
                    }

                    if (Math.Abs(remainingTime - deltaTime) < 10e-5)
                    {
                        var percentChange = 1 - currentCommandPerformancePercent;
                        TargetObject.XPosition += percentChange * xRangeArgument;
                        TargetObject.YPosition += percentChange * yRangeArgument;
                        currentCommandPerformancePercent = 0;
                        executionTime = time;
                    }

                    if (remainingTime < deltaTime)
                    {
                        var percentChange = 1 - currentCommandPerformancePercent;
                        TargetObject.XPosition += percentChange * xRangeArgument;
                        TargetObject.YPosition += percentChange * yRangeArgument;
                        executionTime += new TimeSpan(0, 0, 0, 0,
                            (int)((1 - currentCommandPerformancePercent) * doubleArgument * 1000));
                        currentCommandPerformancePercent = 0;
                    }
                }
                else throw new InvalidCommandException();
            }
            else if (currentCommand.StartsWith("spawn"))
            {
                if (arguments.Length != 8) throw new InvalidEnumArgumentException("Invalid arguments");
                var doubleArguments = arguments.Take(5).ToArray();
                var preparedArguments = new double[5];
                for (int i = 0; i < preparedArguments.Length; i++)
                    preparedArguments[i] = ParseDoubleArgument(doubleArguments[i]);
                targetGame.Spawn(preparedArguments[0], preparedArguments[1], preparedArguments[2], preparedArguments[3], preparedArguments[4],
                    arguments[5], arguments[6], arguments[7].ParseTag());
            }
            else if (currentCommand.StartsWith("execute"))
            {
                if (arguments.Length != 1) throw new InvalidEnumArgumentException("Invalid arguments");
                targetGame.InvokeGameObjectScript(arguments[0], this, TargetObject);
                Interrupt();
            }
            else if (currentCommand.StartsWith("asyncExecute"))
            {
                if (arguments.Length != 1) throw new InvalidEnumArgumentException("Invalid arguments");
                targetGame.InvokeGameObjectScript(arguments[0], null, TargetObject);
            }
            else if (currentCommand.StartsWith("delay"))
            {
                if (arguments.Length != 1) throw new InvalidEnumArgumentException("Invalid arguments");
                var doubleArgument = ParseDoubleArgument(arguments[0]);
                var deltaTime = (time - executionTime).TotalSeconds;
                var remainingTime = (1 - currentCommandPerformancePercent) * doubleArgument;
                if (remainingTime > deltaTime)
                {
                    currentCommandPerformancePercent += deltaTime / doubleArgument;
                    executionTime = time;
                }

                if (Math.Abs(remainingTime - deltaTime) < 10e-5)
                {
                    currentCommandPerformancePercent = 0;
                    executionTime = time;
                }

                if (remainingTime < deltaTime)
                {
                    executionTime += new TimeSpan(0, 0, 0, 0,
                        (int)((1 - currentCommandPerformancePercent) * doubleArgument * 1000));
                    currentCommandPerformancePercent = 0;
                }
            }
            else if(currentCommand.StartsWith("rotate"))
            {
                if(arguments.Length != 2) throw new InvalidEnumArgumentException("Invalid arguments");
                var doubleArgument = ParseDoubleArgument(arguments[1]);
                var rotationArgument = ParseDoubleArgument(arguments[0]);
                var deltaTime = (time - executionTime).TotalSeconds;
                var remainingTime = (1 - currentCommandPerformancePercent) * doubleArgument;
                if (remainingTime > deltaTime)
                {
                    var percentChange = deltaTime / doubleArgument;
                    TargetObject.Angle += percentChange * rotationArgument;
                    currentCommandPerformancePercent += percentChange;
                    executionTime = time;
                }

                if (Math.Abs(remainingTime - deltaTime) < 10e-5)
                {
                    var percentChange = 1 - currentCommandPerformancePercent;
                    TargetObject.Angle += percentChange * rotationArgument;
                    currentCommandPerformancePercent = 0;
                    executionTime = time;
                }

                if (remainingTime < deltaTime)
                {
                    var percentChange = 1 - currentCommandPerformancePercent;
                    TargetObject.Angle += percentChange * rotationArgument;
                    executionTime += new TimeSpan(0, 0, 0, 0,
                        (int)((1 - currentCommandPerformancePercent) * doubleArgument * 1000));
                    currentCommandPerformancePercent = 0;
                }
            }
            else if(currentCommand.StartsWith("scale"))
            {
                if(arguments.Length != 3) throw new InvalidEnumArgumentException("Invalid arguments");
                var timeArgument = ParseDoubleArgument(arguments[2]);
                var scaleX = ParseDoubleArgument(arguments[0]);
                var scaleY = ParseDoubleArgument(arguments[1]);
                var deltaTime = (time - executionTime).TotalSeconds;
                var remainingTime = (1 - currentCommandPerformancePercent) * timeArgument;
                if (remainingTime > deltaTime)
                {
                    var percentChange = deltaTime / timeArgument;
                    TargetObject.XSize += percentChange * scaleX;
                    TargetObject.XSize += percentChange * scaleY;
                    currentCommandPerformancePercent += percentChange;
                    executionTime = time;
                }

                if (Math.Abs(remainingTime - deltaTime) < 10e-5)
                {
                    var percentChange = 1 - currentCommandPerformancePercent;
                    TargetObject.XSize += percentChange * scaleX;
                    TargetObject.XSize += percentChange * scaleY;
                    currentCommandPerformancePercent = 0;
                    executionTime = time;
                }

                if (remainingTime < deltaTime)
                {
                    var percentChange = 1 - currentCommandPerformancePercent;
                    TargetObject.XSize += percentChange * scaleX;
                    TargetObject.XSize += percentChange * scaleY;
                    executionTime += new TimeSpan(0, 0, 0, 0,
                        (int)((1 - currentCommandPerformancePercent) * timeArgument * 1000));
                    currentCommandPerformancePercent = 0;
                }
            }
            else if (currentCommand == "") { }
            else
            {
                throw new InvalidCommandException();
            }

            if (currentCommandPerformancePercent != 0) return;
            if (currentCommand.Contains('}') && brackets.ContainsKey(commandPointerPosition))
            {
                brackets[commandPointerPosition] = (brackets[commandPointerPosition].Item1,
                    brackets[commandPointerPosition].Item2 - 1);
                commandPointerPosition = brackets[commandPointerPosition].Item1;
                brackets[commandPointerPosition] = (brackets[commandPointerPosition].Item1,
                    brackets[commandPointerPosition].Item2 - 1);
                return;
            }
            commandPointerPosition++;
        }

        private double ReplaceTimeAndEvaluate(string equation, double time)
        {
            return equation.Replace("t", time.ToString(CultureInfo.InvariantCulture))
                .EvalNumerical().RealPart.AsDouble();
        }
    }
}
