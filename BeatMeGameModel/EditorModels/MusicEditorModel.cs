using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoundEngineLibrary;
using BeatMeGameModel.BeatVertexes;
using BeatMeGameModel.IOWorkers;

namespace BeatMeGameModel.EditorModels
{
    public enum PackingDirection
    {
        Forward,
        Backward
    }
    public class MusicEditorModel //TODO: подключить плейтест музыки
    {
        public SoundEngineTread WorkTread { get; }
        public LevelSave Save { get; }
        public int CurrentSecond { get; private set; }
        public int FramesPerSecond { get; }
        public BeatVertex[] Vertices { get; private set; }
        private BeatVertex CurrentVertex { get; set; } = new BeatVertex(TimeSpan.Zero, VertexType.None);
        private BeatVertex lastVertexPerFrame = new BeatVertex(TimeSpan.Zero, VertexType.None);
        private readonly Stack<BeatVertex> previousVertexStack = new Stack<BeatVertex>();
        private readonly SpectrogramModel spectrogramModel;
        private Dictionary<TimeSpan, BeatVertex> alternativeType = new Dictionary<TimeSpan, BeatVertex>();
        private BeatEngine engine;
        public MusicEditorModel(SoundEngineTread tread, LevelSave save)
        {
            WorkTread = tread;
            Save = save;
            FramesPerSecond = WorkTread.TrackFFT.samplingFrequency / FFT.FFTSize;
            CurrentSecond = save.Manifest.StartSecond;
            spectrogramModel = new SpectrogramModel(WorkTread.TrackFFT.LowEdge, WorkTread.TrackFFT.HighEdge,
                WorkTread.TrackFFT.samplingFrequency);
        }

        public void StartPlayTest(Action onBeatAction, Action clearAction, Action shutdownAction, bool isSecondTest)
        {
            if(engine != null) return;
            PackVertices(Vertices, PackingDirection.Backward);
            engine = new BeatEngine(WorkTread, Save.Beat.ToDictionary(value => value.Key, value => value.Value), 
                Save.Manifest.DetectionType,
                new TimeSpan(0, 0, 0, CurrentSecond));
            UnpackVertices(CurrentSecond, PackingDirection.Forward);
            engine.OnBeat += onBeatAction;
            engine.Clear += clearAction;
            engine.Shutdown += shutdownAction;
            engine.Play(isSecondTest);
            WorkTread.ChangePlayingPosition(CurrentSecond);
            WorkTread.ChangePlaybackState();
        }

        public void StopPlayTest()
        {
            if(engine == null) return;
            engine.Pause();
            WorkTread.OutputDevice.Pause();
            engine = null;
            WorkTread.ChangePlayingPosition(CurrentSecond);
            GC.Collect();
        }

        public List<List<double>> GetSpectrogram(int lowFrequency, int highFrequency)
        {
            spectrogramModel.HighFrequency = highFrequency;
            spectrogramModel.LowFrequency = lowFrequency;
            return spectrogramModel.GetNormalizedSpectrogramMap(
                WorkTread.TrackFFT.GetFFTSecond(new TimeSpan(0, 0, CurrentSecond)));
        }

        public void UnpackVertices(int second, PackingDirection direction)
        {
            var currentActions = new List<(TimeSpan, BeatVertex)>();
            var keysToRemove = new List<TimeSpan>();
            foreach (var key in Save.Beat.Keys)
            {
                if ((int)key.TotalSeconds == second)
                {
                    currentActions.Add((key, Save.Beat[key]));
                    keysToRemove.Add(key);
                }
            }

            foreach (var key in keysToRemove)
                Save.Beat.Remove(key);

            currentActions = currentActions.OrderBy(value => value.Item1).ToList();
            var ms = currentActions.Select(value =>
            {
                var millisecond = value.Item1.Milliseconds;
                return (Millisecond2Position(millisecond), value.Item2);
            }).ToDictionary(value => value.Item1, value => value.Item2);
            Vertices = UnpackChainVertices(ms, direction);
        }

