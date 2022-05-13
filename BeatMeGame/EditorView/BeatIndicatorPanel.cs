using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace BeatMeGame.EditorView
{
    public class BeatIndicatorPanel : Panel
    {
        private readonly Bitmap DefaultBitmap = new Bitmap("Resources\\BeatCheckerInactive.png");
        private readonly Bitmap ActivatedBitmap = new Bitmap("Resources\\BeatCheckerActive0.png");
        private Bitmap currentBitmap;
        public BeatIndicatorPanel()
        {
            DoubleBuffered = true;
            currentBitmap = DefaultBitmap;
        }

        public void BeatDetected()
        {
            currentBitmap = ActivatedBitmap;
            Invalidate();
        }

        public void ToDefault()
        {
            currentBitmap = DefaultBitmap;
            Invalidate();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            graphics.DrawImage(currentBitmap, new Rectangle(0, 0, this.Width, Height));
            base.OnPaint(e);
        }
    }
}
