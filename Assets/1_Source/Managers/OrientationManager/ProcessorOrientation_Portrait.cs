using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamAlpha.Source
{
    public partial class ProcessorOrientation
    {
        private void SetupStateVertical()
        {
            StateDefault state = new StateDefault();
            state.OnStart = StartStateVertical;
            state.OnEnd = EndStateVertical;

            statesMap.Add(GameMode.Vertical, state);
        }
        private void StartStateVertical()
        {

        }
        private void EndStateVertical(int stateTo)
        {

        }
    }
}
