using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatMeGame.EditorView
{
    public class SpectrogramWrapper
    {
        private static readonly Brush[] brushes = new[]
        {
            new SolidBrush(Color.FromArgb(255, 65, 0, 0)),
            new SolidBrush(Color.FromArgb(255, 95, 0, 0)),
            new SolidBrush(Color.FromArgb(255, 125, 0, 0)),
            new SolidBrush(Color.FromArgb(255, 255, 0, 0)),
            new SolidBrush(Color.FromArgb(255, 255, 56, 0)),
            new SolidBrush(Color.FromArgb(255, 255, 121, 0)),
            new SolidBrush(Color.FromArgb(255, 255, 147, 44)),
            new SolidBrush(Color.FromArgb(255, 255, 152, 54)),
            new SolidBrush(Color.FromArgb(255, 255, 180, 107)),
            new SolidBrush(Color.FromArgb(255, 255, 209, 163))
        };

        public void DrawSpectrogram(Graphics graphics, List<List<double>> normalizedData)
        {
            if (normalizedData == null || !normalizedData.Any() || !normalizedData[0].Any()) return;
            var paintRectangle = graphics.VisibleClipBounds;
            var rectangleWidth = paintRectangle.Width / normalizedData[0].Count;
            var rectangleHeight = paintRectangle.Height / normalizedData.Count;
            for (int i = 0; i < normalizedData.Count; i++)
            {
                for (int j = 0; j < normalizedData[i].Count; j++)
                {
                    graphics.FillRectangle(SelectBrush(normalizedData[i][j]), j * rectangleWidth, paintRectangle.Height - rectangleHeight * (i + 1),
                        rectangleWidth, rectangleHeight);
                }
            }
        }

        public Brush SelectBrush(double value)
        {
            return brushes[(int)((value - 1e-10) * 10)];
        }
    }
}
