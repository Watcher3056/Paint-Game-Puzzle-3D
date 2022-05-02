using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamAlpha.Source
{
    public partial class ProcessorOrientation
    {
        private void SetupStateHorizontal()
        {
            StateDefault state = new StateDefault();
            state.OnStart = StartStateHorizontal;
            state.OnEnd = EndStateHorizontal;

            statesMap.Add(GameMode.Horizontal, state);
        }
        private void StartStateHorizontal()
        {

        }
        private void EndStateHorizontal(int stateTo)
        {

        }
    }
}
