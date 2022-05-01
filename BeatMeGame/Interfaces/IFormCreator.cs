using System.Windows.Forms;

namespace BeatMeGame
{
    interface IFormCreator
    {
        void CreateChildForm(Form formToCreate);
        void ChangeScene(Form previousScene);
        void ReestablishScene();
    }
}
