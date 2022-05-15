using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BeatMeGame.EditorView
{
    public class SpectrographLineCanvas : Panel
    {
        private double time;
        private Pen pen = new Pen(Color.Green, 10);

        public SpectrographLineCanvas()
        {
            DoubleBuffered = true;
        }

        public void DrawNormalizedSecondIndicator(double time)
        {
            this.time = time;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            var xPos = (int)(time * Width);
            graphics.DrawLine(pen, xPos, 0, xPos, Height);
        }
    }
}
