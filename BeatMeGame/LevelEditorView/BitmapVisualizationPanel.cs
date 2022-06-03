using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BeatMeGame.LevelEditorView
{
    public class BitmapVisualizationPanel : Panel
    {
        public static Size DefaultSize { get; } = new Size(100, 80);

        public BitmapVisualizationPanel(string name, Bitmap bitmap)
        {
            var icon = new PictureBox()
            {
                Location = Point.Empty,
                Size = new Size(100, 60),
                Image = bitmap,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            var label = new Label()
            {
                Text = name,
                Font = new Font(FontFamily.GenericMonospace, 8),
                Location = new Point(0, icon.Bottom),
                Size = new Size(100, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Controls.Add(icon);
            Controls.Add(label);
        }
    }
}
