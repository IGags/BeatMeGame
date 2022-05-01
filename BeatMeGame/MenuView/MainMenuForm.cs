using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BeatMeGame.MenuBGWrappers;
using BeatMeGameModel;
using BeatMeGameModel.MenuBGModels;
using SoundEngineLibrary;
using Timer = System.Windows.Forms.Timer;
using NAudio.Wave;

namespace BeatMeGame
{
    public class MainMenuForm : Form
    {
        private Timer updateTimer;
        private Form child;
        private Queue<string> songQueue;
        private string treadName;
        private SoundEngine musicEngine;
        private IMenuBGWrapper wrapper;

        public MainMenuForm(Form parent)
        {
            Initialize(parent);
            updateTimer.Enabled = true;
        }

        private void Initialize(Form parent)
        {
            DoubleBuffered = true;
            MdiParent = parent;
            Size = parent.ClientSize;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.Black;
            Enabled = false;
            songQueue = GetShuffledMenuSongQueue();
            musicEngine = ((ISoundProvider)parent).GetMusicEngine();

            updateTimer = new Timer { Interval = 16 };
            updateTimer.Tick += (sender, args) =>
            {
                CheckAndChangeTrack();
                Invalidate();
            };

            Load += (sender, args) =>
            {
                OnSizeChanged(args);
                CheckAndChangeTrack();
                wrapper = new StarfieldWrapper(ClientSize.Width, ClientSize.Height, musicEngine, treadName);
                Location = Point.Empty;
            };

            Resize += (sender, args) =>
            {
                CheckAndChangeTrack();
                wrapper = new StarfieldWrapper(ClientSize.Width, ClientSize.Height, musicEngine, treadName);
                Invalidate();
            };

            MdiParent.Resize += (sender, args) =>
            {
                Size = new Size(MdiParent.ClientSize.Width - 4, MdiParent.ClientSize.Height - 4);
            };

            Closing += (sender, args) =>
            {
                musicEngine.TerminateTread(treadName);
                updateTimer.Enabled = false;
            };

            child = new MenuListForm(ParentForm);
            child.Show();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            graphics.CompositingQuality = CompositingQuality.GammaCorrected;
            wrapper.Redraw(graphics);
        }

        private Queue<string> GetShuffledMenuSongQueue()
        {
            if (!Directory.Exists("Resources")) Directory.CreateDirectory("Resources");
            var menuSongs = Directory.GetFiles("Resources", "*.mp3", SearchOption.TopDirectoryOnly);
            var random = new Random();
            for (var i = menuSongs.Length - 1; i > 1; i--)
            {
                var j = random.Next(i + 1);
                (menuSongs[j], menuSongs[i]) = (menuSongs[i], menuSongs[j]);
            }

            var outQueue = new Queue<string>();
            foreach (var song in menuSongs)
            {
                outQueue.Enqueue(song);
            }

            return outQueue;
        }

        private void CheckAndChangeTrack()
        {
            if (songQueue.Count == 0 && GetShuffledMenuSongQueue().Count == 0) return;
            if (songQueue.Count == 0) songQueue = GetShuffledMenuSongQueue();
            if (treadName == null)
            {
                treadName = musicEngine
                    .CreateTread(ThreadOptions.StaticThread, songQueue.Dequeue(), FFTExistance.Exist);
            }
            if (musicEngine.GetTread(treadName).OutputDevice.PlaybackState == PlaybackState.Playing) return;
            GC.Collect();
            musicEngine.GetTread(treadName).ChangeTrack(songQueue.Dequeue());
        }
    }
}
