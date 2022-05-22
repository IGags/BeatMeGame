using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatMeGameModel
{
    public enum EditorType
    {
        Music,
        Level
    }
    public static class EditorTypeExtensions
    {
        public static bool TryParseEditorType(this string rawData, out EditorType editorType)
        {
            switch (rawData)
            {
                case "music":
                    editorType = EditorType.Music;
                    return true;
                case "level":
                    editorType = EditorType.Level;
                    return true;
                default:
                    editorType = EditorType.Music;
                    return false;
            }
        }

        public static string PackEditorType(EditorType type)
        {
            switch (type)
            {
                case EditorType.Level:
                    return "level";
                case EditorType.Music:
                    return "music";
                default:
                    return "music";
            }
        }
    }
}
