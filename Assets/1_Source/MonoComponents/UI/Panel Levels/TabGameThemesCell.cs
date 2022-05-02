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
    public class TabGameThemesCell : MonoBehaviour
    {
        [Required]
        public Button button;
        [Required]
        public Image imagePreview;
        [Required]
        public GameObject viewUnknown;
        [Required]
        public GameObject viewSelected;

        [NonSerialized]
        public GameTheme linkedTheme;

        private void Awake()
        {
            button.onClick.AddListener(HandleButtonClick);
        }
        private void HandleButtonClick()
        {
            DataGameMain.Default.audioOnButtonClick.Play(ProcessorSoundPool.PoolLevel.Global);
            if (TabGameThemes.Default.cellDefaultTheme == this)
            {
                LayerDefault.Default.SetGameTheme(
                    DataGameMain.Default.themesDefault[
                        UnityEngine.Random.Range(0, DataGameMain.Default.themesDefault.Count)]);
            }
            else if (linkedTheme.Unlocked)
            {
                LayerDefault.Default.SetGameTheme(linkedTheme);
            }
            TabGameThemes.Default.UpdateView();
        }
    }
}
