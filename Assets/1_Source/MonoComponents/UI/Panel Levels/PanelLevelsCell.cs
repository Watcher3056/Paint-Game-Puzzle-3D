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
    public class PanelLevelsCell : MonoBehaviour
    {
        [Required]
        public Button button;
        [Required]
        public GameObject viewStars;
        [Required]
        public List<GameObject> stars;
        [Required]
        public Slider sliderDiamondsProgress;
        [Required]
        public TextMeshProUGUI textDiamondsProgress;
        [Required]
        public Image imageLevelView;
        [Required]
        public Image imageBG;
        [Required]
        public Image imageUnknown;

        [NonSerialized]
        public Level linkedLevel;

        private void Awake()
        {
            button.onClick.AddListener(HandleButtonClick);
        }
        private void HandleButtonClick()
        {
            DataGameMain.Default.audioOnButtonClick.Play(ProcessorSoundPool.PoolLevel.Global);
            if (linkedLevel == null || !linkedLevel.Unlocked)
                return;
            LayerDefault.Default.LaunchLevel(linkedLevel);
            PanelLevels.Default.panel.ClosePanel();
        }
    }
}
