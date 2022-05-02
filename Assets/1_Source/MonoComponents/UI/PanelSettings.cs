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
    public class PanelSettings : MonoBehaviour
    {
        public static PanelSettings Default { get; private set; }

        [Required]
        public Panel panel;
        [Required]
        public Toggle toggleMusic;
        [Required]
        public Toggle toggleSound;
        [Required]
        public Toggle toggleVibrate;
        [Required]
        public Toggle toggleNotifications;
        [Required]
        public Button buttonClose;
        [Required]
        public Button buttonPrivacyPolicy;
        [Required]
        public Button buttonTermsOfUse;

        public PanelSettings() => Default = this;
        private void Start()
        {
            buttonClose.onClick.AddListener(() =>
            {
                DataGameMain.Default.audioOnButtonClick.Play(ProcessorSoundPool.PoolLevel.Global);
                panel.ClosePanel();
            });
            buttonPrivacyPolicy.onClick.AddListener(() =>
            {
                DataGameMain.Default.audioOnButtonClick.Play(ProcessorSoundPool.PoolLevel.Global);
                Application.OpenURL(DataGameMain.Default.URLPrivacyPolicy);
            });
            buttonTermsOfUse.onClick.AddListener(() =>
            {
                DataGameMain.Default.audioOnButtonClick.Play(ProcessorSoundPool.PoolLevel.Global);
                Application.OpenURL(DataGameMain.Default.URLTermsOfUse);
            });

            toggleMusic.toggle.onValueChanged.AddListener(arg =>
            {
                ProcessorSoundPool.Default.MusicIsEnabled = arg;
            });
            toggleSound.toggle.onValueChanged.AddListener(arg =>
            {
                ProcessorSoundPool.Default.SoundIsEnabled = arg;
            });
            toggleVibrate.toggle.onValueChanged.AddListener(arg =>
            {
                DataGameMain.Default.VibrationsIsEnabled = arg;
            });

            ProcessorSaveLoad.OnLocalDataUpdated += HandleLocalDataUpdated;
            HandleLocalDataUpdated();
        }
        private void HandleLocalDataUpdated()
        {
            toggleMusic.toggle.isOn = ProcessorSoundPool.Default.MusicIsEnabled;
            toggleSound.toggle.isOn = ProcessorSoundPool.Default.SoundIsEnabled;
            toggleVibrate.toggle.isOn = DataGameMain.Default.VibrationsIsEnabled;
        }
    }
}