        public TimeSpan GetTimeLimit()
        {
            return WorkTread?.MaxSongDuration ?? TimeSpan.Zero;
        }

        public void ChangeStartTime(int newTime)
        {
            PackVertices(Vertices, PackingDirection.Backward);
            CurrentVertex = new BeatVertex(TimeSpan.MinValue, VertexType.None);
            Save.Manifest.StartSecond = newTime;
            CurrentSecond = newTime;
            var beatToDeletion = Save.Beat.Keys
                .Where(time => time.TotalSeconds < Save.Manifest.StartSecond)
                .ToList();
            foreach (var beat in beatToDeletion)
                Save.Beat.Remove(beat);
            previousVertexStack.Clear();
            UnpackVertices(newTime, PackingDirection.Forward);
        }

        public int Millisecond2Position(int time)
        {
            return (int)Math.Round((time * FramesPerSecond) / 1000d);
        }

        public int Position2Millisecond(int position)
        {
            return (position * 1000) / FramesPerSecond;
        }

        public double BPM2Millisecond(double bpm)
        {
            if(bpm == 0) return Double.PositiveInfinity;
            return (int)(1000 / (bpm / 60));
        }

        private BeatVertex[] UnpackChainVertices(Dictionary<int, BeatVertex> vertexList, PackingDirection direction)
        {
            var typeArray = new BeatVertex[FramesPerSecond];
            var (type, chainType) = GetChainVertexTypes();
            if (Save.Manifest.DetectionType == BeatDetectionType.FFT)
            {
                type = VertexType.FFT;
                chainType = VertexType.AdditionalFFT;
            }
            if (direction == PackingDirection.Backward)
            {
                if (previousVertexStack.Count != 0)
                {
                    if ((int)previousVertexStack.Peek().Time.TotalSeconds != CurrentSecond || previousVertexStack.Peek().Time.Milliseconds == 0)
                        CurrentVertex = previousVertexStack.Pop();
                    else
                    {
                        previousVertexStack.Pop();
                        CurrentVertex = previousVertexStack.Count == 0 ? new BeatVertex(TimeSpan.Zero, VertexType.None) : previousVertexStack.Peek();
                    }
                }
                else CurrentVertex = new BeatVertex(TimeSpan.Zero, VertexType.None);
            }
            var beatArray = GetUpdatedBeat();
            for (int i = 0; i < typeArray.Length; i++)
            {
                if (beatArray.Length <= i)
                    typeArray[i] = new BeatVertex(new TimeSpan(0, 0, 0, CurrentSecond, Position2Millisecond(i)),
                        VertexType.None);
                else
                {
                    if (vertexList.ContainsKey(i))
                    {
                        typeArray[i] = vertexList[i];
                        if (vertexList[i].Type != type) continue;
                        lastVertexPerFrame = CurrentVertex = vertexList[i];
                        beatArray = GetUpdatedBeat();
                    }

                    else if (beatArray[i])
                        typeArray[i] = new BeatVertex(new TimeSpan(0, 0, 0, CurrentSecond, Position2Millisecond(i)),
                            chainType);
                    else
                    {
                        typeArray[i] = new BeatVertex(new TimeSpan(0, 0, 0, CurrentSecond, Position2Millisecond(i)),
                            VertexType.None);
                    }
                }
            }
            return typeArray;
        }

        public void ChangeAnalyzeType()
        {
            PackVertices(Vertices, PackingDirection.Forward);
            (alternativeType, Save.Beat) = (Save.Beat, alternativeType);
            previousVertexStack.Clear();
            CurrentSecond = Save.Manifest.StartSecond;
            CurrentVertex = lastVertexPerFrame = new BeatVertex(TimeSpan.Zero, VertexType.None);
            Save.Manifest.DetectionType = Save.Manifest.DetectionType == BeatDetectionType.FFT 
                ? BeatDetectionType.BPM 
                : BeatDetectionType.FFT;
            UnpackVertices(CurrentSecond, PackingDirection.Forward);
        }

