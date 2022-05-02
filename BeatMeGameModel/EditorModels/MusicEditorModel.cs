using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoundEngineLibrary;
using BeatMeGameModel.BeatVertexes;
using BeatMeGameModel.IOWorkers;

//ToDo: починить кнопки перехода между страицами сломались сильно!!!
namespace BeatMeGameModel.EditorModels
{
    public enum PackingDirection
    {
        Forward,
        Backward
    }
    public class MusicEditorModel
    {
        public SoundEngineTread WorkTread { get; }
        public LevelSave Save { get; private set; }
        public int CurrentSecond { get; private set; }
        public int FramesPerSecond { get; }
        public BeatVertex[] Vertices { get; private set; }
        private BeatVertex CurrentVertex { get; set; } = new BeatVertex(TimeSpan.Zero, VertexType.None);
        private BeatVertex lastVertexPerFrame = new BeatVertex(TimeSpan.Zero, VertexType.None);
        private readonly Stack<BeatVertex> previousVertexStack = new Stack<BeatVertex>();

        private Dictionary<TimeSpan, BeatVertex> alternativeType = new Dictionary<TimeSpan, BeatVertex>();
        public MusicEditorModel(SoundEngineTread tread, LevelSave save)
        {
            WorkTread = tread;
            Save = save;
            FramesPerSecond = WorkTread.TrackFFT.samplingFrequency / FFT.FFTSize;
            CurrentSecond = save.Manifest.StartSecond;
        }

        public void UnpackVertexes(int second, int length, 
            Func<Dictionary<int, BeatVertex>, PackingDirection,BeatVertex[]> unpacker, 
            PackingDirection direction)
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
            Vertices = unpacker(ms, direction);
        }

        public int Millisecond2Position(int time)
        {
            return (time * FramesPerSecond) / 1000;
        }

        public int Position2Millisecond(int position)
        {
            return (position * 1000) / FramesPerSecond;
        }

        public int BPM2Millisecond(double bpm)
        {
            return (int)(1000 / (bpm / 60));
        }

        public BeatVertex[] FFTUnpacker(Dictionary<int, BeatVertex> vertexList, PackingDirection direction)
        {
            var typeArray = new BeatVertex[FramesPerSecond];
            bool[] beatArray;
            if (direction == PackingDirection.Backward)
            {
                while (previousVertexStack.Count != 0 && previousVertexStack.Peek().Time.TotalSeconds > CurrentSecond)
                    previousVertexStack.Pop();
                if (previousVertexStack.Count != 0)
                {

                    CurrentSecond = previousVertexStack.Peek().Time.Milliseconds == 0
                        ? (int)previousVertexStack.Pop().Time.TotalSeconds
                        : (int)previousVertexStack.Peek().Time.TotalSeconds;
                    beatArray = GetUpdatedFFTBeat();
                }
                else
                {
                    beatArray = new bool[FramesPerSecond];
                }
            }
            else beatArray = GetUpdatedFFTBeat();
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
                        if (vertexList[i].Type != VertexType.FFT) continue;
                        lastVertexPerFrame = CurrentVertex = vertexList[i];
                        beatArray = GetUpdatedFFTBeat();
                    }

