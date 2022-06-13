using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatMeGameModel.IOWorkers
{
    public class StarfieldStar
    {
        public PolarVector InitialPosition { get; set; }
        public PolarVector PositionShift { get; set; }
        public double Speed { get; set; } = 0;

        public StarfieldStar(PolarVector initialPosition, PolarVector positionShift = new PolarVector())
        {
            InitialPosition = initialPosition;
            PositionShift = positionShift;
        }
    }
}
