using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static TeamAlpha.Source.MonoModel;

namespace TeamAlpha.Source
{
    public class ColorPickerCell : MonoBehaviour
    {
        [Required]
        public Image image;
        [Required]
        public Button button;
        [NonSerialized]
        public PartType colorType;
    }
}
