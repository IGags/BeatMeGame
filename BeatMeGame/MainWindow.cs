using System.Drawing;
using System.Windows.Forms;
using BeatMeGame.EditorView;
using BeatMeGameModel;
using SoundEngineLibrary;

namespace BeatMeGame
{
    class MainWindow : Form, ITerminatable, ISoundProvider, IMainWindow, IFormCreator
    {
        private Settings settingsConfig;
        private Form childForm;
        private Form previousScene;
        private readonly SoundEngine sfxEngine = new SoundEngine(100, 50);
        private readonly SoundEngine musicEngine = new SoundEngine(100, 50);
        private MainEditorFormManager editor;

        public MainWindow()
        {
            Initialize();
            Resize += (sender, args) => { };
        }

        public void RunEditor(LevelSave save)
        {
            editor = new MainEditorFormManager(this, save);
        }

        public void TerminateForm()
        {
            var dialog = new TerminationDialogForm(this);
            var result = dialog.ShowDialog();
            if (result != DialogResult.Yes) return;
            Settings.SaveSettingFile(settingsConfig);
            Close();
        }

        public void CreateChildForm(Form formToCrate)
        {
            childForm = formToCrate;
            childForm.Show();
        }

        public Settings GetSettings()
        {
            return settingsConfig;
        }

        public void ChangeScene(Form previousScene)
        {
            this.previousScene = previousScene;
            foreach (var form in MdiChildren)
            {
                if(form != previousScene) form.Close();
                else form.Hide();
            }
        }

        public void ReestablishScene()
        {
            var child = new MainMenuForm(this);
            child.Show();
            childForm = child;
        }

        public void SetSettings(Settings settings)
        {
            settingsConfig = settings;
            musicEngine.ChangeEngineVolume(settings.MusicVolume, 100);
            sfxEngine.ChangeEngineVolume(settings.SfxVolume, 100);
            Size = settings.FormSize;
            (FormBorderStyle, MaximizeBox, Size) = settingsConfig.ScreenState == ScreenState.FullScreen
                ? (FormBorderStyle.None, true, Screen.PrimaryScreen.Bounds.Size) : (FormBorderStyle.Fixed3D, false, settings.FormSize);
            Location = Point.Empty;
        }
        public SoundEngine GetSfxEngine()
        {
            return sfxEngine;
        }

        public SoundEngine GetMusicEngine()
        {
            return musicEngine;
        }

        private void Initialize()
        {
            settingsConfig = Settings.ParseSettingFile();
            Size = settingsConfig.FormSize;
            musicEngine.ChangeEngineVolume(settingsConfig.MusicVolume, 100);
            sfxEngine.ChangeEngineVolume(settingsConfig.SfxVolume, 100);
            IsMdiContainer = true;
            Text = "BeatMeGame";

            Load += (sender, args) => { OnSizeChanged(args); };

            (FormBorderStyle, MaximizeBox, Size) = settingsConfig.ScreenState == ScreenState.FullScreen
                ? (FormBorderStyle.None, true, Screen.PrimaryScreen.Bounds.Size) : (FormBorderStyle.Fixed3D, false, settingsConfig.FormSize);
            var parent = this;
            var child = new MainMenuForm(parent);
            child.Show();
            childForm = child;

            Closing += (sender, args) =>
            {
                Settings.SaveSettingFile(settingsConfig);
            };
        }
    }
}