        public void PackVertices(BeatVertex[] vertices, PackingDirection direction)
        {
            var (type, chainType) = GetChainVertexTypes();
            foreach (var vertex in vertices)
            {
                if (vertex.Type == VertexType.None || vertex.Type == chainType) continue;
                Save.Beat[vertex.Time] = vertex;
                if (vertex.Type == type) CurrentVertex = vertex;
            }
            lastVertexPerFrame = CurrentVertex;
            if(direction == PackingDirection.Forward)previousVertexStack.Push(lastVertexPerFrame);
        }

        public void AddChainVertex(int position, BeatVertex updatedVertex)
        {
            CurrentVertex = updatedVertex;
            var beat = GetUpdatedBeat();
            var (type, chainType) = GetChainVertexTypes();
            if (position != -1) Vertices[position] = updatedVertex;
            for (int i = position + 1; i < FramesPerSecond; i++)
            {
                if(Vertices[i].Type == type) return;
                if (beat.Length <= i)
                    Vertices[i] = new BeatVertex(new TimeSpan(0, 0, 0, CurrentSecond, Position2Millisecond(i)),
                        VertexType.None);
                else if(Vertices[i].Type == VertexType.Artificial) continue;
                else if (beat[i])
                {
                    if(Vertices[i].Type == chainType) continue;
                    Vertices[i] = new BeatVertex(new TimeSpan(0, 0, 0, CurrentSecond, Position2Millisecond(i)),
                        chainType);
                }
                else
                {
                    Vertices[i] = new BeatVertex(new TimeSpan(0, 0, 0, CurrentSecond, Position2Millisecond(position)),
                        VertexType.None);
                }
            }
            ClearDeletionVertexes();
        }

        public void AddMonoVertex(int position, BeatVertex vertex)
        {
            var beat = GetUpdatedBeat();
            if (position < beat.Length)
            {
                Vertices[position] = vertex;
            }

        }

        public void DeleteChainVertex(int position)
        {
            var(type, chainType) = GetChainVertexTypes();
            var prevFFTPos = -1;
            Vertices[position] = new BeatVertex(new TimeSpan(0, 0, 0, CurrentSecond, Position2Millisecond(position)),
                VertexType.None);
            for (int i = position - 1; i >= 0; i--)
            {
                if (Vertices[i].Type != type) continue;
                prevFFTPos = i;
                break;
            }

            if (prevFFTPos == -1)
            {
                if (!previousVertexStack.Any())
                {
                    for (int i = 0; i < Vertices.Length; i++)
                    {
                        if (Vertices[i].Type == type)
                        {
                            CurrentVertex = Vertices[i];
                            break;
                        }
                        switch (Vertices[i].Type)
                        {
                            case VertexType.Artificial:
                                break;
                            default:
                                Vertices[i] = new BeatVertex(new TimeSpan(0, 0, 0, CurrentSecond, Position2Millisecond(position)),
                                    VertexType.None);
                                break;
                        }

                        CurrentVertex = new BeatVertex(TimeSpan.Zero, VertexType.None);
                        if(i + 1 == Vertices.Length) ClearDeletionVertexes();
                    }
                }
                else AddChainVertex(-1, previousVertexStack.Peek());
            }
            else AddChainVertex(prevFFTPos, Vertices[prevFFTPos]);
        }

        public void GetNextSecond()
        {
            if((int)WorkTread.MaxSongDuration.TotalSeconds == CurrentSecond) return;
            CurrentSecond++;
            PackVertices(Vertices, PackingDirection.Forward);
            UnpackVertices(CurrentSecond, PackingDirection.Forward);
        }

        public void GetPreviousSecond()
        {
            if(CurrentSecond == Save.Manifest.StartSecond) return;
            CurrentSecond--;
            PackVertices(Vertices, PackingDirection.Backward);
            UnpackVertices(CurrentSecond, PackingDirection.Backward);
        }

