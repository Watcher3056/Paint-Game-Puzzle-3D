using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DG.Tweening;
using Pixeye.Actors;
using Sirenix.OdinInspector;
using UnityEngine;
using static TeamAlpha.Source.MonoModel;

namespace TeamAlpha.Source
{
    public class ModelPart : MonoBehaviour
    {
        [ValueDropdown("OdinTypeDropDown", DisableGUIInAppendedDrawer = true), SerializeField, OnValueChanged("UpdateView")]
        [LabelText("Type")]
        private string typeGUID;
        public string TypeGUID => typeGUID;
        [Required]
        public MonoModel owner;
        [Required]
        public new MeshRenderer renderer;
        [HideInInspector]
        public Vector3 startLocalPosition;
        [HideInInspector]
        public new Collider collider;

        [HideInInspector]
        public int order;
        public PartType TypeOrigin => owner.types.Find(t => t.guid.id == typeGUID);
        public PartType TypePainted { get; private set; }
        [NonSerialized]
        public SpriteRenderer scanPlate;

        //public Outline Outline
        //{
        //    get
        //    {
        //        if (outline == null)
        //        {
        //            startLocalPosition = transform.localPosition;
        //            outline = gameObject.AddComponent<Outline>();
        //            Color color = Color.black;
        //            color.a = 0.5f;
        //            float h = 0f, s = 0f, v = 0f;
        //            Color.RGBToHSV(color, out h, out s, out v);
        //            color = Color.HSVToRGB(h, s, v);
        //            outline.OutlineColor = color;
        //            outline.OutlineWidth = 10f;
        //            outline.OutlineMode = Outline.Mode.OutlineAndSilhouette;
        //            outline.enabled = false;
        //        }
        //        return outline;
        //    }
        //}
        //[NonSerialized]
        //private Outline outline;

        private bool hintEnabled;
        private float hintTimeBuffer;
        private const float hintSpeed = 1f;
        private bool hintDirectionPositive;
        private void Awake()
        {
            scanPlate =
                Instantiate(DataGameMain.Default.prefabScanPlate.gameObject, transform).GetComponent<SpriteRenderer>();
            scanPlate.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            scanPlate.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
            scanPlate.material.SetFloat("_distance", transform.position.y - 15f);
        }
        private IEnumerable OdinTypeDropDown
        {
            get
            {
                ValueDropdownList<string> types = new ValueDropdownList<string>();
                foreach (PartType partType in owner.types)
                {
                    types.Add(new ValueDropdownItem<string>
                    {
                        Text = partType.Name,
                        Value = partType.guid.id
                    });
                }
                return types;
            }
        }
        private void Start()
        {
            startLocalPosition = transform.localPosition;
            collider = GetComponent<Collider>();
        }
        private void Update()
        {
            if (!hintEnabled)
                return;
            //if (TypePainted.guid.id.Equals(TypeOrigin.guid.id))
            //    return;
            if (hintTimeBuffer >= 1f)
                hintDirectionPositive = false;
            else if (hintTimeBuffer <= 0f)
                hintDirectionPositive = true;

            if (hintDirectionPositive)
                hintTimeBuffer += UnityEngine.Time.deltaTime;
            else
                hintTimeBuffer -= UnityEngine.Time.deltaTime;

            Color color = Color.Lerp(TypePainted.material.color, TypeOrigin.material.color, hintTimeBuffer);
            Color endColorShadow =
                Color.Lerp(TypePainted.material.GetColor("_ColorDim"),
                TypeOrigin.material.GetColor("_ColorDim"), hintTimeBuffer);
            renderer.material.DOKill();
            renderer.material.color = color;
            renderer.material.SetVector("_ColorDim", endColorShadow);
        }
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                owner.CheckPart(this);
        }
        public void UpdateView()
        {
            renderer.material = TypeOrigin.material;
            owner.CheckPart(this);
        }
        public void EnableHint()
        {
            hintEnabled = true;
        }
        public void Paint(PartType type, float speed)
        {
            TypePainted = type;

            transform.DOKill();
            transform.DOLocalMoveY(0.5f, 1f / speed).onComplete = () =>
            {
                renderer.material.DOKill();
                renderer.material.DOColor(type != null ? type.material.color : Color.white, 1f / speed).onComplete = () =>
                {
                    renderer.material = type.material;
                };
                transform.DOScale(1.25f, 1f / speed).onComplete = () =>
                {
                    transform.DOScale(1f, 1f / speed);
                    transform.DOLocalMoveY(0f, 1f / speed);
                };
            };
        }
        public void PaintInstant(PartType type)
        {
            renderer.material = type.material;
            TypePainted = type;
        }
    }
}
