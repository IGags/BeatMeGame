using System;

namespace BeatMeGameModel.BeatVertexes
{
    public enum VertexType
    {
        Artificial,
        BPM,
        FFT,
        Deletion, 
        None,
        AdditionalBPM,
        AdditionalFFT
    }

    public class BeatVertex
    {
        public TimeSpan Time { get; private set; }
        public VertexType Type { get; private set; }

        public BeatVertex(TimeSpan time, VertexType type)
        {
            Time = time;
            Type = type;
        }
    }
}
