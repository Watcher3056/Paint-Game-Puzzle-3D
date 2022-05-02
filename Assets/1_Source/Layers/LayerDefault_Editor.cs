using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamAlpha.Source
{
    public partial class LayerDefault
    {
#if UNITY_EDITOR
        private void _EditorCurLevelDecrease()
        {
            _EditorCurLevelIndex--;
            _EditorUpdateCurLevelIndex();
        }
        private void _EditorCurLevelIncrease()
        {
            _EditorCurLevelIndex++;
            _EditorUpdateCurLevelIndex();
        }
        public void _EditorUpdateCurLevelIndex()
        {
            UnityEditor.EditorUtility.SetDirty(gameObject);
            if (_EditorCurLevelIndex >= AllLevels.Count)
                _EditorCurLevelIndex = 0;
            else if (_EditorCurLevelIndex < 0)
                _EditorCurLevelIndex = AllLevels.Count - 1;
            if (!Application.isPlaying)
            {
                EnableSelectedLevel(_EditorCurLevelIndex);
                return;
            }
            else
            {
                curLevelIndex = _EditorCurLevelIndex;
                Restart();
            }
        }
        private void _EditorOnLevelsChanged()
        {
            foreach (Level level in AllLevels)
            {
                if (level.prefab != null)
                {
                    List<Level> levelsSameGUID =
                        AllLevels.FindAll(l => l.prefab != null &&
                    l.prefab.guid.id.Equals(level.prefab.guid.id));
                    level._invalid = levelsSameGUID.Count > 1;
                    if (levelsSameGUID.Count > 1)
                    {
                        foreach (Level l in levelsSameGUID)
                        {
                            Debug.LogError(l.prefab.name + " " + l.prefab.guid.id);
                        }
                    }
                }
            }
        }
        [Sirenix.OdinInspector.Button]
        private void CleanProgress()
        {
            ProcessorSaveLoad.CleanSaves();
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                Restart();
        }
#endif
    }
}
