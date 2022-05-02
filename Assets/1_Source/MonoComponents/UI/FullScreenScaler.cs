using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Pixeye.Actors;
using Sirenix.OdinInspector;

namespace TeamAlpha.Source
{
    public class FullScreenScaler : MonoBehaviour
    {
        [FoldoutGroup("Setup", Expanded = true)]
        public Transform transformToScale;
        [FoldoutGroup("Setup")]
        public Image mainImage;

        private void Start()
        {
            HandleOrientationChanged();
        }
        private void OnEnable()
        {
            ProcessorOrientation.OnOrientationChanged += HandleOrientationChanged;
            HandleOrientationChanged();
        }
        private void OnDisable()
        {
            ProcessorOrientation.OnOrientationChanged -= HandleOrientationChanged;
        }
        private void HandleOrientationChanged()
        {
            float newScale = 0;
            newScale = (float)Screen.height /
                (((float)Screen.width / (float)mainImage.sprite.rect.width) * mainImage.sprite.rect.height);

            transformToScale.localScale = new Vector3(newScale, newScale, 1f);
        }
    }
}
