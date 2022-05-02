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
        private void SetupStateLevelComplete()
        {
            statesMap.AddState((int)State.LevelComplete, StateLevelCompleteOnStart, StateLevelCompleteOnEnd);
        }
        private void StateLevelCompleteOnStart()
        {
            //PanelLevelComplete.Default.panel.OpenPanel();
        }
        private void StateLevelCompleteOnEnd(int stateTo)
        {
            //PanelLevelComplete.Default.panel.ClosePanel();
            PanelScoresMultiplier.Default.panel.ClosePanel();
            PanelEqualityCheck.Default.panel.ClosePanel();
            PanelStars.Default.panel.ClosePanel();
            PanelEpicLevelProgress.Default.panel.ClosePanel();
        }
    }
}
