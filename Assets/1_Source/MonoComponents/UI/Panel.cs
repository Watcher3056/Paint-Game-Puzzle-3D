using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using Sirenix.Serialization;
using System.Collections;
using Animancer;

namespace TeamAlpha.Source
{
    [RequireComponent(typeof(CanvasGroup), typeof(Animator))]
    public class Panel : MonoBehaviour
    {
        [Serializable]
        public class PanelList
        {
            public List<Panel> list = new List<Panel>();

            public void OpenPanels()
            {
                foreach (Panel panel in list)
                    panel.OpenPanel();
            }
            public void ClosePanels()
            {
                foreach (Panel panel in list)
                    panel.ClosePanel();
            }
        }
        public enum FadeMode { In, Out }
        public enum State { None, Opened, Closed }

        [Required]
        public NamedAnimancerComponent animancer;
        [Required]
        public AnimationClip animOpen;
        [Required]
        public AnimationClip animClose;
        public bool fading;
        public bool hideOnStart = true;
        [ShowIf("fading")]
        public bool closeOnShadowClick;
        public bool playShowHideAudio;
        public bool saveInitialPosition;
        [ShowIf("playShowHideAudio")]
        public bool customAudioOnShow;
        [ShowIf("playShowHideAudio")]
        public bool customAudioOnHide;

        public event Action OnPanelShow = () => { };
        public event Action OnPanelHide = () => { };

        public State CurState => curState;
        private State curState;
        private bool ShowAudioOnShow => playShowHideAudio && customAudioOnShow;
        private bool ShowAudioOnHide => playShowHideAudio && customAudioOnHide;

        private List<object> blockersToShow = new List<object>();

        public static Panel UpperShaded => curShowedPanels.FindLast(p => p.fading);

        private static List<Panel> curShowedPanels = new List<Panel>();
        public void Start()
        {
            RectTransform rt = GetComponent<RectTransform>();
            if (rt != null && !saveInitialPosition)
                rt.anchoredPosition = Vector3.zero;

            //if (UIManager.Default.panelLoading == this)
            //    return;
            if (curState == State.None)
            {
                if (hideOnStart)
                {
                    ClosePanel();
                    gameObject.SetActive(false);
                }
                else
                    OpenPanel();
            }
        }

        public void SetBlockShow(object blocker, bool arg)
        {
            if (arg && !blockersToShow.Contains(blocker))
                blockersToShow.Add(blocker);
            else if (!arg)
                blockersToShow.Remove(blocker);
        }
        public void TogglePanel(bool arg)
        {
            if (arg)
                OpenPanel();
            else
                ClosePanel();
        }

        //Do not place here logic as On Panel Show
        public void OpenPanel()
        {
            gameObject.SetActive(true);
            PanelFade(FadeMode.In);
        }
        //Do not place here logic as On Panels Hide
        public void ClosePanel()
        {
            PanelFade(FadeMode.Out);
        }
        private void PanelFade(FadeMode fadeMode)
        {
            IEnumerator WaitToShow()
            {
                while (blockersToShow.Count > 0)
                    yield return null;
                PanelFade(FadeMode.In);
            }
            if ((curState == State.Opened && fadeMode == FadeMode.In) ||
                (curState == State.Closed && fadeMode == FadeMode.Out))
                return;

            if (fadeMode == FadeMode.In)
            {
                if (blockersToShow.Count > 0)
                {
                    StartCoroutine(WaitToShow());
                    return;
                }
                curShowedPanels.Add(this);
                if (fading && PanelShadow.Default.panel != this)
                    PanelShadow.Default.panel.OpenPanel();
                transform.SetAsLastSibling();
                curState = State.Opened;
                animancer.Play(animOpen);
                OnPanelShow.Invoke();
            }
            else if (fadeMode == FadeMode.Out)
            {
                if (curShowedPanels.Contains(this))
                    curShowedPanels.Remove(this);
                if (fading)
                {
                    Panel panelWithFading = curShowedPanels.Find((p) => p.fading);
                    if (curShowedPanels.Contains(PanelShadow.Default.panel) && panelWithFading == null)
                        PanelShadow.Default.panel.ClosePanel();
                }

                List<Panel> fadedPanels = new List<Panel>(curShowedPanels.FindAll(p => p.fading));
                for (int i = 0; i < fadedPanels.Count; i++)
                {
                    Panel panel = fadedPanels[i];
                    if (i + 1 == fadedPanels.Count)
                        PanelShadow.Default.panel.transform.SetAsLastSibling();
                    if (panel.fading)
                        panel.transform.SetAsLastSibling();
                }
                curState = State.Closed;
                animancer.Play(animClose);
                OnPanelHide.Invoke();
            }

            //UIManager.Default.panelLoading.transform.SetAsLastSibling();
        }
    }
}
