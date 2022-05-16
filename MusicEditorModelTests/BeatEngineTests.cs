using System;
using System.Collections.Generic;
using BeatMeGameModel;
using BeatMeGameModel.BeatVertexes;
using BeatMeGameModel.EditorModels;
using BeatMeGameModel.IOWorkers;
using NUnit.Framework;
using SoundEngineLibrary;

namespace MusicEditorModelTests
{
    [TestFixture]
    public class BeatEngineTests
    {
        private readonly SoundEngine engine = new SoundEngine();

        [Test]
        public void CheckTemporalThreadExceptionTest()
        {
            var tread = engine.CreateTread(ThreadOptions.TemporalThread, "Resources\\2962355319 (1).mp3",
                FFTExistance.DoesntExist);
            Assert.Catch<ArgumentException>(() =>
            {
                var beatEngine = new BeatEngine(engine.GetTread(tread), new Dictionary<TimeSpan, BeatVertex>(),
                    BeatDetectionType.FFT, TimeSpan.Zero);
            });
        }

        [Test]
        public void TestOneSecondModeDuration()
        {
            var tread = engine.CreateTread(ThreadOptions.StaticThread, "Resources\\2962355319 (1).mp3",
                FFTExistance.Exist);
        }
    }
}
