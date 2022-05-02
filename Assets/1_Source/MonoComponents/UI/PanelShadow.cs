using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TeamAlpha.Source
{
    public class PanelShadow : MonoBehaviour
    {
        public static PanelShadow Default { get; private set; }

        [Required]
        public Panel panel;
        [Required]
        public Button button;

        public PanelShadow() => Default = this;
        private void Start()
        {
            button.onClick.AddListener(HandleClick);
        }
        private void HandleClick()
        {
            if (Panel.UpperShaded != null && Panel.UpperShaded.closeOnShadowClick)
                Panel.UpperShaded.ClosePanel();
        }
    }
}