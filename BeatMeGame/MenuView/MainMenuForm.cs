using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BeatMeGame.Interfaces;
using BeatMeGame.MenuBGWrappers;
using BeatMeGame.MenuView;
using BeatMeGameModel;
using BeatMeGameModel.IOWorkers;
using SoundEngineLibrary;
using Timer = System.Windows.Forms.Timer;
using NAudio.Wave;

namespace BeatMeGame
{
    public class MainMenuForm : Form, IStateEditor, ISoundPlayer
    {
        public MenuStateMachine StateMachine { get; set; }
        public MenuSoundEngine MusicEngine { get; private set; }

        private Timer updateTimer;
        private IMenuBGWrapper wrapper;

        public MainMenuForm(Form parent, MenuStateMachine stateMachine = null)
        {
            AllowTransparency = true;
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            Initialize(parent, stateMachine);
            updateTimer.Enabled = true;
        }

        private void Initialize(Form parent, MenuStateMachine stateMachine)
        {
            DoubleBuffered = true;
            MdiParent = parent;
            Size = parent.ClientSize;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.Black;
            StateMachine = stateMachine ?? new MenuStateMachine();
            StateMachine.StateChanged += VisualizeState;
            MusicEngine = new MenuSoundEngine(((ISoundProvider)parent).GetMusicEngine());

            updateTimer = new Timer { Interval = 16 };
            updateTimer.Tick += (sender, args) =>
            {
                Invalidate();
            };

            Load += (sender, args) =>
            {
                OnSizeChanged(args);
                wrapper = new StarfieldWrapper(ClientSize.Width, ClientSize.Height, MusicEngine);
                Location = Point.Empty;
            };

            Resize += (sender, args) =>
            {
                wrapper = new StarfieldWrapper(ClientSize.Width, ClientSize.Height, MusicEngine);
                Invalidate();
            };

            MdiParent.Resize += (sender, args) =>
            {
                if(MdiParent == null) Close();
                else Size = new Size(MdiParent.ClientSize.Width - 5, MdiParent.ClientSize.Height - 5);
            };

            Closing += (sender, args) =>
            {
                updateTimer.Enabled = false;
                MusicEngine.PauseTread();
            };

            VisibleChanged += (sender, args) =>
            {
                if (!Visible)
                {
                    MusicEngine.PauseTread();
                    return;
                }
                Location = Point.Empty;
                MusicEngine.ResumeTread();
            };

            VisualizeState();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            graphics.CompositingQuality = CompositingQuality.GammaCorrected;
            wrapper.Redraw(graphics);
        }

        private void VisualizeState()
        {
            Controls.Clear();
            switch (StateMachine.State)
            {
                case State.MenuList:
                    Controls.Add(new MenuListPanel(this));
                    break;
                case State.Settings:
                    Controls.Add(new SettingsPanel(this));
                    break;
                case State.Play:
                    break;
                case State.Editor:
                    Controls.Add(new MenuRedactorPanel(this));
                    break;
            }
        }
    }
}
