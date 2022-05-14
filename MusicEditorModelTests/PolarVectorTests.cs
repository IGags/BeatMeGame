using NUnit;
using System;
using System.Linq.Expressions;
using BeatMeGameModel;
using NUnit.Framework;


namespace MusicEditorModelTests
{
    [TestFixture]
    public class PolarVectorTests
    {
        public void ToPolarTests(int x, int y, PolarVector should)
        {
            var polar = new PolarVector(x, y);
            Assert.AreEqual(should.Length, polar.Length, 1e-10);
            Assert.AreEqual(should.Angle, polar.Angle, 1e-10);
        }
        [Test]
        public void ZeroVectorTest() { ToPolarTests(0, 0, new PolarVector(0d, 0d)); }
        [Test]
        public void ZeroAngleTest(){ToPolarTests(100, 0, new PolarVector(0d, 100d));}
        [Test]
        public void PiDivideTwoTest() { ToPolarTests(0, 10, new PolarVector(Math.PI / 2, 10)); }
        [Test]
        public void PiTest() { ToPolarTests(-10, 0, new PolarVector(Math.PI, 10)); }
        [Test]
        public void FourPiDivideThree() { ToPolarTests(0, -10, new PolarVector(3 * Math.PI / 2, 10)); }

        [Test]
        public void FirstQuarterTest() { ToPolarTests(10, 10, new PolarVector(Math.PI / 4, 10 * Math.Sqrt(2))); }
        [Test]
        public void SecondQuarterTest() { ToPolarTests(-10, 10, new PolarVector(3 * Math.PI / 4, 10 * Math.Sqrt(2))); }
        [Test]
        public void ThirdQuarterTest() { ToPolarTests(-10, -10, new PolarVector(5 * Math.PI / 4, 10 * Math.Sqrt(2))); }
        [Test]
        public void FourthQuarterTest() { ToPolarTests(10, -10, new PolarVector(7 * Math.PI / 4, 10 * Math.Sqrt(2))); }

        public void SumPolarTests(PolarVector first, PolarVector second, PolarVector should)
        {
            var sum = first + second;
            Assert.AreEqual(should.Length, sum.Length, 1e-10);
            Assert.AreEqual(should.Angle, sum.Angle, 1e-10);
        }

        [Test]
        public void SumTwoZeroAngleTest() { SumPolarTests(new PolarVector(10, 0), new PolarVector(20, 0), new PolarVector(30, 0)); }
        [Test]
        public void SumTwoZeroTest(){ SumPolarTests(new PolarVector(), new PolarVector(), new PolarVector());}
        [Test]
        public void SumTwoOppositeTest(){ SumPolarTests(new PolarVector(10,10), new PolarVector(-10, -10), new PolarVector()); }
        [Test]
        public void SumTwoPerpendicularTest(){ SumPolarTests(new PolarVector(10, 10), new PolarVector(-10, 10), new PolarVector(Math.PI / 2, 20));}

        [Test]
        public void RandomSumTest() //Странный тест(ломает сложение) ошибка округления починил :^)
        {
            var random = new Random();
            for (int i = 0; i < 500; i++)
            {
                var firstX = random.Next(-100, 100);
                var secondX = random.Next(-100, 100);
                var firstY = random.Next(-100, 100);
                var secondY = random.Next(-100, 100);
                SumPolarTests(new PolarVector(firstX, firstY), new PolarVector(secondX, secondY),
                    new PolarVector(firstX + secondX, firstY + secondY));
            }
        }

    }
}
