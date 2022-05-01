using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatMeGameModel;
using BeatMeGameModel.MenuBGModels;
using SoundEngineLibrary;

namespace BeatMeGame.MenuBGWrappers
{
    public class StarfieldWrapper : IMenuBGWrapper
    {
        private StarfieldModel bgModel;
        private SoundEngine engine;
        private readonly SpectrumBarsModel bars = new SpectrumBarsModel();
        private Size ClientSize;
        private string treadName;

        public StarfieldWrapper(int xSize, int ySize, SoundEngine engine, string treadName)
        {
            Initialize(xSize, ySize, engine, treadName);
        }

        public void Initialize(int x, int y, SoundEngine engine, string treadName)
        {
            bgModel = new StarfieldModel(x, y);
            this.engine = engine;
            ClientSize = new Size(x, y);
            this.treadName = treadName;
        }

        public void Resize(int x, int y)
        {
            bgModel = new StarfieldModel(x, y);
            ClientSize = new Size(x, y);
        }

        public void Redraw(Graphics graphics)
        {
            bgModel.MoveStars();
            foreach (var star in bgModel.Stars)
            {
                var starVector = star.PositionShift + star.InitialPosition;
                var colorBrightness = (int)(star.Speed / StarfieldModel.SpeedBase * 200) + 55;
                colorBrightness = Math.Abs(colorBrightness) > 255 ? 255 : colorBrightness;
                var starPen = new Pen(Color.FromArgb(255, colorBrightness, colorBrightness, colorBrightness));
                graphics.DrawLine(starPen, ToGraphicsCoordinates(starVector), ToGraphicsCoordinates(new PolarVector(starVector.Angle, starVector.Length - star.Speed)));
            }

            if (treadName == null) return;
            var spectrum = engine.GetTread(treadName).TrackFFT
                .GetFFTData(engine.GetTread(treadName).CurrentTrack.CurrentTime).ToList();
            var updatedSpectrum = bars.Update(spectrum).Take(100).ToList();
            var barWidth = (float)ClientSize.Width / updatedSpectrum.Count;
            var totalBars = updatedSpectrum.Count;
            for (var i = 0; i < totalBars; i++)
            {
                var barBrush = new SolidBrush(GetBrushColor(i, totalBars));
                var bar = (float)updatedSpectrum[i] / 20 + 10;
                if (bar > ClientSize.Height / 3) bar = ClientSize.Height / 3;
                graphics.FillRectangle(barBrush, barWidth * i, ClientSize.Height - bar, barWidth, bar);
            }
        }

        private Point ToGraphicsCoordinates(PolarVector vector)
        {
            var cartesianCoordinates = PolarVector.ToCartesianСoordinates(vector);
            var screenPosition = new Point(cartesianCoordinates.Item1, cartesianCoordinates.Item2);
            return new Point(screenPosition.X + ClientSize.Width / 2, ClientSize.Height / 2 - screenPosition.Y);
        }

        private Color GetBrushColor(int barNumber, int totalBars)
        {
            var currentTime = engine.GetTread(treadName).CurrentTrack.CurrentTime.TotalSeconds;
            var relativeTimePart = currentTime / engine.GetTread(treadName).MaxSongDuration.TotalSeconds;
            var paintedBarCount = relativeTimePart * totalBars;
            if (paintedBarCount < barNumber) return Color.FromArgb(150, 122, 122, 122);
            var barDifference = paintedBarCount - barNumber;
            return barDifference >= 1 ? Color.FromArgb(150, 255, 122, 0)
                : Color.FromArgb(150, (int)(122 + 123 * barDifference), 122, (int)(122 - 122 * barDifference));
        }
    }
}
