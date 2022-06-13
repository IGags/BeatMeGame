using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BeatMeGame.GameView;
using BeatMeGame.LevelEditorView;
using BeatMeGame.MenuView;
using BeatMeGameModel;
using BeatMeGameModel.EitorModels;
using BeatMeGameModel.Exceptions;
using BeatMeGameModel.GameModels;
using BeatMeGameModel.IOWorkers;
using BeatMeGameModel.LevelEditorModels;
using SoundEngineLibrary;

namespace BeatMeGame.EditorView
{
    public class MainLevelEditorForm : Form
    {
        private readonly EditorSettings settings;
        private readonly EditorModel model;
        private readonly Dictionary<string, Bitmap> assets;
        private Timer UpdateTimer;
        private BeatEngine beatEngine;
        private SoundEngineTread workTread;
        private Bitmap playerBitmap;
        private Game engine;
        public MainLevelEditorForm(Form parent, LevelSave save)
        {
            Select();
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
            assets = BitmapIOParser.ParseSavedBitmaps(model.Save.LevelName);
            Initialize();
            this.KeyPreview = true;
        }

        private void Initialize()
        {
            TabStop = false;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.DarkGray;
            DoubleBuffered = true;
            SetStyle(ControlStyles.StandardDoubleClick, true);
            SetStyle(ControlStyles.StandardClick, true);
            var margin = ClientSize.Height / 70;

            var playTestButton = new BoolButton(false)
            {
                Text = "Тестировать"
            };

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

            var bitmapList = new AssetList(assets)
            {
                BackColor = Color.Gray
            };

            var settingsButton = new Button()
            {
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkGray,
                Text = "Настройки"
            };

            var paintPanel = new PaintPanel();

            var musicEditorButton = new Button()
            {
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkGray,
                Text = "В редактор музыки"
            };

            var gameVisualizationPanel = new GamePlayTestPanel()
            {
                BackColor = Color.DimGray
            };

            var exitButton = new Button()
            {
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkGray,
                Text = "Выйти"
            };

            scriptCreationButton.Click += (sender, args) =>
            {
                var dialog = new ScriptCreationDialogForm(model.Scripts.Keys.ToArray());
                if (dialog.ShowDialog() != DialogResult.OK) return;
                model.Scripts[dialog.ScriptName] = Array.Empty<string>();
                scriptList.Redraw(model.Scripts.Keys.ToArray());
            };

            playTestButton.Click += (sender, args) =>
            {
                if (!playTestButton.IsActivated)
                {
                    try
                    {
                        FreezeControls(playTestButton);
                        var bitmaps = assets.ToDictionary(value => value.Key, value => BitmapCroper.Crop(value.Value));
                        if (scriptList.SelectedButton != null)
                            model.Scripts[scriptList.SelectedButton] = codeEditor.CodeEditor.Text.Split('\n');
                        LevelAssetsParser.PackScripts(model.Scripts, model.Save.LevelName);
                        var scripts = model.Scripts.ToDictionary(value => value.Key, value => new GameObjectScript(value.Value));
                        using (var bmpFromFile = (Bitmap)Image.FromFile("Resources\\playerStub.png"))
                        {
                            playerBitmap = new Bitmap(bmpFromFile);
                        }
                        UpdateTimer = new Timer()
                        {
                            Enabled = false,
                            Interval = 16
                        };
                        workTread = new SoundEngineTread("Levels\\" + model.Save.LevelName + "\\" + model.Save.Manifest.SongName,
                            ThreadOptions.StaticThread,
                            FFTExistance.Exist);
                        beatEngine = new BeatEngine(
                            workTread, model.Save.Beat, model.Save.Manifest.DetectionType, TimeSpan.Zero);
                        engine = new Game(scripts["Main"], scripts, new PlayerStub(500, 100),
                            beatEngine, null);
                        var testStopwatch = new Stopwatch();
                        gameVisualizationPanel.Bitmaps = bitmaps;
                        gameVisualizationPanel.PlayerBitmap = playerBitmap;
                        engine.IsPlayerCollisionEnabled = true;
                        engine.PlayerDeath += () =>
                        {
                            playTestButton.PerformClick();
                        };
                        testStopwatch.Start();
                        engine.GameStart();
                        UpdateTimer.Tick += (snd, arg) =>
                        {
                            
                            engine.GetGameStateByTime(testStopwatch.Elapsed);
                            gameVisualizationPanel.Redraw(engine.gameObjects, engine.Player1);
                        };
                        UpdateTimer.Enabled = true;

                    }
                    catch (Exception e)
                    {
                        if(UpdateTimer != null)UpdateTimer.Enabled = false;
                        beatEngine?.Stop();
                        workTread?.ChangePlaybackState();
                        workTread?.Dispose();
                        UnfreezeControls();
                        new EditorExceptionDialogForm(e.Message).ShowDialog();
                    }
                }
                else
                {
                    UpdateTimer.Enabled = false;
                    beatEngine.Stop();
                    workTread.ChangePlaybackState();
                    workTread.Dispose();
                    UnfreezeControls();
                }
            };

            scriptList.ButtonChanged += () =>
            {
                if (scriptList.PreviousButton != null)
                    model.Scripts[scriptList.PreviousButton] = codeEditor.CodeEditor.Text.Split('\n');
                codeEditor.CodeEditor.Text = string.Join("\n", model.Scripts[scriptList.SelectedButton]);
            };

            bitmapList.ActiveBitmapUpdated += () =>
            {
                paintPanel.UpdateBitmap(bitmapList.ActiveBitmap);
            };

            musicEditorButton.Click += (sender, args) =>
            {
                var creator = (IFormCreator)MdiParent;
                model.Save.Manifest.EditorType = EditorType.Music;
                LevelSavePacker.PackSave(model.Save);
                creator.CreateChildForm(new MusicEditorForm(MdiParent, model.Save));
                Close();
            };

            exitButton.Click += (sender, args) =>
            {
                var dialog = new EditorExitDialogForm(this);
                var result = dialog.ShowDialog();
                switch (result)
                {
                    case DialogResult.OK:
                    {
                        var creator = (IFormCreator)MdiParent;
                        creator.ReestablishScene();
                        Close();
                        break;
                    }
                    case DialogResult.Yes:
                    {
                        var creator = (IFormCreator)MdiParent;
                        creator.ReestablishScene();
                        Close();
                        break;
                    }
                }
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
                scriptList.Size = new Size(Size.Width / 5, 2 * Size.Height / 3);
                scriptList.Location = new Point(4 * Size.Width / 5, 0);
                scriptDeletionButton.Location = new Point(scriptList.Left - margin - settingsButton.Width,
                    codeEditor.Top - margin - settingsButton.Height);
                scriptDeletionButton.Size = settingsButton.Size;
                scriptCreationButton.Size = settingsButton.Size;
                scriptCreationButton.Location = new Point(scriptDeletionButton.Left,
                    scriptDeletionButton.Top - margin - settingsButton.Height);
                paintPanel.Location = new Point(0, codeEditor.Top);
                paintPanel.Size = new Size(Width / 3, Height / 3);
                settingsButton.Location = new Point(scriptCreationButton.Left, ClientSize.Height / 50);
                bitmapList.Size = paintPanel.Size;
                bitmapList.Location = new Point(paintPanel.Right, paintPanel.Top);
                playTestButton.Size = settingsButton.Size;
                playTestButton.Location = new Point(settingsButton.Left, settingsButton.Bottom + margin);
                gameVisualizationPanel.Size = new Size(playTestButton.Left - margin, paintPanel.Top);
                musicEditorButton.Size = settingsButton.Size;
                musicEditorButton.Location = new Point(playTestButton.Left, playTestButton.Bottom + margin);
                exitButton.Size = settingsButton.Size;
                exitButton.Location = new Point(musicEditorButton.Left, musicEditorButton.Bottom + margin);
            };

            Closing += (sender, args) =>
            {
                EditorSettings.Serialize(settings);
                LevelSavePacker.PackSave(model.Save);
                beatEngine?.Stop();
                workTread?.ChangePlaybackState();
                workTread?.Dispose();
                if (scriptList.SelectedButton != null)
                    model.Scripts[scriptList.SelectedButton] = codeEditor.CodeEditor.Text.Split('\n');
                LevelAssetsParser.PackScripts(model.Scripts, model.Save.LevelName); 
                //LevelFolderWorker.ClearLevelFolder(model.Save.LevelName + "\\Assets");
                BitmapIOParser.PackBitmapDictionary(assets, model.Save);
            };

            Controls.Add(codeEditor);
            Controls.Add(settingsButton);
            Controls.Add(scriptList);
            Controls.Add(scriptCreationButton);
            Controls.Add(scriptDeletionButton);
            Controls.Add(paintPanel);
            Controls.Add(bitmapList);
            Controls.Add(playTestButton);
            Controls.Add(gameVisualizationPanel);
            Controls.Add(musicEditorButton);
            Controls.Add(exitButton);
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

        private void FreezeControls(params Control[] aliveControls)
        {
            foreach (var control in Controls)
            {
                var ctrl = (Control)control;
                if (!aliveControls.Contains(ctrl)) ctrl.Enabled = false;
            }
        }

        private void UnfreezeControls()
        {
            foreach (var control in Controls)
            {
                ((Control)control).Enabled = true;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (engine == null) return;
            switch (e.KeyCode)
            {
                case Keys.A:
                    engine.PlayerMove(false, Directions.Left);
                    break;
                case Keys.D:
                    engine.PlayerMove(false, Directions.Right);
                    break;
                case Keys.W:
                    engine.PlayerMove(false, Directions.Up);
                    break;
                case Keys.S:
                    engine.PlayerMove(false, Directions.Down);
                    break;
            }
        }
    }
}
