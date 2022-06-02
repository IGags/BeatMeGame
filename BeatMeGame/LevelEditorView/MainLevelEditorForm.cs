using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BeatMeGame.LevelEditorView;
using BeatMeGameModel;
using BeatMeGameModel.IOWorkers;
using BeatMeGameModel.LevelEditorModels;

namespace BeatMeGame.EditorView
{
    public class MainLevelEditorForm : Form
    {
        private readonly EditorSettings settings;
        private readonly EditorModel model;

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
            model = new EditorModel(save, LevelAssetsParser.ParseScripts(save.LevelName));
            Initialize();
        }

        private void Initialize()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.DarkGray;
            DoubleBuffered = true;
            var margin = ClientSize.Height / 70;

            var codeEditor = new CodeEditorPanel(settings.TextSize);
            var scriptList = new ScriptsPanel(model.Scripts.Keys.ToArray());
            var scriptDeletionButton = new Button()
            {
                Text = "Удалить Скрипт",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkGray
            };
            var scriptCreationButton = new Button()
            {
                Text = "Создать Скрипт",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkGray
            };

            var settingsButton = new Button()
            {
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkGray,
                Text = "Настройки"
            };

            scriptCreationButton.Click += (sender, args) =>
            {
                var dialog = new ScriptCreationDialogForm(model.Scripts.Keys.ToArray());
                if (dialog.ShowDialog() != DialogResult.OK) return;
                model.Scripts[dialog.ScriptName] = Array.Empty<string>();
                scriptList.Redraw(model.Scripts.Keys.ToArray());
            };

            scriptList.ButtonChanged += () =>
            {
                if (scriptList.PreviousButton != null)
                    model.Scripts[scriptList.PreviousButton] = codeEditor.CodeEditor.Text.Split('\n');
                codeEditor.CodeEditor.Text = string.Join("\n", model.Scripts[scriptList.SelectedButton]);
            };

            settingsButton.Click += (sender, args) =>
            {
                var settingsEditionForm = new EditorSettingsForm(settings);
                settingsEditionForm.ShowDialog();
                if (scriptList.PreviousButton != null)
                    model.Scripts[scriptList.SelectedButton] = codeEditor.CodeEditor.Text.Split('\n');
                scriptList.PreviousButton = null;
                scriptList.SelectedButton = null;
                Controls.Clear();
                Initialize();
                OnLoad(EventArgs.Empty);
            };

            scriptDeletionButton.Click += (sender, args) =>
            {
                if(scriptList.SelectedButton == "Main") return;
                model.Scripts.Remove(scriptList.SelectedButton);
                scriptList.PreviousButton = null;
                scriptList.SelectedButton = null;
                scriptList.Redraw(model.Scripts.Keys.ToArray());
                codeEditor.Text = "";
            };

            Load += (sender, args) =>
            {
                Size = new Size(MdiParent.ClientSize.Width - 4, MdiParent.ClientSize.Height - 4);
                Location = Parent.Location;
                codeEditor.Size = new Size(Size.Width / 3, Size.Height / 3);
                codeEditor.Location = new Point(2 * Size.Width / 3, 2 * Size.Height / 3);
                settingsButton.Size = new Size(ClientSize.Width / 30, ClientSize.Width / 30);
                settingsButton.Location = new Point(ClientSize.Width / 50, ClientSize.Height / 50);
                scriptList.Size = new Size(Size.Width / 5, 2 * Size.Height / 3);
                scriptList.Location = new Point(4 * Size.Width / 5, 0);
                scriptDeletionButton.Location = new Point(scriptList.Left - margin - settingsButton.Width,
                    codeEditor.Top - margin - settingsButton.Height);
                scriptDeletionButton.Size = settingsButton.Size;
                scriptCreationButton.Size = settingsButton.Size;
                scriptCreationButton.Location = new Point(scriptDeletionButton.Left,
                    scriptDeletionButton.Top - margin - settingsButton.Height);
            };

            Closing += (sender, args) =>
            {
                EditorSettings.Serialize(settings);
                LevelSavePacker.PackSave(model.Save);
                if (scriptList.SelectedButton != null)
                    model.Scripts[scriptList.SelectedButton] = codeEditor.CodeEditor.Text.Split('\n');
                LevelAssetsParser.PackScripts(model.Scripts, model.Save.LevelName);
            };

            Controls.Add(codeEditor);
            Controls.Add(settingsButton);
            Controls.Add(scriptList);
            Controls.Add(scriptCreationButton);
            Controls.Add(scriptDeletionButton);
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