        public void GetSecondByTime(int time)
        {
            if(time == CurrentSecond) return;
            if (time > CurrentSecond)
            {
                while (CurrentSecond != time && CurrentSecond < (int)WorkTread.MaxSongDuration.TotalSeconds)
                {
                    GetNextSecond();
                }
            }

            else
            {
                CurrentSecond = 0;
                CurrentVertex = new BeatVertex(TimeSpan.MinValue, VertexType.None);
                PackVertices(Vertices, PackingDirection.Backward);
                previousVertexStack.Clear();
                UnpackVertices(CurrentSecond, PackingDirection.Forward);
                while (CurrentSecond != time)
                {
                    GetNextSecond();
                }
            }
        }

        public void SaveModel()
        {
            PackVertices(Vertices, PackingDirection.Backward);
            LevelSavePacker.PackSave(Save);
            UnpackVertices(CurrentSecond, PackingDirection.Forward);
        }

        private void ClearDeletionVertexes()
        {
            var maxVertex = new BeatVertex(TimeSpan.MaxValue, VertexType.FFT);
            var deletionList = new List<TimeSpan>();
            foreach (var key in Save.Beat.Keys)
            {
                if ((Save.Beat[key].Type == VertexType.FFT
                     || Save.Beat[key].Type == VertexType.BPM)
                    && key < maxVertex.Time
                    && key > new TimeSpan(0, 0, CurrentSecond + 1))
                {
                    maxVertex = Save.Beat[key];
                    var keyToDelete =
                        deletionList.Where(deletionKey => deletionKey > maxVertex.Time).ToList();
                    foreach (var entity in keyToDelete)
                        deletionList.Remove(entity);
                }
                else if (Save.Beat[key].Type == VertexType.Deletion
                         && key < maxVertex.Time
                         && key > new TimeSpan(0, 0, CurrentSecond + 1))
                {
                    deletionList.Add(key);
                }
            }

            foreach (var key in deletionList)
            {
                Save.Beat.Remove(key);
            }
        }

        private (VertexType, VertexType) GetChainVertexTypes()
        {
            return Save.Manifest.DetectionType == BeatDetectionType.FFT
                ? (VertexType.FFT, VertexType.AdditionalFFT)
                : (VertexType.BPM, VertexType.AdditionalBPM);
        }

        private bool[] GetUpdatedBeat()
        {
            return Save.Manifest.DetectionType == BeatDetectionType.FFT 
                ? GetUpdatedFFTBeat() 
                : GetUpdatedBPMBeat();
        }

        private bool[] GetUpdatedFFTBeat()
        {
            if (CurrentVertex.Type == VertexType.None)
                return new bool[WorkTread.TrackFFT.GetBeatSecond(new TimeSpan(0, 0, 0, CurrentSecond)).Count];
            var fftVertex = (FFTVertex)CurrentVertex;
            WorkTread.TrackFFT.HighEdge = fftVertex.TopFrequency;
            WorkTread.TrackFFT.LowEdge = fftVertex.BotFrequency;
            WorkTread.TrackFFT.ThresholdValue = fftVertex.ThresholdValue;
            return WorkTread.TrackFFT.GetBeatSecond(new TimeSpan(0, 0, 0, CurrentSecond)).ToArray();
        }

        private bool[] GetUpdatedBPMBeat()
        {
            if (CurrentVertex.Type == VertexType.None)
                return new bool[WorkTread.TrackFFT.GetBeatSecond(new TimeSpan(0, 0, 0, CurrentSecond)).Count];
            var bpmVertex = (BPMVertex)CurrentVertex;
            var interval = BPM2Millisecond(bpmVertex.BPM);
            var beatNumber = 0;
            while (bpmVertex.Time.TotalSeconds * 1000 + interval * beatNumber < CurrentSecond * 1000) beatNumber++;
            var startBeat = bpmVertex.Time.TotalSeconds * 1000 + interval * beatNumber - CurrentSecond * 1000;
            var beatArray = new bool[FramesPerSecond];
            while (startBeat < 1000)
            {
                var millisecond = Millisecond2Position((int)startBeat);
                var number = millisecond < FramesPerSecond ? millisecond : FramesPerSecond - 1;
                beatArray[(int)number] = true;
                startBeat += interval;
            }

            return beatArray;
        }
    }
}