﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;
using SoundEngineLibrary;

namespace BeatMeGame.MenuView
{
    public class MenuSoundEngine
    {
        public SoundEngine Engine { get; }
        public string TreadName { get; private set; }

        private Queue<string> musicQueue = new Queue<string>();
        private readonly Timer updateTimer = new Timer() { Enabled = true, Interval = 16 };
        public MenuSoundEngine(SoundEngine engine)
        {
            Engine = engine;
            CheckAndChangeTrack();
            updateTimer.Tick += (sender, args) => { CheckAndChangeTrack(); };
        }

        public void DeleteTread()
        {
            Engine.TerminateTread(TreadName);
            TreadName = null;
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
            if (musicQueue.Count == 0 && GetShuffledMenuSongQueue().Count == 0) return;
            if (musicQueue.Count == 0) musicQueue = GetShuffledMenuSongQueue();
            if (TreadName == null)
            {
                TreadName = Engine
                    .CreateTread(ThreadOptions.StaticThread, musicQueue.Dequeue(), FFTExistance.Exist);
            }

            if (Engine.GetTread(TreadName).OutputDevice.PlaybackState == PlaybackState.Playing) return;
            GC.Collect();
            Engine.GetTread(TreadName).ChangeTrack(musicQueue.Dequeue());
        }
    }
}
