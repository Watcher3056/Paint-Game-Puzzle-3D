using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamAlpha.Source
{
    public class GameTheme : ScriptableObject
    {
        public string SAVE_KEY_UNLOCKED => guid.id + ".VIBRATIONS_IS_ENABLED";
        [HideLabel]
        public ComponentGUID guid;
        [Required]
        public Material materialBG;
        [Required, PreviewField]
        public Sprite spritePreview;

        public bool Unlocked
        {
            get
            {
                if (DataGameMain.Default.themesDefault.Contains(this))
                    return true;
                return unlocked;
            }
            set
            {
                unlocked = value;
                ProcessorSaveLoad.Save(SAVE_KEY_UNLOCKED, unlocked);
            }
        }
        private bool unlocked;

        public void Init()
        {
            ProcessorSaveLoad.OnLocalDataUpdated += HandleLocalDataUpdated;

            HandleLocalDataUpdated();
        }
        private void HandleLocalDataUpdated()
        {
            unlocked = ProcessorSaveLoad.Load(SAVE_KEY_UNLOCKED, false);
        }
    }
}
