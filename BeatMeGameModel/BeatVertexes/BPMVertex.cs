using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatMeGameModel.BeatVertexes
{
    public class BPMVertex : BeatVertex
    {
        public double BPM { get; set; }

        public BPMVertex(TimeSpan time, VertexType type, double bpm) :
            base(time, type)
        {
            BPM = bpm;
        }
    }
}
