using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatMeGameModel.BeatVertexes
{
    public class FFTVertex : BeatVertex
    {
        public int TopFrequency { get; set; }
        public int BotFrequency { get; set; }
        public double ThresholdValue { get; set; }

        public FFTVertex(TimeSpan time, VertexType type, int topFrequency, int botFrequency, double thresholdValue) :
            base(time, type)
        {
            TopFrequency = topFrequency;
            BotFrequency = botFrequency;
            ThresholdValue = thresholdValue;
        }

        public override string ToString()
        {
            return string.Join(" ", base.ToString(), TopFrequency, BotFrequency, ThresholdValue);
        }
    }
}
