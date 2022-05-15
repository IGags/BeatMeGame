using System;
using NAudio.Utils;
using NAudio.Wave;

namespace SoundEngineLibrary
{
    public enum FFTExistance
    {
        Exist,
        DoesntExist
    }

    public class SoundEngineTread
    {
        public Mp3FileReader CurrentTrack { get; private set; }
        public WaveChannel32 WaveChannel32 { get; private set; }
        public string FullFilePath { get; private set; }
        public WaveOutEvent OutputDevice { get; private set; }
        public TimeSpan MaxSongDuration { get; private set; }
        public FFT TrackFFT { get; private set; }
        public ThreadOptions TreadType { get; }
        private TimeSpan startPositionTimeSpan = TimeSpan.Zero;

        /// <summary>
        /// Проигрывает файл по указаному пути
        /// </summary>
        /// <param name="fullPath">Путь до файла</param>
        /// <param name="treadType">Тип создаваемого потока</param>
        /// <param name="existence">Существование анализаторов бита и спектра</param>
        public SoundEngineTread(string fullPath, ThreadOptions treadType, FFTExistance existence)
        {
            FullFilePath = fullPath;
            TreadType = treadType;
            OutputDevice = new WaveOutEvent();
            CurrentTrack = new Mp3FileReader(fullPath);
            WaveChannel32 = new WaveChannel32(CurrentTrack);
            TrackFFT = existence == FFTExistance.Exist ? new FFT(fullPath) : null;
            GC.Collect();
            OutputDevice.Init(WaveChannel32);
            OutputDevice.Play();
            MaxSongDuration = CurrentTrack.TotalTime;
        }
        public SoundEngineTread(string fullPath, ThreadOptions treadType, FFTExistance existence, int soundPower, int maxPower)
            : this(fullPath, treadType, existence)
        {
            ChangeVolume(soundPower, maxPower);
        }

        /// <summary>
        /// Меняет проигрываемый файл по указанному пути
        /// </summary>
        /// <param name="fullPath">Путь до файла</param>
        public void ChangeTrack(string fullPath)
        {
            if (TreadType == ThreadOptions.StaticThread)
            {
                Dispose();
                FullFilePath = fullPath;
                if (OutputDevice != null) OutputDevice.Stop();
                else OutputDevice = new WaveOutEvent();
                CurrentTrack = new Mp3FileReader(fullPath);
                WaveChannel32 = new WaveChannel32(CurrentTrack);
                TrackFFT = TrackFFT != null ? new FFT(fullPath) : null;
                GC.Collect();
                OutputDevice.Init(WaveChannel32);
                OutputDevice.Play();
                MaxSongDuration = CurrentTrack.TotalTime;
            }
            else throw new InvalidOperationException("Cannot change track in temporal tread");
        }

        /// <summary>
        /// Меняет состояние потока
        /// </summary>
        public void ChangePlaybackState()
        {
            if (OutputDevice != null)
            {
                switch (OutputDevice.PlaybackState)
                {
                    case PlaybackState.Playing:
                        OutputDevice.Pause();
                        break;
                    case PlaybackState.Paused:
                        OutputDevice.Play();
                        break;
                    case PlaybackState.Stopped:
                        OutputDevice.Play();
                        break;
                }
            }
        }

        /// <summary>
        /// Изменяет уровень громкости потока относительно максимума
        /// </summary>
        /// <param name="value">Требуемое значение</param>
        /// <param name="max">Максимально возможное значение</param>
        public void ChangeVolume(int value, int max)
        {
            if (value > max || value < 0)
                throw new ArgumentException("Значение выше максимального или меньше нуля");
            WaveChannel32.Volume = (float)value / max;
        }

        /// <summary>
        /// Измерят время В СЕКУНДАХ текущего аудио потока
        /// </summary>
        /// <returns>время</returns>
        public TimeSpan MeasureTime()
        {
            if(OutputDevice == null) return TimeSpan.Zero;
            return OutputDevice.GetPositionTimeSpan() + startPositionTimeSpan;
        }

        public void Dispose()
        {
            OutputDevice.Stop();
            TrackFFT?.Dispose();
            WaveChannel32?.Close();
            CurrentTrack?.Close();
            WaveChannel32?.Dispose();
            CurrentTrack?.Dispose();
            OutputDevice?.Dispose();
        }

        /// <summary>
        /// Выбирает время воспроизведения В СЕКУНДАХ
        /// </summary>
        /// <param name="position">время воспроизведения</param>
        public void ChangePlayingPosition(int position)
        {
            if (OutputDevice == null) return;
            OutputDevice.Stop();
            if (OutputDevice.GetPosition() != 0)
            {
                WaveChannel32?.Close();
                CurrentTrack?.Close();
                WaveChannel32?.Dispose();
                CurrentTrack?.Dispose();
                OutputDevice.Dispose();
            }
            CurrentTrack = new Mp3FileReader(FullFilePath);
            CurrentTrack.CurrentTime = new TimeSpan(0, 0, position);
            startPositionTimeSpan = CurrentTrack.CurrentTime;
            WaveChannel32 = new WaveChannel32(CurrentTrack);
            OutputDevice.Init(WaveChannel32);
            OutputDevice.Pause();
        }
    }
}