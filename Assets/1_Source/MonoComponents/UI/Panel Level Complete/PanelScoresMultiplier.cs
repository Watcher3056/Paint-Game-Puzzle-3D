using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeamAlpha.Source
{
    public class PanelScoresMultiplier : MonoBehaviour
    {
        [Serializable]
        public class MultiplierStep
        {
            [Range(0f, 1f)]
            public float offset;
            public int multiplier;
        }

        public static PanelScoresMultiplier Default { get; private set; }

        [Required]
        public Panel panel;
        [Required]
        public Slider sliderMultiplier;
        [Required]
        public Button buttonGetBonus;
        [Required]
        public Button buttonRetry;
        [Required]
        public Button buttonSkip;
        [Required]
        public TextMeshProUGUI textGetBonus;
        [Required]
        public TextMeshProUGUI textStarsReward;
        public AnimationCurve curveSpeed;
        public List<MultiplierStep> steps;
        public AudioSimpleData audioStars;

        private bool directionRight;
        private bool active;
        private float curReward;
        public PanelScoresMultiplier() => Default = this;
        private void Awake()
        {
            sliderMultiplier.minValue = -1f;
            sliderMultiplier.maxValue = 1f;
            buttonGetBonus.onClick.AddListener(HandleButtonGetBonusClick);
            buttonRetry.onClick.AddListener(HandleButtonRetryClick);
            buttonSkip.onClick.AddListener(HandleButtonSkipClick);
            panel.OnPanelShow += HandlePanelShow;
        }
        private void Update()
        {
            if (!active)
                return;
            float speed = curveSpeed.Evaluate(sliderMultiplier.value);
            sliderMultiplier.value += (directionRight ? 1 : -1f) * speed * Time.deltaTime;
            if (Mathf.Abs(sliderMultiplier.value) == 1f)
                directionRight = !directionRight;
            curReward = steps.FindLast(s => Mathf.Abs(s.offset) <= Mathf.Abs(sliderMultiplier.value)).multiplier;
            textGetBonus.text = 'x' + curReward.ToString();
        }
        private void HandleButtonGetBonusClick()
        {
            DataGameMain.Default.audioOnButtonClick.Play(ProcessorSoundPool.PoolLevel.GameLevel);

            if (DataGameMain.Default.GameThemeFirstLocked != null)
            {
                RewardedVideo.Default.ShowRewardedVideo(success =>
                {
                    if (success)
                    {
                        active = false;
                        textStarsReward.transform.DOScale(1.5f, 0.25f).onComplete = () =>
                        {
                            textStarsReward.transform.DOScale(1f, 0.25f);
                        };
                        LevelController.Current.LevelStats.diamondsBonusMultiplier = (int)curReward;
                        audioStars.Play(ProcessorSoundPool.PoolLevel.GameLevel);
                        UpdateViewScores();
                        buttonGetBonus.gameObject.SetActive(false);
                        buttonRetry.gameObject.SetActive(false);
                        buttonSkip.gameObject.SetActive(false);

                        PanelTop.Default.AddDiamonds(LevelController.Current.LevelStats.TotalDiamondsReward, textStarsReward.transform.position, panel.ClosePanel);
                    }
                });
            }
        }
        private void HandleButtonSkipClick()
        {
            DataGameMain.Default.audioOnButtonClick.Play(ProcessorSoundPool.PoolLevel.GameLevel);
            buttonGetBonus.gameObject.SetActive(false);
            buttonRetry.gameObject.SetActive(false);
            buttonSkip.gameObject.SetActive(false);

            PanelTop.Default.AddDiamonds(LevelController.Current.LevelStats.TotalDiamondsReward, textStarsReward.transform.position, panel.ClosePanel);
        }
        private void HandleButtonRetryClick()
        {
            PanelTop.Default.AddDiamonds(LevelController.Current.LevelStats.TotalDiamondsReward, textStarsReward.transform.position);
            DataGameMain.Default.audioOnButtonClick.Play(ProcessorSoundPool.PoolLevel.GameLevel);
            buttonGetBonus.gameObject.SetActive(false);
            buttonRetry.gameObject.SetActive(false);
            buttonSkip.gameObject.SetActive(false);

            ProcessorDeferredOperation.Default.Add(() =>
            {
                panel.ClosePanel();
                LayerDefault.Default.LaunchLevel(LayerDefault.Default.CurLevel);
            }, true, false, 1f / PanelTop.Default.diamondsAddFlySpeed);
        }
        private void HandlePanelShow()
        {
            active = true;
            buttonGetBonus.gameObject.SetActive(true);
            buttonRetry.gameObject.SetActive(false);
            buttonSkip.gameObject.SetActive(false);
            ProcessorDeferredOperation.Default.Add(() =>
            {
                if (active)
                {
                    buttonSkip.gameObject.SetActive(true);
                    buttonRetry.gameObject.SetActive(true);
                }
            }, true, false, 2f);
            UpdateViewScores();
        }
        private void UpdateViewScores()
        {
            textStarsReward.text = '+' + ((int)LevelController.Current.LevelStats.TotalDiamondsReward).ToString();
        }
    }
}
