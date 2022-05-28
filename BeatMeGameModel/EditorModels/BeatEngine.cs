using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatMeGameModel.BeatVertexes;
using BeatMeGameModel.IOWorkers;
using NAudio.Wave;
using SoundEngineLibrary;

namespace BeatMeGameModel.EitorModels //TODO: FFT парсер созадёт для себя отдельный поток
{
    public class BeatEngine
    {
        public event Action OnBeat;
        public event Action Clear;
        public event Action Shutdown;
        private readonly Queue<BeatVertex> vertexQueue = new Queue<BeatVertex>();
        private readonly FFTTread workTread;
        private readonly int measureDelay;
        private Thread asyncEventInvoker;
        private readonly BeatDetectionType detectionType;
        private BeatVertex lastVertex;

        public BeatEngine(SoundEngineTread workTread, Dictionary<TimeSpan, BeatVertex> beat, BeatDetectionType detectionType, TimeSpan position)
        {
            this.workTread =
                new FFTTread(workTread.FullFilePath, position, workTread.MaxSongDuration);
            this.workTread.Closing += Shutdown;
            FillVertexQueue(beat, position);
            measureDelay = 1000 / (this.workTread.FFT.samplingFrequency / FFT.FFTSize);
            this.detectionType = detectionType;
        }

        public void Play(bool isShutdownAfterSecond )
        {
            workTread.Run();
            asyncEventInvoker = detectionType == BeatDetectionType.FFT
                ? new Thread(ParseFFTQueue)
                : new Thread(ParseBPMQueue);
            asyncEventInvoker.Start();
            if (isShutdownAfterSecond) WaitOneSecondAsync();
        }

        public void Pause()
        {
            asyncEventInvoker.Abort();
            workTread.Stop();
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
                    workTread.FFT.HighEdge = ((FFTVertex)lastVertex).TopFrequency;
                    workTread.FFT.LowEdge = ((FFTVertex)lastVertex).BotFrequency;
                    workTread.FFT.ThresholdValue = ((FFTVertex)lastVertex).ThresholdValue;
                }
                Clear();
                var isDeletion = false;
                while (vertexQueue.Any() && vertexQueue.Peek().Time < workTread.GetTime())
                {
                    var vertex = vertexQueue.Dequeue();
                    switch (vertex.Type)
                    {
                        case VertexType.FFT:
                            workTread.FFT.HighEdge = ((FFTVertex)vertex).TopFrequency;
                            workTread.FFT.LowEdge = ((FFTVertex)vertex).BotFrequency;
                            workTread.FFT.ThresholdValue = ((FFTVertex)vertex).ThresholdValue;
                            workTread.FFT.GetBeatData(workTread.GetTime());
                            break;
                        case VertexType.Artificial:
                            OnBeat();
                            break;
                        case VertexType.Deletion:
                            isDeletion = true;
                            break;
                    }
                }

                if (!isDeletion && workTread.FFT.GetBeatData(workTread.GetTime()))
                {
                    OnBeat();
                }

                Thread.Sleep(measureDelay);
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
                while (vertexQueue.Any() && vertexQueue.Peek().Time < workTread.GetTime())
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
                    var deltaTime = workTread.GetTime() - startTime;
                    var beatInterval = 1000 / (int)(bpm / 60);
                    var released = deltaTime.TotalMilliseconds / beatInterval;
                    if ((int)released > beatCount)
                    {
                        OnBeat();
                        beatCount = (int)released;
                    }
                }

                Thread.Sleep(measureDelay);
            }
        }

        private async Task WaitOneSecondAsync()
        {
            await Task.Delay(1020);
            Shutdown();
        }
    }
}
