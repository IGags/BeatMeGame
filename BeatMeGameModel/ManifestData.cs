using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatMeGameModel.IOWorkers;

namespace BeatMeGameModel
{
    public class ManifestData
    {
        public string SongName { get; set; } = "";
        public BeatDetectionType DetectionType { get; set; } = BeatDetectionType.FFT;
        public int StartSecond { get; set; } = 0;

        public EditorType EditorType { get; set; }

        public ManifestData(){}

        public ManifestData(string songName, BeatDetectionType detectionType, int startSecond, EditorType editorType)
        {
            SongName = songName;
            DetectionType = detectionType;
            StartSecond = startSecond;
            EditorType = editorType;
        }
    }
}
