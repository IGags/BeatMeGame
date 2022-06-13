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
        public Bitmap Icon { get; }
        public Label Label { get; }

        public BitmapVisualizationPanel(string name, Bitmap bitmap)
        {
            Icon = bitmap;
            SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true);
            var icon = new PictureBox()
            {
                Location = Point.Empty,
                Size = new Size(100, 60),
                Image = bitmap,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            Label = new Label()
            {
                Text = name,
                Font = new Font(FontFamily.GenericMonospace, 8),
                Location = new Point(0, icon.Bottom),
                Size = new Size(100, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label.MouseHover += (sender, args) =>
            {
                var t = new ToolTip();
                t.SetToolTip(Label, Label.Text);
            };

            Controls.Add(icon);
            Controls.Add(Label);
        }
    }
}
