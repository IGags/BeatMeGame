using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatMeGameModel.LevelEditorModels
{
    public class EditorModel
    {
        public Dictionary<string, string[]> Scripts { get; set; }
        public LevelSave Save { get; }

        public EditorModel(LevelSave save, Dictionary<string, string[]> scripts)
        {
            Save = save;
            Scripts = scripts;
        }
    }
}
