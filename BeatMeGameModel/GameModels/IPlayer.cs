using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatMeGameModel.GameModels
{
    public enum Directions
    {
        Up,
        Down,
        Left,
        Right
    }
    public interface IPlayer
    {
        double X { get;  set; }
        double Y { get; set; }
        double Velocity { get; }
        int SuperPercent { get; set; }
        bool IsSuperDecreasing { get; set; }
        Game TargetGame { get; set; }
        void Shoot();
        void Move(Directions direction, bool isSlowed);
        void ActivateSuper();
        IPlayer Copy();
    }
}
