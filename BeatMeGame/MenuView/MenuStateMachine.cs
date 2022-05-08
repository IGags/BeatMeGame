using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatMeGame.MenuView
{
    public enum State
    {
        MenuList,
        Settings,
        Editor,
        Play
    }

    public class MenuStateMachine
    {
        public State State { get; private set; } = State.MenuList;

        public void ChangeState(RedirectionButton button)
        {
            State = button.Direction;
            StateChanged();
        }

        public event Action StateChanged;
    }
}
