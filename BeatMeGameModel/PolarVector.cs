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
            if (double.IsNaN(angle)) angle = 0;
            if (y < 0) angle = 2* Math.PI - angle;
            return new PolarVector(angle, length);
        }

        public static (int, int) ToCartesianСoordinates(PolarVector vector)
        {
            var x = (int)Math.Round(vector.Length * Math.Cos(vector.Angle));
            var y = (int)Math.Round(vector.Length * Math.Sin(vector.Angle));
            return (x, y);
        }

        public static PolarVector operator +(PolarVector first, PolarVector second)
        {
            var firstCart = ToCartesianСoordinates(first);
            var secondCart = ToCartesianСoordinates(second);
            return new PolarVector((int)(firstCart.Item1 + secondCart.Item1), (int)(firstCart.Item2 + secondCart.Item2));
        }
    }
}
