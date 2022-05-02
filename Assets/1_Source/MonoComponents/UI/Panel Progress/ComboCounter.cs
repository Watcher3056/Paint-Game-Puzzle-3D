using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeamAlpha.Source
{
    public class ComboCounter : MonoBehaviour
    {
        public static ComboCounter Default { get; private set; }

        [Required]
        public Slider sliderProgressBar;
        [Required]
        public TextMeshProUGUI textCounter;
        public List<AudioSimpleData> audioCombo;
        public float decreaseSpeed;
        public float increaseSpeed;
        public float increaseForEveryBlock;

        private float progress;
        private float maxProgressReached;
        private int diamondsAchieved;

        public ComboCounter() => Default = this;
        private void Start()
        {
            sliderProgressBar.minValue = 0f;
            sliderProgressBar.maxValue = 1f;
        }
        private void Update()
        {
            progress -= decreaseSpeed * Time.deltaTime;
            progress = Mathf.Clamp(progress, 0f, 7f);
            if (progress > maxProgressReached)
            {
                if ((int)progress > (int)maxProgressReached)
                {
                    audioCombo[(int)progress - 1].Play(ProcessorSoundPool.PoolLevel.GameLevel);
                    textCounter.transform.DOScale(1.5f, 0.25f).onComplete = () =>
                    {
                        textCounter.transform.DOScale(1f, 0.25f);
                    };
                    PanelTop.Default.AddDiamonds((int)progress, textCounter.transform.position);
                    diamondsAchieved += (int)progress;
                    LevelController.Current.LevelStats.diamondsAchievedByCombo = diamondsAchieved;
                }
                maxProgressReached = progress;
            }
            float diff = progress % 1 - sliderProgressBar.value;
            float maxChange = increaseSpeed * Time.deltaTime;
            sliderProgressBar.value += Mathf.Clamp(diff, -maxChange, maxChange);
            if ((int)progress > 0)
                textCounter.text = 'x' + ((int)progress).ToString();
            else
                textCounter.text = "";
        }
        public void HandleBlockPainted()
        {
            progress += increaseForEveryBlock;
        }
        public void ResetCounter()
        {
            progress = 0f;
            maxProgressReached = 0f;
            sliderProgressBar.value = 0f;
            diamondsAchieved = 0;
        }
    }
}
