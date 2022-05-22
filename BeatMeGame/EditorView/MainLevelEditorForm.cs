using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BeatMeGameModel;
using BeatMeGameModel.MenuBGModels;

namespace BeatMeGame.EditorView
{
    public class MainLevelEditorForm : Form
    {
        private LevelSave save;
        private EditorSettings settings = EditorSettings.Deserialize();
        public MainLevelEditorForm(Form parent, LevelSave save)
        {
            MdiParent = parent;
            this.save = save;
            Initialize();
        }

        private void Initialize()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.DarkGray;
            DoubleBuffered = true;

            var codeEditor = new CodeEditorPanel(settings.TextSize);


            Load += (sender, args) =>
            {
                Size = new Size(MdiParent.ClientSize.Width - 4, MdiParent.ClientSize.Height - 4);
                Location = Parent.Location;
                codeEditor.Size = new Size(Size.Width / 3, Size.Height / 3);
                codeEditor.Location = new Point(2 * Size.Width / 3, 2 * Size.Height / 3);
            };

            Closing += (sender, args) =>
            {
                EditorSettings.Serialize(settings);
            };

            Controls.Add(codeEditor);
        }
    }
}
