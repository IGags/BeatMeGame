using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoundEngineLibrary;
using BeatMeGameModel.BeatVertexes;
//ToDo: Пофиксить баг с неубиранием меток на удаление, после удаления врешины на предыдущем листе
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

        public void UnpackVertexes(int second, int length, Func<Dictionary<int, BeatVertex>, PackingDirection,BeatVertex[]> Unpacker, PackingDirection direction)
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
            Vertices = Unpacker(ms, direction);
        }

        public int Millisecond2Position(int time)
        {
            return (time * FramesPerSecond) / 1000;
        }

        public int Position2Millisecond(int position)
        {
            return (position * 1000) / FramesPerSecond;
        }

        public BeatVertex[] FFTUnpacker(Dictionary<int, BeatVertex> vertexList, PackingDirection direction)
        {
            var typeArray = new BeatVertex[FramesPerSecond];
            var beatArray = new bool[FramesPerSecond];
            if (direction == PackingDirection.Backward)
            {
                if (previousVertexStack.Count != 0)
                {
                    CurrentVertex = previousVertexStack.Pop();
                }
            }

            beatArray = GetUpdatedBeat();
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
                        beatArray = GetUpdatedBeat();
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
            var beat = GetUpdatedBeat();
            if (position != -1) Vertices[position] = updatedVertex;
            for (int i = position + 1; i < FramesPerSecond; i++)
            {
                if(Vertices[i].Type == VertexType.FFT) break;
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
        }

        public void AddMonoVertex(int position, BeatVertex vertex)
        {
            var beat = GetUpdatedBeat();
            if (position < beat.Length)
            {
                Vertices[position] = vertex;
            }

        }

        public void DeleteFFTVertex(int position)
        {
            var prevFFTPos = -1;
            Vertices[position] = new BeatVertex(new TimeSpan(0, 0, 0, CurrentSecond, Position2Millisecond(position)),
                VertexType.None);
            for (int i = position - 1; i >= 0; i--)
            {
                if (Vertices[i].Type == VertexType.FFT)
                {
                    prevFFTPos = i;
                    break;
                }
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
                    }
                }
                else
                {
                    AddFFTVertex(-1, (FFTVertex)previousVertexStack.Peek());
                }
            }
            else
            {
                AddFFTVertex(prevFFTPos, (FFTVertex)Vertices[prevFFTPos]);
            }
        }

        public void GetNextSecond()
        {
            if((int)WorkTread.MaxSongDuration.TotalSeconds == CurrentSecond) return;
            CurrentSecond++;
            PackFFTVertexes(Vertices, PackingDirection.Forward);
            UnpackVertexes(CurrentSecond, FramesPerSecond, FFTUnpacker, PackingDirection.Forward);
        }

        public void GetPreviousSecond()
        {
            if(CurrentSecond == 0) return;
            CurrentSecond--;
            PackFFTVertexes(Vertices, PackingDirection.Backward);
            UnpackVertexes(CurrentSecond, FramesPerSecond, FFTUnpacker, PackingDirection.Backward);
        }

        private bool[] GetUpdatedBeat()
        {
            if (CurrentVertex.Type == VertexType.None)
                return new bool[WorkTread.TrackFFT.GetBeatSecond(new TimeSpan(0, 0, 0, CurrentSecond)).Count];
            var fftVertex = (FFTVertex)CurrentVertex;
            WorkTread.TrackFFT.HighEdge = fftVertex.TopFrequency;
            WorkTread.TrackFFT.LowEdge = fftVertex.BotFrequency;
            WorkTread.TrackFFT.ThresholdValue = fftVertex.ThresholdValue;
            return WorkTread.TrackFFT.GetBeatSecond(new TimeSpan(0, 0, 0, CurrentSecond)).ToArray();
        }
    }
}