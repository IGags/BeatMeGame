using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BeatMeGame.LevelEditorView
{
    class AssetList : Panel
    {
        public AssetList(Dictionary<string, Bitmap> assets) 
        {
            VScroll = true;
            AutoScroll = true;
            Visualize(assets);
            BorderStyle = BorderStyle.FixedSingle;
            SizeChanged += (sender, args) =>
            {
                Visualize(assets);
            };
        }

        public void Visualize(Dictionary<string, Bitmap> assets)
        {
            Controls.Clear();
            var margin = 30;
            var elementCount = assets.Count;
            var elementsPerRow = Width / (BitmapVisualizationPanel.DefaultSize.Width + margin);
            var keys = assets.Keys.ToArray();
            var position = 0;
            var row = 0;
            while (elementCount > position)
            {
                var rowCount = Math.Min(elementsPerRow, elementCount - position);
                for (int i = 0; i < rowCount; i++)
                {
                    var panel = new BitmapVisualizationPanel(keys[position], assets[keys[position]])
                    {
                        Location = new Point(margin * (i + 1) + BitmapVisualizationPanel.DefaultSize.Width * i, 
                            margin * (row + 1) + BitmapVisualizationPanel.DefaultSize.Height * row),
                        Size = BitmapVisualizationPanel.DefaultSize
                    };
                    Controls.Add(panel);
                    position++;
                }

                row++;
            }
        }
    }
}
