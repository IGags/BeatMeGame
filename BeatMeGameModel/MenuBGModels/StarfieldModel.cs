using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatMeGameModel.IOWorkers
{
    public class StarfieldModel
    {
        public readonly List<StarfieldStar> Stars = new List<StarfieldStar>();
        public const int SpeedBase = 100;
        private const int StarsCount = 1000;
        private const int SpawnRadius = 200;
        private Random Random { get; } = new Random();
        private readonly int cutoutCircleRadius;

        public StarfieldModel(int sizeX, int sizeY)
        {
            cutoutCircleRadius = (int)Math.Sqrt(sizeX * sizeX + sizeY * sizeY);
            for (int i = 0; i < StarsCount; i++)
            {
                var initialPosition = GetRandomInitialPosition(SpawnRadius);
                Stars.Add(new StarfieldStar(initialPosition, new PolarVector(initialPosition.Angle,
                    Random.Next(Math.Abs(cutoutCircleRadius - (int)initialPosition.Length)))));
            }
        }

        public void MoveStars()
        {
            for (var i = 0; i < StarsCount; i++)
            {
                Stars[i].PositionShift = new PolarVector(Stars[i].PositionShift.Angle, Stars[i].PositionShift.Length + Stars[i].Speed);
                if ((Stars[i].PositionShift + Stars[i].InitialPosition).Length > cutoutCircleRadius)
                {
                    Stars[i].InitialPosition = GetRandomInitialPosition(SpawnRadius);
                    Stars[i].PositionShift = new PolarVector(Stars[i].InitialPosition.Angle);
                }
                Stars[i].Speed = SpeedBase * (Stars[i].PositionShift.Length + Stars[i].InitialPosition.Length) / cutoutCircleRadius;
            }
        }

        public PolarVector GetRandomInitialPosition(int spawnRadius)
        {
            var xShift = (Random.NextDouble() - 0.5) * spawnRadius;
            var yShift = Math.Sqrt(spawnRadius * spawnRadius - xShift * xShift) * (Random.NextDouble() - 0.5);
            return new PolarVector((int)xShift, (int)yShift);
        }
    }
}
