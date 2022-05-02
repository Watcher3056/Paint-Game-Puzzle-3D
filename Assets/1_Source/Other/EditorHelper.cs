using Pixeye.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamAlpha.Source
{
    [ExecuteInEditMode]
    public class EditorHelper : MonoBehaviour
    {
        public static EditorHelper Default => _default;
        private static EditorHelper _default;

        public DataGameMain DataGameMain
        {
            get
            {
                if (dataGameMain == null)
                    dataGameMain = Resources.Load<DataGameMain>(new SettingsActors().Plugins[0]);
                return dataGameMain;
            }
        }
        [NonSerialized]
        private DataGameMain dataGameMain;
        public EditorHelper() => _default = this;
        private void Awake()
        {
            if (!Application.isEditor)
            {
                Destroy(this);
                return;
            }
        }
    }
}
