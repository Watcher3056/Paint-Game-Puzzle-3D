using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamAlpha.Source
{
    public partial class UIManager
    {
        private void SetupStateMainMenu()
        {
            statesMap.AddState((int)State.MainMenu, StateMainMenuOnStart, StateMainMenuOnEnd);
        }
        private void StateMainMenuOnStart()
        {

        }
        private void StateMainMenuOnEnd(int stateTo)
        {

        }
    }
}
