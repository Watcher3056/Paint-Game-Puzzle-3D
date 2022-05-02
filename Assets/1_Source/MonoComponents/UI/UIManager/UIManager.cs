using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace TeamAlpha.Source
{
    public partial class UIManager : MonoBehaviour
    {
        public static UIManager Default => _default;
        private static UIManager _default;

        [Required]
        public Canvas mainCanvas;

        public enum State { None, MainMenu, Play, LevelComplete }

        public State CurState
        {
            get => curState;
            set
            {
                if (curState == value ||
                !statesMap.ContainsKey((int)value) ||
                !statesMap[(int)value].Condition((int)curState))
                    return;
                statesMap[(int)curState].OnEnd((int)value);
                statesMap[(int)value].OnStart();
                curState = value;

            }
        }
        private State curState;
        private Dictionary<int, StateDefault> statesMap = new Dictionary<int, StateDefault>();
        public UIManager()
        {
            _default = this;

            statesMap.AddState((int)State.None, () => { }, (a) => { });
            SetupStateMainMenu();
            SetupStatePlay();
            SetupStateLevelComplete();
        }
        public void ExitGame()
        {
            Application.Quit();
        }
        public void SetHighlightUIElement(GameObject go, bool arg)
        {
            Canvas canvas = null;
            if (arg)
            {
                canvas = go.AddComponent<Canvas>();
                canvas.overrideSorting = true;
                canvas.sortingOrder = 1;
            }
            else
            {
                canvas = go.GetComponent<Canvas>();
                GameObject.Destroy(canvas);
            }
        }
    }
}
