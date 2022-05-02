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
using System.Collections;
using DG.Tweening;
using Animancer;
using static TeamAlpha.Source.LayerDefault;

namespace TeamAlpha.Source
{
    public class PanelProgress : MonoBehaviour
    {
        public static PanelProgress Default => _default;
        private static PanelProgress _default;

        [Required]
        public Panel panel;
        [Required]
        public Image progressBar;
        [Required]
        public Image imageButtonHintWatchAds;
        [Required]
        public Image imageButtonTimeBoostWatchAds;
        [Required]
        public Slider progressBarSlider;
        [Required]
        public TextMeshProUGUI textCurLevel;
        [Required]
        public TextMeshProUGUI textTimeLeft;
        [Required]
        public TextMeshProUGUI textHintsLeft;
        [Required]
        public TextMeshProUGUI textTimeBoostLeft;
        [Required]
        public ColorPicker colorPicker;
        [Required]
        public Button buttonHint;
        [Required]
        public Button buttonDone;
        [Required]
        public Button buttonTime;
        [Required]
        public GameObject viewHintsAmount;
        [Required]
        public GameObject viewTimeBoostAmount;
        public float scoresAnimTime;

        private int curScores;
        private Tweener tweenerScoresCounter;
        private float skillIndicatorOffset;

        public PanelProgress() => _default = this;
        private void Awake()
        {
            ProcessorObserver.Default.Add(() => DataGameMain.Default.HintsLeft, hints =>
            {
                if (DataGameMain.Default.HintsLeft > 0)
                {
                    viewHintsAmount.SetActive(true);
                    imageButtonHintWatchAds.gameObject.SetActive(false);
                    textHintsLeft.gameObject.SetActive(true);
                    textHintsLeft.text = DataGameMain.Default.HintsLeft.ToString();
                }
                else
                {
                    viewHintsAmount.SetActive(false);
                    imageButtonHintWatchAds.gameObject.SetActive(true);
                    textHintsLeft.gameObject.SetActive(false);
                }
            }, true);
            ProcessorObserver.Default.Add(() => DataGameMain.Default.TimeBoostLeft, timeBoosters =>
            {
                if (DataGameMain.Default.TimeBoostLeft > 0)
                {
                    viewTimeBoostAmount.SetActive(true);
                    imageButtonTimeBoostWatchAds.gameObject.SetActive(false);
                    textTimeBoostLeft.gameObject.SetActive(true);
                    textTimeBoostLeft.text = DataGameMain.Default.TimeBoostLeft.ToString();
                }
                else
                {
                    viewTimeBoostAmount.SetActive(false);
                    imageButtonTimeBoostWatchAds.gameObject.SetActive(true);
                    textTimeBoostLeft.gameObject.SetActive(false);
                }
            }, true);

            LevelController.OnLevelChangedChanged += UpdateView;
            buttonDone.onClick.AddListener(HandleButtonDoneClick);
            buttonHint.onClick.AddListener(HandleButtonHintClick);
            buttonTime.onClick.AddListener(HandleButtonTimeClick);
            panel.OnPanelShow += HandlePanelOpen;
        }
        private void HandleButtonTimeClick()
        {
            DataGameMain.Default.audioOnButtonClick.Play(ProcessorSoundPool.PoolLevel.GameLevel);
            if (DataGameMain.Default.TimeBoostLeft > 0)
            {
                DataGameMain.Default.TimeBoostLeft--;
                LevelController.Current.TimeLeft += 60;
            }
            else
            {
                RewardedVideo.Default.ShowRewardedVideo(success =>
                {
                    if (success)
                        LevelController.Current.TimeLeft += 60;
                });
            }
        }
        private void HandlePanelOpen()
        {
            //UpdateView();
        }
        private void HandleButtonDoneClick()
        {
            DataGameMain.Default.audioOnButtonClick.Play(ProcessorSoundPool.PoolLevel.GameLevel);
            LevelController.Current.CompleteLevel();
        }
        private void HandleButtonHintClick()
        {
            DataGameMain.Default.audioOnButtonClick.Play(ProcessorSoundPool.PoolLevel.Global);
            if (DataGameMain.Default.HintsLeft > 0)
            {
                DataGameMain.Default.HintsLeft--;
                LevelController.Current.modelPlayable.EnableHint();
            }
            else
            {
                RewardedVideo.Default.ShowRewardedVideo(success =>
                {
                    if (success)
                        LevelController.Current.modelPlayable.EnableHint();
                });
            }
        }
        private void Update()
        {
            if (!LayerDefault.Default.Playing)
                return;
            textTimeLeft.text = LevelController.Current.TimeSpanLeft.ToString(@"mm\:ss");
            //Vector3 playerScreenPos = MonoCamera.Default.cam.WorldToScreenPoint(MonoPlayerController.Current.transform.position);
            //playerScreenPos.z = 0f;

            //float progress =
            //    (MonoBallController.Default.rigidbody.transform.position.z -
            //    LayerDefault.Default.playerCtrlStartFrom.startPosition.z) /
            //    LayerDefault.Default.CurLevel.LevelDistanceTotal;
            //progressBar.fillAmount = progress;
            //progressBarSlider.value = progress;
        }
        private void UpdateView()
        {
            colorPicker.UpdateView();
            int levelIndex = 1;
            if (LayerDefault.Default.CurLevel is LevelEpic)
            {
                levelIndex += LayerDefault.Default.levelsEpic.IndexOf(LayerDefault.Default.CurLevel as LevelEpic);
                textCurLevel.text = "Epic " + levelIndex.ToString();
            }
            else if (LayerDefault.Default.CurLevel is LevelLegendary)
            {
                levelIndex += LayerDefault.Default.levelsLegendary.IndexOf(LayerDefault.Default.CurLevel as LevelLegendary);
                textCurLevel.text = "Legendary " + levelIndex.ToString();
            }
            else
            {
                levelIndex += LayerDefault.Default.levelsDefault.IndexOf(LayerDefault.Default.CurLevel);
                textCurLevel.text = "Level " + levelIndex.ToString();
            }
        }
    }
}