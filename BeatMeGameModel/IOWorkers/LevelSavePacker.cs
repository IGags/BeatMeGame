using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatMeGameModel.BeatVertexes;

namespace BeatMeGameModel.IOWorkers
{
    public static class LevelSavePacker
    {
        public static void PackSave(LevelSave saveData)
        {
            var manifestString = string.Join("\n", saveData.Manifest.SongName, 
                BeatDetectionTypeExtensions.ToString(saveData.Manifest.DetectionType), 
                saveData.Manifest.StartSecond.ToString(), 
                EditorTypeExtensions.PackEditorType(saveData.Manifest.EditorType));
            var beatString = string.Join("\n", PackBeatVertices(saveData.Beat));
            LevelFolderWorker.SaveFile(saveData.LevelName, "manifest.txt", manifestString);
            LevelFolderWorker.SaveFile(saveData.LevelName, "beat.txt", beatString);
        }

        public static string[] PackBeatVertices(Dictionary<TimeSpan, BeatVertex> vertexes)
        {
            var keys = vertexes.Keys.OrderBy(value => value).ToList();
            var packedVertices = new List<string>();
            foreach (var key in keys)
            {
                if (vertexes[key] is FFTVertex)
                    packedVertices.Add(((FFTVertex)vertexes[key]).ToString());
                else if(vertexes[key] is BeatVertex)
                    packedVertices.Add(((BeatVertex)vertexes[key]).ToString());
                else packedVertices.Add((vertexes[key]).ToString());
            }

            return packedVertices.ToArray();
        }
    }
}
