using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace TeamAlpha.Source
{
    public class PanelLegendaryLevelPopup : MonoBehaviour
    {
        public static PanelLegendaryLevelPopup Default { get; private set; }

        [Required]
        public Panel panel;
        [Required]
        public GameObject viewTable;
        [Required]
        public Button buttonPlay;
        [Required]
        public Button buttonSkip;

        public PanelLegendaryLevelPopup() => Default = this;
        private void Awake()
        {
            panel.OnPanelShow += HandlePanelShow;
            buttonPlay.onClick.AddListener(HandleButtonPlayClick);
            buttonSkip.onClick.AddListener(HandleButtonSkipClick);

            Vector3 viewTableLocalPos = viewTable.transform.localPosition + Vector3.down * 50;
            viewTable.transform.DOLocalMove(viewTableLocalPos, 1f).SetLoops(-1, LoopType.Yoyo);
        }
        private void HandlePanelShow()
        {
            buttonSkip.gameObject.SetActive(false);
            ProcessorDeferredOperation.Default.Add(() => buttonSkip.gameObject.SetActive(true), true, false, 2f);
        }
        private void HandleButtonPlayClick()
        {
            RewardedVideo.Default.ShowRewardedVideo(success =>
            {
                if (success)
                {
                    GenericActions();
                    LevelController.Current.Launch();
                }
            });
        }
        private void HandleButtonSkipClick()
        {
            GenericActions();
            LayerDefault.Default.LaunchLevel(LayerDefault.Default.GetLastLevelToPlay());
        }
        private void GenericActions()
        {
            DataGameMain.Default.audioOnButtonClick.Play(ProcessorSoundPool.PoolLevel.Global);
            panel.ClosePanel();
            PanelTop.Default.panel.OpenPanel();
            LayerDefault.Default.ResetLegendPopupCounter();
        }
    }
}
