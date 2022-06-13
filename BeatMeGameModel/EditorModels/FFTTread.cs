using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using SoundEngineLibrary;

namespace BeatMeGameModel.EitorModels
{
    public class FFTTread
    {
        public FFT FFT { get; private set; }
        public Stopwatch ElapsedStopwatch { get; private set; } = new Stopwatch();
        public TimeSpan Position { get; set; }
        private Timer autoKillTimer = new Timer(10);
        public event Action Closing; 

        public FFTTread(string fullFilePath, TimeSpan position, TimeSpan length)
        {
            Position = position;
            FFT = new FFT(fullFilePath);

            autoKillTimer.Elapsed += (sender, args) =>
            {
                if (length <= ElapsedStopwatch.Elapsed) Closing();
                Stop();
            };
        }

        public void Run()
        {
            ElapsedStopwatch.Start();
        }

        public TimeSpan GetTime()
        {
            return ElapsedStopwatch.Elapsed + Position;
        }

        public void Pause()
        {
            ElapsedStopwatch.Stop();
        }

        public void Stop()
        {
            ElapsedStopwatch.Stop();
            FFT.Dispose();
        }
    }
}
