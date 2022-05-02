using Pixeye.Actors;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace TeamAlpha.Source
{
    [Serializable]
    public class ComponentGUID
    {
        public enum IdSourceType { None, Prefab, Random }
        [Required, ReadOnly]
        public string id;
        [OnValueChanged("UpdateGUID"), InfoBox("GUID Source is required", Sirenix.OdinInspector.InfoMessageType.Error, VisibleIf = "ErrorVisible")]
        public IdSourceType sourceType;

        [ReadOnly, ShowIf("prefabId")]
        public string prefabId;
        [ReadOnly, ShowIf("randomId")]
        public string randomId;
        public Guid GetGuid()
        {
            return new Guid(id);
        }
        [Sirenix.OdinInspector.Button]
        public void RenerateGUID()
        {
            randomId = null;
            UpdateGUID();
        }
        public void UpdateGUID()
        {
            if (sourceType == IdSourceType.Prefab)
                id = prefabId;
            else if (sourceType == IdSourceType.Random)
            {
                if (randomId == null || randomId == String.Empty)
                    randomId = Guid.NewGuid().ToString().Replace("-", "");
                id = randomId;
            }
        }
        private bool ErrorVisible() => sourceType == IdSourceType.None;
    }

    #region HELPERS
    static partial class Component
    {
        public const string GUID = "TeamAlpha.Source.ComponentGUID";

        public static ref ComponentGUID ComponentGUID(in this ent entity) =>
        ref Storage<ComponentGUID>.components[entity.id];
    }

    sealed class StorageComponentGUID : Storage<ComponentGUID>
    {
        public override ComponentGUID Create() => new ComponentGUID();


        // Use for cleaning components that were removed at the current frame.
        public override void Dispose(indexes disposed)
        {
            foreach (var id in disposed)
            {
                ref var component = ref components[id];
            }
        }
    }
    #endregion
}




