using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

namespace TeamAlpha.Source
{
    public class DisableOnStart : MonoBehaviour
    {
        [Required]
        public GameObject viewToHide;
        // Use this for initialization
        void Start()
        {
            if (viewToHide != null)
                viewToHide.SetActive(false);
        }
    }
}