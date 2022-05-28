using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BeatMeGameModel;
using BeatMeGameModel.IOWorkers;

namespace BeatMeGame.EditorView
{
    public class MainLevelEditorForm : Form
    {
        private LevelSave save;
        private readonly EditorSettings settings;

        public MainLevelEditorForm(Form parent, LevelSave save)
        {
            try
            {
                settings = EditorSettings.Deserialize();
            }
            catch (SerializationException)
            {
                settings = ChangeSettingsForcibly();
            }

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
            var objectList = new EditorElementsPanel();

            var settingsButton = new Button()
            {
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkGray,
                Text = "Настройки"
            };

            settingsButton.Click += (sender, args) =>
            {
                var settingsEditionForm = new EditorSettingsForm(settings);
                settingsEditionForm.ShowDialog();
                Controls.Clear();
                Initialize();
                OnLoad(EventArgs.Empty);
            };

            Load += (sender, args) =>
            {
                Size = new Size(MdiParent.ClientSize.Width - 4, MdiParent.ClientSize.Height - 4);
                Location = Parent.Location;
                codeEditor.Size = new Size(Size.Width / 3, Size.Height / 3);
                codeEditor.Location = new Point(2 * Size.Width / 3, 2 * Size.Height / 3);
                settingsButton.Size = new Size(ClientSize.Width / 30, ClientSize.Width / 30);
                settingsButton.Location = new Point(ClientSize.Width / 50, ClientSize.Height / 50);
                objectList.Size = new Size(Size.Width / 5, 2 * Size.Height / 3);
                objectList.Location = new Point(4 * Size.Width / 5, 0);
            };

            Closing += (sender, args) =>
            {
                EditorSettings.Serialize(settings);
            };

            Controls.Add(codeEditor);
            Controls.Add(settingsButton);
            Controls.Add(objectList);
        }

        private EditorSettings ChangeSettingsForcibly()
        {
            while (true)
            {
                var settings = new EditorSettings();
                if (new EditorSettingsForm(settings).ShowDialog() == DialogResult.OK)
                {
                    return settings;
                }
            }
        }
    }
}
