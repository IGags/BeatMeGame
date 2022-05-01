using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatMeGameModel.IOWorkers;
using SoundEngineLibrary;

namespace BeatMeGameModel
{
    public static class BeatDetectionTypeExtensions
    {
        public static bool TryParseBeatDetectionType(this string rawData, out BeatDetectionType type)
        {
            switch (rawData)
            {
                case "FFT":
                    type = BeatDetectionType.FFT;
                    return true;
                case "BPM":
                    type = BeatDetectionType.BPM;
                    return true;
                default:
                    type = BeatDetectionType.FFT;
                    return false;
            }
        }
    }
}
