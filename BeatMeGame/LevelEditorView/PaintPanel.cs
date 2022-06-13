using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BeatMeGame.LevelEditorView
{
    class PaintPanel : Panel
    {
        public Bitmap EditorBitmap { get; private set; } = new Bitmap(32, 32);
        private float pixelSize;
        private Color selectedColor = Color.Black;
        private readonly CheckBox gridVisualizationCheckBox;

        public PaintPanel()
        {
            var colorButtonsHorizontalMargin = EditorBitmap.Width * pixelSize;
            pixelSize = (float)Height / EditorBitmap.Height;
            DoubleBuffered = true;
            InitializeColorButtons();

            gridVisualizationCheckBox = new CheckBox()
            {
                Text = "Рисовать сетку"
            };

            gridVisualizationCheckBox.CheckStateChanged += (sender, args) =>
            {
                Invalidate();
            };

            Click += (sender, args) =>
            {
                var mouse = (MouseEventArgs)args;
                if(mouse.X > ClientSize.Height) return;
                EditorBitmap.SetPixel(EditorBitmap.Width * mouse.X / ClientSize.Height, EditorBitmap.Height * mouse.Y / ClientSize.Height, selectedColor);
                Invalidate();
            };
            SizeChanged += (sender, args) =>
            {
                pixelSize = (float)Height / EditorBitmap.Height;
                gridVisualizationCheckBox.Location =
                    new Point((int)(EditorBitmap.Width * pixelSize) + 30, 9 * Height / 10);
                InitializeColorButtons();
            };

            Controls.Add(gridVisualizationCheckBox);
        }

        public void UpdateBitmap(Bitmap bitmap)
        {
            EditorBitmap = bitmap;
            pixelSize = (float)Height / EditorBitmap.Height;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.FillRectangle(new SolidBrush(Color.Gray), new Rectangle(0, 0, Width, Height));
            if(gridVisualizationCheckBox.Checked) DrawGrid(e.Graphics);
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.DrawImage(EditorBitmap, new Rectangle(0, 0, this.Height, this.Height)); 
        }

        private void DrawGrid(Graphics g)
        {
            var pen = new Pen(Color.DarkGreen, 0.1f);
            for (int i = 1; i < EditorBitmap.Height; i++)
            {
                g.DrawLine(pen, i * pixelSize, 0, i * pixelSize, Height);
            }

            for (int i = 1; i < EditorBitmap.Width; i++)
            {
                g.DrawLine(pen, 0, i * pixelSize, Height, i * pixelSize);
            }
        }

        private void InitializeColorButtons()
        {
            var verticalCount = 12;
            var horizontalCount = 15;
            Controls.Clear();
            Controls.Add(gridVisualizationCheckBox);
            var colorButtonSize = new Size(Width / 40, Width / 40);
            var colorButtonsHorizontalMargin = (int)(Width -  EditorBitmap.Width * pixelSize - horizontalCount * colorButtonSize.Width) / (horizontalCount + 1);
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
                        Location = new Point((int)(EditorBitmap.Width * pixelSize) + (i + 1) * colorButtonsHorizontalMargin 
                            + i * colorButtonSize.Width,
                            (j + 1) * colorButtonsVerticalMargin + colorButtonSize.Height * j),
                        BackColor = Color.FromArgb(0, random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
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
