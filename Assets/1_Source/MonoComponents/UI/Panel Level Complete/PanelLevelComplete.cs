using Pixeye.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using Animancer;
using TMPro;
using DG.Tweening;
using System.Collections;

namespace TeamAlpha.Source
{
    public class PanelLevelComplete : MonoBehaviour
    {
        public static PanelLevelComplete Default { get; private set; }

        [Required]
        public Panel panel;

        public PanelLevelComplete() => Default = this;
        private void Awake()
        {
            panel.OnPanelShow += HandlePanelShow;
            panel.OnPanelHide += HandlePanelHide;
        }
        private void HandleButtonCountinueClick()
        {
            LayerDefault.Default.Restart();
        }
        private void HandlePanelShow()
        {

        }
        private void HandlePanelHide()
        {

        }
    }
}
