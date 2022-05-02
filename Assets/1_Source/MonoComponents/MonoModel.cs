using Cinemachine;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamAlpha.Source
{
    public class MonoModel : MonoBehaviour
    {
        [Serializable]
        public class PartType
        {
            public string Name => String.Format("{0}: {1}", orderIndex, material != null ? material.name : String.Empty);
            [ReadOnly, HideLabel]
            public ComponentGUID guid = new ComponentGUID { sourceType = ComponentGUID.IdSourceType.Random };
            [Required, OnValueChanged("OdinOnValueChanged")]
            public Material material;
            [OnValueChanged("OdinOnValueIsDefaultChanged")]
            public bool isDefault;
            public bool IsBorder => material == DataGameMain.Default.matBorder;

            [HideInInspector]
            public Color prevColor = Color.clear;
            [HideInInspector]
            public Transform holder;
            [HideInInspector]
            public int orderIndex;
#if UNITY_EDITOR
            private void OdinOnValueChanged()
            {
                _editorModel.UpdateView();
                OdinUpdateDefaultType();
            }
            private void OdinOnValueIsDefaultChanged()
            {
                OdinUpdateDefaultType();
            }
            private void OdinUpdateDefaultType()
            {
                PartType defaultType = _editorModel.types.Find(t => t.isDefault);
                if (defaultType == null && _editorModel.types.Count > 0)
                    _editorModel.types[0].isDefault = true;
                else
                    foreach (PartType type in _editorModel.types)
                        if (type != this)
                            type.isDefault = false;
            }
            [HideInInspector]
            public MonoModel _editorModel;
#endif
        }
        [InfoBox("$_OdinPartsCount")]
        [OnValueChanged("OdinOnTypesListChanged")]
        public List<PartType> types;
        public float offset = 1f;
        [NonSerialized, ShowInInspector, OnValueChanged("_OdinOnChangePartMeshChanged")]
        public Mesh changePartMesh;
        public float scaleInGame = 1f;
        public bool customPreviewScale;
        [ShowIf("customPreviewScale")]
        public float previewScale = 1f;
        private string _OdinPartsCount => "Parts Count: " + parts.Count.ToString();


        public static event Action OnModelCompleted = () => { };
        public static event Action OnTypeCompletedOrChanged = () => { };

        public PartType CurTypeInOrder => lastTypeInOrder;

        public int PartsPaintedCorrect => PartsPlayable.Count(p => p.TypePainted.guid.id == p.TypeOrigin.guid.id);

        public PartType TypeDefault
        {
            get
            {
                if (typeDefault == null)
                    typeDefault = types.Find(t => t.isDefault);
                return typeDefault;
            }
        }
        private PartType typeDefault;
        public List<ModelPart> PartsPlayable
        {
            get
            {
                if (partsPlayable == null)
                    partsPlayable = parts.FindAll(p => !p.TypeOrigin.IsBorder);
                return partsPlayable;
            }
        }
        private List<ModelPart> partsPlayable;
        [SerializeField, HideInInspector]
        public List<ModelPart> parts = new List<ModelPart>();
        [SerializeField, HideInInspector]
        public Bounds bounds;
        [ReadOnly, PreviewField, Required, AssetsOnly]
        public Sprite spritePreview;
        private int lastPartIndexInOrder;
        private PartType lastTypeInOrder;
        private bool outLineEnabled;

#if UNITY_EDITOR
        private string previewFileName => "Preview " + name + ".png";
        private void OdinOnTypesListChanged()
        {
            foreach (PartType type in types)
            {
                type.orderIndex = types.IndexOf(type);
                type._editorModel = this;
            }
            UpdateHolders();
        }
        private void _OdinOnChangePartMeshChanged()
        {
            if (changePartMesh == null)
                return;
            _EditorChangePartMesh(changePartMesh);
        }
        public void _EditorChangePartMesh(Mesh mesh)
        {
            foreach (ModelPart part in parts)
                part.GetComponent<MeshFilter>().mesh = mesh;
        }
        [Button]
        private void ValidateModel()
        {
            RemoveClosestPartsFast();
            CenterOnPivot();
            UpdateAllMaterialsShadowSettings();
            UpdatePreview();
        }
        [Sirenix.OdinInspector.Button]
        private void Migrate()
        {
            RemoveWrong();
            foreach (ModelPart part in parts)
                part.transform.SetParent(transform);
            foreach (PartType type in types)
                type.holder.localPosition = Vector3.zero;
            foreach (ModelPart part in parts)
                part.transform.SetParent(part.TypeOrigin.holder);
        }
        [ShowInInspector, FoldoutGroup("Remove Closest Parts")]
        private float _odinDistanceToRemove;
        private void RemoveClosestPartsFast()
        {
            RemoveClosestParts(_odinDistanceToRemove, 2);
        }
        private void RemoveClosestPartsSlow()
        {
            RemoveClosestParts(_odinDistanceToRemove, 10);
        }
        private void CenterOnPivot()
        {
            transform.position = Vector3.zero;
            Vector3 positionPivot = transform.position;
            Vector3 positionCenter = Vector3.zero;
            UpdateBounds();
            positionCenter = bounds.center;
            Vector3 diff = positionPivot - positionCenter;
            foreach (PartType type in types)
            {
                type.holder.transform.position += diff;
            }
        }
        private void UpdateBounds()
        {
            bounds = new Bounds();
            foreach (ModelPart part in parts)
                bounds.Encapsulate(part.transform.position);
        }
        private void UpdateAllMaterialsShadowSettings()
        {
            foreach (PartType type in types)
                UpdateMaterialShadowSettings(type);
        }
        private void RemoveClosestParts(float maxDistanceToRemove, int maxCheck)
        {
            if (UnityEditor.PrefabUtility.GetPrefabAssetType(gameObject) != UnityEditor.PrefabAssetType.NotAPrefab)
            {
                Debug.LogError("Wrong parts will be not removed. Open prefab in Prefab Edit mode!");
                return;
            }
            int prevRemoved = 0;
            int removed = 0;
            try
            {


            check:
                RemoveWrong();
                List<ModelPart> partsSortedByDistance = new List<ModelPart>(parts.ToArray());
                partsSortedByDistance.Sort((p1, p2) => p1.transform.position.sqrMagnitude.CompareTo(p2.transform.position.sqrMagnitude));

                for (int i = 0; i < parts.Count - 1; i++)
                {
                    ModelPart curPart = partsSortedByDistance[i];

                    int curPartIndexInSorted = partsSortedByDistance.IndexOf(curPart);
                    int startSearchIndex = Mathf.Clamp(curPartIndexInSorted, 0, parts.Count - 1);
                    int endSearchIndex = Mathf.Clamp(i + maxCheck, 0, parts.Count);
                    int nearestIndex =
                        partsSortedByDistance.FindIndex(startSearchIndex, endSearchIndex - startSearchIndex,
                        p => Vector3.Distance(p.transform.position, curPart.transform.position) <= maxDistanceToRemove && p != curPart);

                    if (nearestIndex >= 0)
                    {
                        ModelPart nearest = partsSortedByDistance[nearestIndex];
                        parts.RemoveAt(i);
                        partsSortedByDistance.Remove(curPart);
                        DestroyImmediate(curPart.gameObject);
                        i--;
                        removed++;
                    }
                }
                if (removed != prevRemoved)
                {
                    prevRemoved = removed;
                    goto check;
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }

            Debug.Log(String.Format("Removed {0} parts!", removed));
        }
        private void UpdatePreview()
        {
            if (spritePreview != null)
            {
                string path = UnityEditor.AssetDatabase.GetAssetPath(spritePreview);
                if (path != null && path != string.Empty)
                    UnityEditor.AssetDatabase.DeleteAsset(path);
                spritePreview = null;
            }
            Camera cameraMain = FindObjectOfType<Camera>();
            Canvas tempCanvas = new GameObject("Temp Canvas").AddComponent<Canvas>();
            tempCanvas.renderMode = RenderMode.ScreenSpaceCamera;

            Camera cameraTemp = Instantiate(cameraMain.gameObject).GetComponent<Camera>();
            DestroyImmediate(cameraTemp.GetComponent<CinemachineBrain>());

            DoSnapshot(gameObject, tempCanvas, cameraTemp);
            DestroyImmediate(tempCanvas.gameObject);
            DestroyImmediate(cameraTemp.gameObject);
        }
        private void DoSnapshot(GameObject origin, Canvas canvas, Camera cam)
        {
            origin.SetActive(false);
            MonoModel ins = GameObject.Instantiate(origin, canvas.transform, false).GetComponent<MonoModel>();
            MeshFilter primitive = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshFilter>();
            ins._EditorChangePartMesh(primitive.sharedMesh);
            DestroyImmediate(primitive.gameObject);

            ins.gameObject.SetActive(true);
            ins.transform.position = canvas.transform.position;
            ins.transform.localScale = Vector3.one * (customPreviewScale ? previewScale : scaleInGame);


            cam.transform.position = canvas.transform.position + Vector3.up * 5f;
            cam.transform.LookAt(ins.transform);
            cam.transform.localEulerAngles += Vector3.forward * 180f;

            string pathToModel = gameObject.GetAssetPath();
            string pathToFolder = pathToModel.Replace(pathToModel.Substring(pathToModel.LastIndexOf('/')), "") + "/Preview";
            string pathToPreviewSprite = pathToFolder + "/" + previewFileName;
            string absolutePathToFolder = Application.dataPath + pathToFolder.Replace("Assets", "");
            absolutePathToFolder = absolutePathToFolder.Replace("/", "\\");
            string fileName = absolutePathToFolder + "/" + previewFileName;
            FileInfo info = new FileInfo(fileName);
            if (info.Exists)
                File.Delete(fileName);
            else if (!info.Directory.Exists)
                info.Directory.Create();

            RenderTexture renderTarget = RenderTexture.GetTemporary(128, 128);
            cam.aspect = 1f;
            cam.orthographic = true;
            cam.orthographicSize = 7f;
            cam.targetTexture = renderTarget;
            cam.allowHDR = true;
            cam.allowMSAA = true;
            cam.Render();

            RenderTexture.active = renderTarget;
            Texture2D tex = new Texture2D(renderTarget.width, renderTarget.height);
            tex.ReadPixels(new Rect(0, 0, renderTarget.width, renderTarget.height), 0, 0);

            File.WriteAllBytes(fileName, tex.EncodeToPNG());
            UnityEditor.AssetDatabase.ImportAsset(pathToPreviewSprite, UnityEditor.ImportAssetOptions.ForceSynchronousImport);
            Texture2D texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(pathToPreviewSprite);
            UnityEditor.AssetImporter assetImporter = UnityEditor.TextureImporter.GetAtPath(pathToPreviewSprite);
            UnityEditor.TextureImporter importer = assetImporter as UnityEditor.TextureImporter;
            importer.isReadable = true;
            importer.textureType = UnityEditor.TextureImporterType.Sprite;
            importer.SaveAndReimport();

            spritePreview = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(pathToPreviewSprite);

            cam.targetTexture = null;
            DestroyImmediate(ins.gameObject);
            origin.SetActive(true);
        }

#endif
        private void Start()
        {
            //UpdateView();
        }
        private void UpdateView()
        {
            UpdateHolders();
            RemoveWrong();

            for (int i = 0; i < parts.Count; i++)
            {
                ModelPart part = parts[i];
                part.UpdateView();
            }
        }
        private void RemoveWrong()
        {
            for (int i = 0; i < parts.Count; i++)
            {
                ModelPart part = parts[i];
                if (part == null || (part != null && part.owner != this))
                {
                    parts.Remove(part);
                    i--;
                    continue;
                }
            }
        }
        private void UpdateHolders()
        {
            foreach (PartType type in types)
            {
                if (type.holder == null)
                {
                    type.holder = new GameObject(type.Name).transform;
                }
                type.holder.name = type.Name;
                type.holder.SetParent(transform);
                type.holder.localPosition = Vector3.zero;
                type.holder.SetAsLastSibling();
            }
#if UNITY_EDITOR
            CenterOnPivot();
#endif
        }
        private void InvokeOnModelCompleted()
        {
            OnModelCompleted.Invoke();
        }
        public void OnDestroy()
        {
            foreach (ModelPart part in parts)
                if (part != null)
                    Destroy(part.gameObject);
        }
        public void CheckPart(ModelPart part)
        {
            if (parts == null)
                parts = new List<ModelPart>();
            if (part.owner == this && !parts.Contains(part))
                parts.Add(part);
            if (part.transform.parent != part.TypeOrigin.holder)
                part.transform.SetParent(part.TypeOrigin.holder);

            Vector3 gridPosition = part.transform.localPosition;
            float diff = (gridPosition.x % offset);
            gridPosition.x = Mathf.Round(gridPosition.x / offset) * offset;
            diff = (gridPosition.y % offset);
            gridPosition.y = Mathf.Round(gridPosition.y / offset) * offset;
            diff = (gridPosition.z % offset);
            gridPosition.z = Mathf.Round(gridPosition.z / offset) * offset;
            part.transform.localPosition = gridPosition;
        }
        public void AddPart(ModelPart part)
        {
            ModelPart _partLastInOrder = parts[lastPartIndexInOrder];
            //part.Outline.enabled = false;

            part.transform.SetParent(part.TypeOrigin.holder, true);

            part.transform.DOScale(0.25f, 0.5f);
            part.transform.DOLocalRotate(Vector3.zero, 0.25f);
            part.transform.DOLocalMove(_partLastInOrder.startLocalPosition, 0.25f);
            lastPartIndexInOrder++;
            if (parts.Count - lastPartIndexInOrder <= 50 && !outLineEnabled)
            {
                outLineEnabled = true;
                for (int i = 0; i < parts.Count; i++)
                {
                    ModelPart _part = parts[i];
                    //_part.Outline.enabled = true;
                }
            }
            if (lastPartIndexInOrder >= parts.Count)
            {
                int indexNextTypeInOrder = types.IndexOf(lastTypeInOrder) + 1;
                if (indexNextTypeInOrder < types.Count)
                {
                    lastTypeInOrder = types[indexNextTypeInOrder];
                    lastPartIndexInOrder = 0;
                }
                else
                {
                    InvokeOnModelCompleted();
                }
            }
        }
        public void PaintPartsToDefault()
        {
            PartType defaultType = types.Find(t => t.isDefault);
            foreach (ModelPart part in parts)
            {
                if (part.TypeOrigin.IsBorder)
                    continue;
                part.PaintInstant(defaultType);
            }
        }
        public bool IsTypeUsed(PartType type)
        {
            return parts.Exists(p => p.TypeGUID.Equals(type.guid.id));
        }
        public void EnableHint()
        {
            foreach (ModelPart part in parts)
            {
                if (part.TypeOrigin.IsBorder)
                    continue;
                part.EnableHint();
            }
        }
        public void LaunchScanEffect(float speed)
        {
            StartCoroutine(_LaunchScanEffect(speed));
        }
        private IEnumerator _LaunchScanEffect(float speed)
        {
            float distance = transform.position.y - 5f;
            string varName = "_distance";
            while (distance < 15f)
            {
                distance += Time.deltaTime * speed;
                foreach (ModelPart part in parts)
                {
                    part.scanPlate.material.SetFloat(varName, distance);
                }
                yield return null;
            }
        }
        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
                return;
            foreach (PartType type in types)
            {
                if (type.material != null && !type.prevColor.Equals(type.material.color))
                {
                    UpdateMaterialShadowSettings(type);
                }
            }
        }
        private void UpdateMaterialShadowSettings(PartType type)
        {
            Color color = type.material.color;
            //float colorBrigthness = (0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b) / (255f * 3f);
            float colorBrigthness = (color.r + color.g + color.b) / 3f;
            type.material.SetFloat("_SelfShadingSize", Mathf.Lerp(0.6f, 0.7f, colorBrigthness));
            type.material.SetFloat("_ShadowEdgeSize", Mathf.Lerp(0.4f, 0.05f, colorBrigthness));
            type.material.SetFloat("_Flatness", Mathf.Lerp(0.25f, 1f, colorBrigthness));
            type.prevColor = color;
        }
    }
}
