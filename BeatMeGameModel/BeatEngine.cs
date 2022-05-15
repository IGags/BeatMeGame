using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatMeGameModel.BeatVertexes;
using BeatMeGameModel.IOWorkers;
using NAudio.Wave;
using SoundEngineLibrary;
using Timer = System.Timers.Timer;

namespace BeatMeGameModel //TODO: FFT парсер созадёт для себя отдельный поток
{
    public class BeatEngine
    {
        public event Action OnBeat;
        public event Action Clear;
        public event Action Shutdown;
        private readonly Queue<BeatVertex> vertexQueue = new Queue<BeatVertex>();
        private readonly SoundEngineTread workTread;
        private readonly int measureDelay;
        private Thread asyncEventInvoker;
        private readonly BeatDetectionType detectionType;
        private BeatVertex lastVertex;

        public BeatEngine(SoundEngineTread workTread, Dictionary<TimeSpan, BeatVertex> beat, BeatDetectionType detectionType, TimeSpan position)
        {
            this.workTread =
                new SoundEngineTread(workTread.FullFilePath, ThreadOptions.StaticThread, FFTExistance.Exist);
            if (this.workTread.TreadType == ThreadOptions.TemporalThread) throw new ArgumentException("Invalid tread type");
            FillVertexQueue(beat, position);
            this.workTread.ChangePlayingPosition((int)position.TotalSeconds);
            this.workTread.ChangeVolume(0, 1);
            if (this.workTread.OutputDevice.PlaybackState == PlaybackState.Playing) this.workTread.ChangePlaybackState();
            measureDelay = 1000 / (this.workTread.TrackFFT.samplingFrequency / FFT.FFTSize);
            this.detectionType = detectionType;
        }

        public void Play(bool isShutdownAfterSecond )
        {
            if(workTread.OutputDevice.PlaybackState == PlaybackState.Playing) return;
            workTread.ChangePlaybackState();
            asyncEventInvoker = detectionType == BeatDetectionType.FFT
                ? new Thread(ParseFFTQueue)
                : new Thread(ParseBPMQueue);
            asyncEventInvoker.Start();
            if (isShutdownAfterSecond) WaitOneSecondAsync();
        }

        public void Pause()
        {
            asyncEventInvoker.Abort();
            workTread.Dispose();
        }

        private void FillVertexQueue(Dictionary<TimeSpan, BeatVertex> beat, TimeSpan position)
        {
            foreach (var beatVertex in beat.OrderBy(value => value.Key))
            {
                vertexQueue.Enqueue(beatVertex.Value);
            }

            while (vertexQueue.Any() && vertexQueue.Peek().Time <= position)
            {
                if (vertexQueue.Peek().Type == VertexType.FFT
                    || vertexQueue.Peek().Type == VertexType.BPM) lastVertex = vertexQueue.Dequeue();
                else vertexQueue.Dequeue();
            }
        }

        private void ParseFFTQueue()
        {
            
            while (true)
            {
                if (lastVertex != null)
                {
                    workTread.TrackFFT.HighEdge = ((FFTVertex)lastVertex).TopFrequency;
                    workTread.TrackFFT.LowEdge = ((FFTVertex)lastVertex).BotFrequency;
                    workTread.TrackFFT.ThresholdValue = ((FFTVertex)lastVertex).ThresholdValue;
                }
                Clear();
                var isDeletion = false;
                while (vertexQueue.Any() && vertexQueue.Peek().Time < workTread.MeasureTime())
                {
                    var vertex = vertexQueue.Dequeue();
                    switch (vertex.Type)
                    {
                        case VertexType.FFT:
                            workTread.TrackFFT.HighEdge = ((FFTVertex)vertex).TopFrequency;
                            workTread.TrackFFT.LowEdge = ((FFTVertex)vertex).BotFrequency;
                            workTread.TrackFFT.ThresholdValue = ((FFTVertex)vertex).ThresholdValue;
                            workTread.TrackFFT.GetBeatData(workTread.MeasureTime());
                            break;
                        case VertexType.Artificial:
                            OnBeat();
                            break;
                        case VertexType.Deletion:
                            isDeletion = true;
                            break;
                    }
                }

                if (!isDeletion && workTread.TrackFFT.GetBeatData(workTread.MeasureTime()))
                {
                    OnBeat();
                }

                Thread.Sleep(measureDelay);
                if(workTread.WaveChannel32.Length < workTread.WaveChannel32.Position) Pause();
            }
        }

        private void ParseBPMQueue()
        {
            var bpm = 0d;
            if (lastVertex != null)
            {
                bpm = ((BPMVertex)lastVertex).BPM;
            }
            var beatCount = 0;
            var startTime = TimeSpan.Zero;
            while (true)
            {
                Clear();
                var isDeletion = false;
                while (vertexQueue.Any() && vertexQueue.Peek().Time < workTread.MeasureTime())
                {
                    var vertex = vertexQueue.Dequeue();
                    switch (vertex.Type)
                    {
                        case VertexType.BPM:
                            bpm = ((BPMVertex)vertex).BPM;
                            startTime = vertex.Time;
                            beatCount = 0;
                            OnBeat();
                            break;
                        case VertexType.Artificial:
                            OnBeat();
                            break;
                        case VertexType.Deletion:
                            isDeletion = true;
                            break;
                    }
                }

                if (bpm != 0)
                {
                    if (isDeletion) beatCount++;
                    var deltaTime = workTread.MeasureTime() - startTime;
                    var beatInterval = 1000 / (int)(bpm / 60);
                    var released = deltaTime.TotalMilliseconds / beatInterval;
                    if ((int)released > beatCount)
                    {
                        OnBeat();
                        beatCount = (int)released;
                    }
                }

                Thread.Sleep(measureDelay);
                if (workTread.WaveChannel32.Length < workTread.WaveChannel32.Position) Pause();
            }
        }

        private async Task WaitOneSecondAsync()
        {
            var task = new Task(() => Thread.Sleep(1020));
            task.Start();
            await task;
            Shutdown();
        }
    }
}
