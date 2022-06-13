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
        public event Action ActiveBitmapUpdated;
        public Bitmap ActiveBitmap { get; private set; }
        private ToolStripMenuItem createItem;
        public AssetList(Dictionary<string, Bitmap> assets) 
        {
            SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true);
            VScroll = true;
            AutoScroll = true;
            Visualize(assets);
            BorderStyle = BorderStyle.FixedSingle;
            createItem = new ToolStripMenuItem("Создать");
            createItem.Click += (sender, args) =>
            {
                var nameDialog = new ScriptCreationDialogForm(assets.Keys.ToArray());
                if (nameDialog.ShowDialog() != DialogResult.OK) return;
                var name = nameDialog.ScriptName;
                assets[name] = new Bitmap(32, 32);
                Visualize(assets);
            };
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
                        Size = BitmapVisualizationPanel.DefaultSize,
                    };

                    foreach (var control in panel.Controls)
                    {
                        var ctr = (Control)control;
                        ctr.MouseDoubleClick += (sender, args) =>
                        {
                            Visualize(assets);
                            ActiveBitmap = panel.Icon;
                            ActiveBitmapUpdated?.Invoke();
                        };

                        ctr.MouseClick += (sender, args) =>
                        {
                            if (args.Button != MouseButtons.Right) return;
                            var renameItem = new ToolStripMenuItem("Переименовать");
                            var deleteItem = new ToolStripMenuItem("Удалить");
                            var context = new ContextMenuStrip();
                            context.Items.AddRange(new []{renameItem, deleteItem, createItem});
                            ContextMenuStrip = context;
                            renameItem.ShowDropDown();
                            renameItem.Click += (snd, arg) =>
                            {
                                var nameDialog = new ScriptCreationDialogForm(assets.Keys.ToArray());
                                if(nameDialog.ShowDialog() != DialogResult.OK) return;
                                var name = nameDialog.ScriptName;
                                if(!assets.ContainsKey(panel.Label.Text)) return;
                                var asset = assets[panel.Label.Text];
                                assets.Remove(panel.Label.Text);
                                assets[name] = asset;
                                Visualize(assets);
                            };

                            deleteItem.Click += (snd, arg) =>
                            {
                                assets.Remove(panel.Label.Text);
                                Visualize(assets);
                            };
                        };
                    }

                    Controls.Add(panel);
                    position++;
                }

                row++;
            }
        }
    }
}
