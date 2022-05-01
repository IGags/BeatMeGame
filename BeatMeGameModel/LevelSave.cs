using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatMeGameModel.BeatVertexes;

namespace BeatMeGameModel
{
    public class LevelSave
    {
        public string LevelName { get; private set; }
        public ManifestData Manifest { get; set; }
        public Dictionary<TimeSpan, BeatVertex> Beat { get; set; } = new Dictionary<TimeSpan, BeatVertex>();

        public LevelSave(ManifestData manifest, Dictionary<TimeSpan, BeatVertex> beat, string levelName)
        {
            Manifest = manifest;
            Beat = beat;
            LevelName = levelName;
        }
    }
}
