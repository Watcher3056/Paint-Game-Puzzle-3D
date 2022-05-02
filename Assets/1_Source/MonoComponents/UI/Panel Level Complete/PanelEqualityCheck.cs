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
    public class PanelEqualityCheck : MonoBehaviour
    {
        public static PanelEqualityCheck Default { get; private set; }

        [Required]
        public Panel panel;
        [Required]
        public TextMeshProUGUI textResult;
        [Required]
        public Slider slider;
        [Required]
        public Image imageEmoji;
        [Required]
        public Image imageFill;
        [Required]
        public Sprite spriteEmojiBad;
        [Required]
        public Sprite spriteEmojiGood;
        [Required]
        public Sprite spriteFillBad;
        [Required]
        public Sprite spriteFillGood;

        public PanelEqualityCheck() => Default = this;

        private void Awake()
        {
            panel.OnPanelShow += HandlePanelShow;
        }
        private void HandlePanelShow()
        {
            UpdateView(0f);
            DOTween.To(() => 0f, equality =>
            {
                UpdateView(equality);
            }, LevelController.Current.Equality, 2f);
        }
        private void UpdateView(float equality)
        {
            slider.value = equality;
            textResult.text = ((int)(equality * 100f)).ToString() + '%';
            if (equality >= 0.5f)
            {
                imageEmoji.sprite = spriteEmojiGood;
                imageFill.sprite = spriteFillGood;
            }
            else
            {
                imageEmoji.sprite = spriteEmojiBad;
                imageFill.sprite = spriteFillBad;
            }
        }
    }
}
