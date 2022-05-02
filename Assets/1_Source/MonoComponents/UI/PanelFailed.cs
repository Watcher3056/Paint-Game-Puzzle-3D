using Pixeye.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace TeamAlpha.Source
{
    public class PanelFailed : MonoBehaviour
    {
        public static PanelFailed Default { get; private set; }

        [Required]
        public Panel panel;
        [Required]
        public Button buttonRestart;
        [Required]
        public Button buttonRewardedVideo;
        [Required]
        public TextMeshProUGUI textDiamondsGained;

        public PanelFailed() => Default = this;
        private void Awake()
        {
            panel.OnPanelShow += HandlePanelShow;

            buttonRestart.onClick.AddListener(HandleButtonRestartClick);
            buttonRewardedVideo.onClick.AddListener(HandleButtonRewardedVideoClick);
            buttonRewardedVideo.transform.DOScale(1.25f, 1f).SetLoops(-1, LoopType.Yoyo);
        }
        private void HandleButtonRestartClick()
        {
            DataGameMain.Default.audioOnButtonClick.Play(ProcessorSoundPool.PoolLevel.Global);
            panel.ClosePanel();
            LayerDefault.Default.Restart();
        }
        private void HandleButtonRewardedVideoClick()
        {
            DataGameMain.Default.audioOnButtonClick.Play(ProcessorSoundPool.PoolLevel.Global);
            RewardedVideo.Default.ShowRewardedVideo(success =>
            {
                if (success)
                {
                    panel.ClosePanel();
                    LevelController.Current.Equality = 1f;
                    LayerDefault.Default.Restart();
                }
            });
        }
        private void HandlePanelShow()
        {
            textDiamondsGained.text = "+0";
        }
    }
}
