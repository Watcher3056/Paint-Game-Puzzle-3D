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
    public class Toggle : MonoBehaviour
    {
        [Required]
        public UnityEngine.UI.Toggle toggle;
        [Required]
        public Image imageBG;
        [Required]
        public Image imageHandle;
        public float offsetHandle;
        public Color colorEnabledBG;
        public Color colorDisabledBG;
        public Color colorEnabledHandle;
        public Color colorDisabledHandle;

        private void Awake()
        {
            toggle.onValueChanged.AddListener(HandleToggleValueChanged);
            HandleToggleValueChanged(toggle.isOn);
        }
        private void HandleToggleValueChanged(bool arg)
        {
            DataGameMain.Default.audioOnButtonClick.Play(ProcessorSoundPool.PoolLevel.Global);

            imageBG.DOKill();
            imageHandle.DOKill();

            imageBG.DOColor(arg ? colorEnabledBG : colorDisabledBG, 0.5f);
            imageHandle.DOColor(arg ? colorEnabledHandle : colorDisabledHandle, 0.5f);
            imageHandle.transform.DOLocalMoveX(arg ? offsetHandle : -offsetHandle, 0.5f);
        }
    }
}
