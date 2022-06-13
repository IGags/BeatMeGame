using System;
using System.Collections.Generic;
using BeatMeGameModel.BeatVertexes;

namespace BeatMeGameModel.IOWorkers
{
    public enum LaunchType
    {
        Redactor,
        Game
    }

    public class LevelSaveParsers
    {
        public static LevelSave TryParseLevelSave(string path, LaunchType type)
        {
            var manifestData = TryParseLevelManifest(type, path);
            if (manifestData == null) return null;
            if (manifestData.SongName == null)
                return new LevelSave(manifestData, new Dictionary<TimeSpan, BeatVertex>(), path);
            var beatData = TryParseBeatData(path);
            return beatData == null 
                ? new LevelSave(manifestData, new Dictionary<TimeSpan, BeatVertex>(), path) 
                : new LevelSave(manifestData, beatData, path);
        }

        public static Dictionary<TimeSpan, BeatVertex> TryParseBeatData(string levelName)
        {
            var isRead = LevelFolderWorker.TryReadFile(levelName, out var rawData, "beat.txt");
            if (!isRead) return null;
            var beats = rawData.Split('\n');
            var beatData = new Dictionary<TimeSpan, BeatVertex>();
            foreach (var beat in beats)
            {
                var vertex = TryParseBeatVertex(beat);
                if (vertex == null) return null;
                beatData[vertex.Time] = vertex;
            }

            return beatData;
        }

        private static BeatVertex TryParseBeatVertex(string rawVertex)
        {
            var data = rawVertex.Split(' ');
            if (data.Length < 2 || data.Length == 4 || data.Length > 5) return null;
            var isParsed = TimeSpan.TryParse(data[0], out var time) 
                           & VertexTypeExtensions.TryParseVertex(data[1], out var type);
            switch (data.Length)
            {
                case 3:
                {
                    isParsed &= double.TryParse(data[2], out var bpm);
                    if (!isParsed) return null;
                    return type != VertexType.BPM ? null : new BPMVertex(time, type, bpm);
                }
                case 5:
                {
                    isParsed &= int.TryParse(data[2], out var topFrequency) & int.TryParse(data[3], out var botFrequency) &
                                double.TryParse(data[4], out var thresholdValue);
                    if (!isParsed || topFrequency < botFrequency) return null;
                    return type != VertexType.FFT ? null : new FFTVertex(time, type, topFrequency, botFrequency, thresholdValue);
                }
            }

            if (!isParsed) return null;
            return type != VertexType.Artificial && type != VertexType.Deletion
                ? null : new BeatVertex(time, type);
        }

        private static ManifestData TryParseLevelManifest(LaunchType type, string levelName)
        {
            var isRead = LevelFolderWorker.TryReadFile(levelName, out var rawManifest, "manifest.txt");
            if (!isRead && type == LaunchType.Game) return null;
            if (!isRead) return RebuildManifest(levelName);
            var config = rawManifest.Split('\n');
            if (config.Length != 4) return RebuildManifest(levelName);
            return !LevelFolderWorker.CheckFileExist(levelName, config[0]) 
                   | !config[1].TryParseBeatDetectionType(out var beatType) 
                   | !int.TryParse(config[2], out var startSecond)
                   | !config[3].TryParseEditorType(out var editorType)
                ? new ManifestData() : new ManifestData(config[0], beatType, startSecond, editorType);
        }

        private static ManifestData RebuildManifest(string path)
        {
            LevelFolderWorker.ClearLevelFolder(path, "manifest.txt");
            return new ManifestData();
        }
    }
}