                    else if (beatArray[i])
                        typeArray[i] = new BeatVertex(new TimeSpan(0, 0, 0, CurrentSecond, Position2Millisecond(i)),
                            VertexType.AdditionalFFT);
                    else
                    {
                        typeArray[i] = new BeatVertex(new TimeSpan(0, 0, 0, CurrentSecond, Position2Millisecond(i)),
                            VertexType.None);
                    }
                }
            }
            return typeArray;
        }

        public BeatVertex[] BPMUnpacker(Dictionary<int, BeatVertex> vertexList, PackingDirection direction)
        {
            var typeArray = new BeatVertex[FramesPerSecond];
            bool[] beatArray;
            if (direction == PackingDirection.Backward)
            {
                while (previousVertexStack.Count != 0 && previousVertexStack.Peek().Time.TotalSeconds > CurrentSecond)
                    previousVertexStack.Pop();
                if (previousVertexStack.Count != 0)
                {

                    CurrentSecond = previousVertexStack.Peek().Time.Milliseconds == 0
                        ? (int)previousVertexStack.Pop().Time.TotalSeconds
                        : (int)previousVertexStack.Peek().Time.TotalSeconds;
                    beatArray = GetUpdatedBPM();
                }
                else
                {
                    beatArray = new bool[FramesPerSecond];
                }
            }
            else beatArray = GetUpdatedBPM();

            for (int i = 0; i < typeArray.Length; i++)
            {
                if (vertexList.ContainsKey(i))
                {
                    typeArray[i] = vertexList[i];
                    if(vertexList[i].Type != VertexType.BPM)continue;
                    CurrentVertex = lastVertexPerFrame = vertexList[i];
                    beatArray = GetUpdatedBPM();
                }

                else if (beatArray[i])
                    typeArray[i] = new BeatVertex(new TimeSpan(0, 0, 0, CurrentSecond, Position2Millisecond(i)),
                        VertexType.AdditionalBPM);
                else
                {
                    typeArray[i] = new BeatVertex(new TimeSpan(0, 0, 0, CurrentSecond, Position2Millisecond(i)),
                        VertexType.None);
                }
            }

            return typeArray;
        }

        public void ChangeAnalyzeType()
        {
            if(Save.Manifest.DetectionType == BeatDetectionType.FFT) PackFFTVertexes(Vertices, PackingDirection.Forward);
            else PackBeatVertices(Vertices, PackingDirection.Forward);
            (alternativeType, Save.Beat) = (Save.Beat, alternativeType);
            previousVertexStack.Clear();
            CurrentSecond = 0;
            if (Save.Manifest.DetectionType == BeatDetectionType.FFT)
            {
                CurrentVertex = lastVertexPerFrame = new BeatVertex(TimeSpan.Zero, VertexType.None);
                Save.Manifest.DetectionType = BeatDetectionType.BPM;
                UnpackVertexes(CurrentSecond, FramesPerSecond, BPMUnpacker, PackingDirection.Forward);
            }
            else
            {
                CurrentVertex = lastVertexPerFrame = new BeatVertex(TimeSpan.Zero, VertexType.None);
                Save.Manifest.DetectionType = BeatDetectionType.FFT;
                UnpackVertexes(CurrentSecond, FramesPerSecond, FFTUnpacker, PackingDirection.Forward);
            }
        }

        public void PackFFTVertexes(BeatVertex[] vertices, PackingDirection direction)
        {
            foreach (var vertex in vertices)
            {
                if (vertex.Type == VertexType.None || vertex.Type == VertexType.AdditionalFFT) continue;
                Save.Beat[vertex.Time] = vertex;
                if (vertex.Type == VertexType.FFT) CurrentVertex = vertex;
            }
            lastVertexPerFrame = CurrentVertex;
            if(direction == PackingDirection.Forward)previousVertexStack.Push(lastVertexPerFrame);
        }

        public void AddFFTVertex(int position, FFTVertex updatedVertex)
        {
            CurrentVertex = updatedVertex;
            var beat = GetUpdatedFFTBeat();
            if (position != -1) Vertices[position] = updatedVertex;
            for (int i = position + 1; i < FramesPerSecond; i++)
            {
                if(Vertices[i].Type == VertexType.FFT) return;
                if (beat.Length <= i)
                    Vertices[i] = new BeatVertex(new TimeSpan(0, 0, 0, CurrentSecond, Position2Millisecond(i)),
                        VertexType.None);
                else if(Vertices[i].Type == VertexType.Artificial) continue;
                else if (beat[i])
                {
                    if(Vertices[i].Type == VertexType.AdditionalFFT) continue;
                    Vertices[i] = new BeatVertex(new TimeSpan(0, 0, 0, CurrentSecond, Position2Millisecond(i)),
                        VertexType.AdditionalFFT);
                }
                else
                {
                    Vertices[i] = new BeatVertex(new TimeSpan(0, 0, 0, CurrentSecond, Position2Millisecond(position)),
                        VertexType.None);
                }
            }
            ClearDeletionVertexes();
        }

        public void AddBPMVertex(int position, BPMVertex updatedVertex)
        {
            CurrentVertex = updatedVertex;
            var beat = GetUpdatedBPM();
            if (position != -1) Vertices[position] = updatedVertex;
            for (int i = position + 1; i < FramesPerSecond; i++)
            {
                if (Vertices[i].Type == VertexType.BPM) return;
                if (beat.Length <= i)
                    Vertices[i] = new BeatVertex(new TimeSpan(0, 0, 0, CurrentSecond, Position2Millisecond(i)),
                        VertexType.None);
                else if (Vertices[i].Type == VertexType.Artificial) continue;
                else if (beat[i])
                {
                    if (Vertices[i].Type == VertexType.AdditionalBPM) continue;
                    Vertices[i] = new BeatVertex(new TimeSpan(0, 0, 0, CurrentSecond, Position2Millisecond(i)),
                        VertexType.AdditionalBPM);
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
            var beat = GetUpdatedFFTBeat();
            if (position < beat.Length)
            {
                Vertices[position] = vertex;
            }

        }

        public void PackBeatVertices(BeatVertex[] vertices, PackingDirection direction)
        {
            foreach (var vertex in Vertices)
            {
                if(vertex.Type == VertexType.AdditionalBPM || vertex.Type == VertexType.None) continue;
                Save.Beat[vertex.Time] = vertex;
                if (vertex.Type == VertexType.BPM) CurrentVertex = vertex;
            }

            lastVertexPerFrame = CurrentVertex;
            if(direction == PackingDirection.Forward) previousVertexStack.Push(lastVertexPerFrame);
        }

        public void DeleteFFTVertex(int position)
        {
            var prevFFTPos = -1;
            Vertices[position] = new BeatVertex(new TimeSpan(0, 0, 0, CurrentSecond, Position2Millisecond(position)),
                VertexType.None);
            for (int i = position - 1; i >= 0; i--)
            {
                if (Vertices[i].Type != VertexType.FFT) continue;
                prevFFTPos = i;
                break;
            }

            if (prevFFTPos == -1)
            {
                if (!previousVertexStack.Any())
                {
                    for (int i = 0; i < Vertices.Length; i++)
                    {
                        if (Vertices[i].Type == VertexType.FFT)
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
                else AddFFTVertex(-1, (FFTVertex)previousVertexStack.Peek());
            }
            else AddFFTVertex(prevFFTPos, (FFTVertex)Vertices[prevFFTPos]);
        }

        public void GetNextSecond()
        {
            if((int)WorkTread.MaxSongDuration.TotalSeconds == CurrentSecond) return;
            CurrentSecond++;
            switch (Save.Manifest.DetectionType)
            {
                case BeatDetectionType.FFT:
                    PackFFTVertexes(Vertices, PackingDirection.Forward);
                    UnpackVertexes(CurrentSecond, FramesPerSecond, FFTUnpacker, PackingDirection.Forward);
                    break;
                case BeatDetectionType.BPM:
                    PackBeatVertices(Vertices, PackingDirection.Forward);
                    UnpackVertexes(CurrentSecond, FramesPerSecond, BPMUnpacker, PackingDirection.Forward);
                    break;
            }
        }

        public void GetPreviousSecond()
        {
            if(CurrentSecond == 0) return;
            CurrentSecond--;
            switch (Save.Manifest.DetectionType)
            {
                case BeatDetectionType.FFT:
                    PackFFTVertexes(Vertices, PackingDirection.Backward);
                    UnpackVertexes(CurrentSecond, FramesPerSecond, FFTUnpacker, PackingDirection.Backward);
                    break;
                case BeatDetectionType.BPM:
                    PackBeatVertices(Vertices, PackingDirection.Backward);
                    UnpackVertexes(CurrentSecond, FramesPerSecond, BPMUnpacker, PackingDirection.Backward);
                    break;
            }
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
                if (Save.Manifest.DetectionType == BeatDetectionType.FFT)
                {
                    PackFFTVertexes(Vertices, PackingDirection.Backward);
                    previousVertexStack.Clear();
                    UnpackVertexes(CurrentSecond, FramesPerSecond, FFTUnpacker, PackingDirection.Forward);
                }
                else
                {
                    PackBeatVertices(Vertices, PackingDirection.Backward);
                    previousVertexStack.Clear();
                    UnpackVertexes(CurrentSecond, FramesPerSecond, BPMUnpacker, PackingDirection.Forward);
                }
                while (CurrentSecond != time)
                {
                    GetNextSecond();
                }
            }
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

        private bool[] GetUpdatedBPM()
        {
            if (CurrentVertex.Type == VertexType.None)
                return new bool[WorkTread.TrackFFT.GetBeatSecond(new TimeSpan(0, 0, 0, CurrentSecond)).Count];
            var bpmVertex = (BPMVertex)CurrentVertex;
            var interval = BPM2Millisecond(bpmVertex.BPM);
            var beatNumber = 0;
            while (bpmVertex.Time.TotalSeconds + interval * beatNumber < CurrentSecond) beatNumber++;
            var startBeat = (int)((bpmVertex.Time.TotalSeconds + interval * beatNumber - CurrentSecond) * 1000);
            var beatArray = new bool[FramesPerSecond];
            while (startBeat < 1000)
            {
                beatArray[Millisecond2Position(startBeat)] = true;
                startBeat += interval;
            }

            return beatArray;
        }
    }
}