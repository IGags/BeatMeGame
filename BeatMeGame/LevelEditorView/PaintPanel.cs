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
        private Color selectedColor = Color.Black;

        public PaintPanel()
        {
            var colorButtonsHorizontalMargin = editorBitmap.Width * pixelSize;
            pixelSize = (float)Height / editorBitmap.Height;
            DoubleBuffered = true;
            InitializeColorButtons();

            Click += (sender, args) =>
            {
                var mouse = (MouseEventArgs)args;
                if(mouse.X > ClientSize.Height) return;
                editorBitmap.SetPixel(editorBitmap.Width * mouse.X / ClientSize.Height, editorBitmap.Height * mouse.Y / ClientSize.Height, selectedColor);
                Invalidate();
            };

            SizeChanged += (sender, args) =>
            {
                pixelSize = (float)Height / editorBitmap.Height;
                InitializeColorButtons();
            };

            //Controls.Add(paintingArea);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.FillRectangle(new SolidBrush(Color.Gray), new Rectangle(0, 0, Width, Height));
            DrawGrid(e.Graphics);
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.DrawImage(editorBitmap, new Rectangle(0, 0, this.Height, this.Height)); 
        }

        private void DrawGrid(Graphics g)
        {
            var pen = new Pen(Color.DarkGreen, 0.1f);
            for (int i = 1; i < editorBitmap.Height; i++)
            {
                g.DrawLine(pen, i * pixelSize, 0, i * pixelSize, Height);
            }

            for (int i = 1; i < editorBitmap.Height; i++)
            {
                g.DrawLine(pen, 0, i * pixelSize, Height, i * pixelSize);
            }
        }

        private void InitializeColorButtons()
        {
            var verticalCount = 7;
            var horizontalCount = 8;
            Controls.Clear();
            var colorButtonSize = new Size(Width / 40, Width / 40);
            var colorButtonsHorizontalMargin = (int)(Width -  editorBitmap.Width * pixelSize - horizontalCount * colorButtonSize.Width) / (horizontalCount + 1);
            var colorButtonsVerticalMargin = (int)(Height * 0.8 - colorButtonSize.Height * verticalCount) / verticalCount;
            colorButtonsHorizontalMargin = colorButtonsHorizontalMargin >= 0 ? colorButtonsHorizontalMargin : 0;
            colorButtonsVerticalMargin = colorButtonsVerticalMargin >= 0 ? colorButtonsVerticalMargin : 0;
            var random = new Random();

            for (int j = 0; j < verticalCount; j++)
            {
                for (int i = 0; i < horizontalCount; i++)
                {
                    var button = new ColorButton()
                    {
                        FlatStyle = FlatStyle.Flat,
                        Size = colorButtonSize,
                        Location = new Point((int)(editorBitmap.Width * pixelSize) + (i + 1) * colorButtonsHorizontalMargin 
                            + i * colorButtonSize.Width,
                            (j + 1) * colorButtonsVerticalMargin + colorButtonSize.Height * j),
                        BackColor = Color.FromArgb(255, random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
                    };

                    button.Click += (sender, args) =>
                    {
                        selectedColor = button.BackColor;
                    };

                    button.MouseDoubleClick += (sender, args) =>
                    {
                        var dialog = new ColorDialog();
                        dialog.AnyColor = true;
                        if (dialog.ShowDialog() != DialogResult.OK) return;
                        selectedColor = dialog.Color;
                        button.BackColor = dialog.Color;
                    };

                    button.MouseClick += (sender, args) =>
                    {
                        if (args.Button != MouseButtons.Right) return;
                        button.BackColor = Color.Transparent;
                        selectedColor = Color.Transparent;
                    };

                    Controls.Add(button);
                }
            }
        }
    }
}
