using System;

namespace BeatMeGameModel
{
    public struct PolarVector
    {
        public double Angle { get; set; }
        public double Length { get; set; }

        public PolarVector(double angle = 0, double length = 0)
        {
            Angle = angle;
            Length = length;
        }

        public PolarVector(int x, int y)
        {
            this = ToPolarCoordinates(x, y);
        }

        public static PolarVector ToPolarCoordinates(int x, int y)
        {
            var length = Math.Sqrt(x * x + y * y);
            var angle = Math.Acos(x / length);
            if (y < 0) angle += Math.PI;
            return new PolarVector(angle, length);
        }

        public static (int, int) ToCartesianСoordinates(PolarVector vector)
        {
            var x = (int)(vector.Length * Math.Cos(vector.Angle));
            var y = (int)(vector.Length * Math.Sin(vector.Angle));
            return (x, y);
        }

        public static PolarVector operator +(PolarVector first, PolarVector second)
        {
            if (first.Angle - second.Angle < 1e2) return new PolarVector(first.Angle, first.Length + second.Length);
            var length = first.Length * first.Length + second.Length + second.Length -
                         2 * first.Length * second.Length * Math.Cos(first.Angle + Math.PI - second.Angle);
            var angle = Math.Asin(Math.Sin(first.Angle + Math.PI - second.Angle) * (second.Length / length)) -
                        first.Angle;
            return new PolarVector(angle, length);
        }
    }
}
