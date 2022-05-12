using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BeatMeGameModel.BeatVertexes;
using BeatMeGameModel.IOWorkers;
using NAudio.Wave;
using SoundEngineLibrary;

namespace BeatMeGameModel
{
    public class BeatEngine
    {
        public Action OnBeat;
        private readonly Queue<BeatVertex> vertexQueue = new Queue<BeatVertex>();
        private readonly SoundEngineTread workTread;
        private readonly int measureDelay;
        private Thread asyncEventInvoker;
        private BeatDetectionType detectionType;

        public BeatEngine(SoundEngineTread workTread, Dictionary<TimeSpan, BeatVertex> beat, BeatDetectionType detectionType, TimeSpan position)
        {
            if (workTread.TreadType == ThreadOptions.TemporalThread) throw new ArgumentException("Invalid tread type");
            FillVertexQueue(beat);
            if (workTread.OutputDevice.PlaybackState == PlaybackState.Playing) workTread.ChangePlaybackState();
            workTread.ChangePlayingPosition((int)position.TotalSeconds);
            this.workTread = workTread;
            measureDelay = 1000 / (this.workTread.TrackFFT.samplingFrequency / FFT.FFTSize);
            this.detectionType = detectionType;
        }

        public void Play()
        {
            if(workTread.OutputDevice.PlaybackState == PlaybackState.Playing) return;
            workTread.ChangePlaybackState();
            asyncEventInvoker = detectionType == BeatDetectionType.FFT
                ? new Thread(ParseFFTQueue)
                : new Thread(ParseBPMQueue);
            asyncEventInvoker.Start();
        }

        public void Pause()
        {
            if (workTread.OutputDevice.PlaybackState == PlaybackState.Paused 
                || workTread.OutputDevice.PlaybackState == PlaybackState.Stopped) return;
            workTread.ChangePlaybackState();
            asyncEventInvoker.Abort();
        }

        private void FillVertexQueue(Dictionary<TimeSpan, BeatVertex> beat)
        {
            foreach (var beatVertex in beat.OrderBy(value => value.Key))
            {
                vertexQueue.Enqueue(beatVertex.Value);
            }
        }

        private void ParseFFTQueue()
        {
            while (true)
            {
                var isDeletion = false;
                while (vertexQueue.Peek().Time < workTread.MeasureTime())
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
            }
        }

        private void ParseBPMQueue()
        {
            while (true)
            {
                var bpm = 0d;
                var startTime = TimeSpan.Zero;
                var isDeletion = false;
                var isBPMChanged = false;
                while (vertexQueue.Peek().Time < workTread.MeasureTime())
                {
                    var vertex = vertexQueue.Dequeue();
                    switch (vertex.Type)
                    {
                        case VertexType.BPM:
                            bpm = ((BPMVertex)vertex).BPM;
                            startTime = vertex.Time;
                            isBPMChanged = true;
                            break;
                        case VertexType.Artificial:
                            OnBeat();
                            break;
                        case VertexType.Deletion:
                            isDeletion = true;
                            break;
                    }
                }

                if (!isDeletion)
                {
                    if(bpm == 0) break;
                    var deltaTime = workTread.MeasureTime() - startTime;
                    var beatInterval = 1000 / (int)(bpm / 60);
                    var released = deltaTime.TotalMilliseconds / beatInterval;
                    var fractionalPart = released % (int)released;
                    if (fractionalPart < (double)measureDelay / 1000) OnBeat();
                }

                Thread.Sleep(measureDelay);
            }
        }
    }
}
