using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BeatMeGame.LevelEditorView
{
    class PaintPanel : Panel
    {
        private Bitmap editorBitmap = new Bitmap(32, 32);
        private float pixelSize;

        public PaintPanel()
        {
            pixelSize = (float)Height / editorBitmap.Height;
            DoubleBuffered = true;
            Click += (sender, args) =>
            {
                var mouse = (MouseEventArgs)args;
                if(mouse.X > ClientSize.Height) return;
                editorBitmap.SetPixel(editorBitmap.Width * mouse.X / ClientSize.Height, editorBitmap.Height * mouse.Y / ClientSize.Height, Color.Red);
                Invalidate();
            };

            SizeChanged += (sender, args) =>
            {
                pixelSize = (float)Height / editorBitmap.Height;
            };

            //Controls.Add(paintingArea);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.FillRectangle(new SolidBrush(Color.AliceBlue), new Rectangle(0, 0, Width, Height));
            DrawGrid(e.Graphics);
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.DrawImage(editorBitmap, new Rectangle(0, 0, this.Height, this.Height)); 
        }

        private void DrawGrid(Graphics g)
        {
            var pen = new Pen(Color.DarkGreen, 2);
            for (int i = 1; i < editorBitmap.Height; i++)
            {
                g.DrawLine(pen, i * pixelSize, 0, i * pixelSize, Height);
            }

            for (int i = 1; i < editorBitmap.Height; i++)
            {
                g.DrawLine(pen, 0, i * pixelSize, Height, i * pixelSize);
            }
        }
    }
}
