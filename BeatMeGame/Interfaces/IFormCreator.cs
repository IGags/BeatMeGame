using System.Windows.Forms;
using BeatMeGame.MenuView;

namespace BeatMeGame
{
    interface IFormCreator
    {
        void CreateChildForm(Form formToCreate);
        void ChangeScene(MenuStateMachine previousScene, Form newScene);
        void ReestablishScene();
    }
}
