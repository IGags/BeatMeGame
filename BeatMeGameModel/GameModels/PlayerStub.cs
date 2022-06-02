using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatMeGameModel.GameModels
{
    public class PlayerStub : IPlayer
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Velocity { get; }
        public int SuperPercent { get; set; }
        public bool IsSuperDecreasing { get; set; }
        public Game TargetGame { get; set; }
        public void Shoot()
        {
        }

        public void Move(Directions direction, bool isSlowed)
        {
        }

        public void ActivateSuper()
        {
        }

        public IPlayer Copy()
        {
            return new PlayerStub(X, Y);
        }

        public PlayerStub(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
