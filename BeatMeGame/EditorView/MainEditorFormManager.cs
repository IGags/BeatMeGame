using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BeatMeGameModel;
using BeatMeGameModel.IOWorkers;

namespace BeatMeGame.EditorView
{
    public class MainEditorFormManager
    {
        private Form child;
        public MainEditorFormManager(Form parent, LevelSave save)
        {
            Initialize(parent, save);
        }

        private void Initialize(Form parent, LevelSave save)
        {
            if (save.Manifest == null || save.Manifest.SongName == "")
            {
                ChooseSong(save, true);
            }
            var creator = (IFormCreator)parent;
            creator.CreateChildForm(new MusicEditorForm(parent, save));
        }

        private void ChooseSong(LevelSave save, bool looped)
        {
            while (true)
            {
                var dialog = new OpenFileDialog();
                dialog.CheckFileExists = true;
                dialog.Filter = @"File in mp3 |*.mp3";
                if (DialogResult.OK == dialog.ShowDialog())
                {
                    save.Manifest = new ManifestData(dialog.SafeFileName, BeatDetectionType.FFT, 0);
                    LevelFolderWorker.CopyMp3ToLevelFolder(dialog.FileName, save.LevelName + "\\" +dialog.SafeFileName);
                    LevelFolderWorker.DeleteFile(save.LevelName, "beat.txt");
                    break;
                }
                if(!looped) break;
            }
        }
    }
}
