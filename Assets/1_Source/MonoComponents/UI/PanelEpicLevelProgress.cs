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
using static TeamAlpha.Source.LayerDefault;

namespace TeamAlpha.Source
{
    public class PanelEpicLevelProgress : MonoBehaviour
    {
        public static PanelEpicLevelProgress Default { get; private set; }

        [Required]
        public Panel panel;
        [Required]
        public Slider sliderProgress;
        [Required]
        public TextMeshProUGUI textProgress;
        [Required]
        public Image imageUnknown;
        [Required]
        public Image imagePreview;
        [Required]
        public Sprite spriteLocked;
        [Required]
        public Button buttonNext;
        public AudioSimpleData audioOnStart;
        public AudioSimpleData audioLoopIncrease;
        public AudioSimpleData audioOnUnlock;

        public LevelEpic LevelTracked { get; private set; }
        public float Progress
        {
            get
            {
                int result = 0;
                foreach (LevelEpic level in LayerDefault.Default.levelsEpic)
                {
                    if (level.Unlocked && LayerDefault.Default.levelsEpic.IndexOf(level) < 
                        LayerDefault.Default.levelsEpic.IndexOf(LevelTracked))
                    {
                        result = level.requiredDiamonds;
                    }
                }
                return (float)(DataGameMain.Default.Diamonds - result) / (float)(LevelTracked.requiredDiamonds - result);
            }
        }
        private AudioSource audioSourceLoopIncrease;
        private Tweener tween;

        public PanelEpicLevelProgress() => Default = this;
        private void Awake()
        {
            panel.OnPanelHide += HandlePanelHide;
            panel.OnPanelShow += HandlePanelShow;
            buttonNext.onClick.AddListener(HandleButtonNextClick);

            UpdateLevelTracked();
            if (LevelTracked != null)
                UpdateViewProgress(Progress);
        }
        private void HandlePanelHide()
        {
            UpdateLevelTracked();
        }
        private void HandleButtonNextClick()
        {
            DataGameMain.Default.audioOnButtonClick.Play(ProcessorSoundPool.PoolLevel.Global);
            LayerDefault.Default.Restart();
        }
        private void HandlePanelShow()
        {
            UpdateView(1f, 1f);
        }
        private bool UpdateLevelTracked()
        {
            LevelEpic _nextLevel = LayerDefault.Default.levelsEpic.Find(l => !l.Unlocked);
            imagePreview.gameObject.SetActive(false);
            if (_nextLevel != null)
            {
                imagePreview.sprite = _nextLevel.prefab.modelPrefab.spritePreview;
                if (_nextLevel != LevelTracked)
                    UpdateViewProgress(0f);
            }
            LevelTracked = _nextLevel;

            return LevelTracked != null;
        }
        private void UpdateView(float delay = 0f, float time = 0f)
        {
            tween =
            DOTween.To(() => sliderProgress.value, _progress =>
            {
                UpdateViewProgress(_progress);
            }, Progress, time);

            tween
                .SetDelay(delay)
                .OnStart(() =>
                {
                    audioOnStart.Play(ProcessorSoundPool.PoolLevel.Global);
                    audioSourceLoopIncrease = audioLoopIncrease.Play(ProcessorSoundPool.PoolLevel.Global);
                })
                .OnUpdate(() =>
                {
                    if (sliderProgress.value == 1f)
                        tween.Complete(true);
                })
                .onComplete = () =>
                {
                    if (LevelTracked.Unlocked)
                    {
                        imagePreview.color = new Color(1f, 1f, 1f, 0f);
                        imagePreview.transform.localScale = Vector3.one * 1.5f;
                        imagePreview.DOColor(Color.white, 0.5f);
                        imagePreview.transform.DOScale(1f, 0.5f);
                        imagePreview.gameObject.SetActive(true);
                        audioOnUnlock.Play(ProcessorSoundPool.PoolLevel.Global);
                    }
                    audioSourceLoopIncrease.Stop();
                };
        }
        private void UpdateViewProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);
            sliderProgress.value = progress;
            textProgress.text = ((int)(progress * 100f)).ToString() + '%';
        }
    }
}
