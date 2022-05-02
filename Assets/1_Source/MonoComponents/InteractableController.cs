using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using Pixeye.Actors;
using UnityEngine.EventSystems;
using UnityEngine;

namespace TeamAlpha.Source
{
    public class InteractableController : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler,
        IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {

        #region SETTINGS
        public bool limitSpeed;
        public bool rotating;
        public bool physicsMove;
        [MinValue(0f), ShowIf("limitSpeed")]
        public float speed;
        #endregion

        #region RUNTIME

        public bool Interactable
        {
            set
            {
                interactable = value;
                if (collider)
                    collider.enabled = value;
                isDrag = false;
            }
            get => interactable;
        }
        public static readonly string EventKeyOnClick = "EventKeyOnClick";
        public static readonly string EventKeyOnDragStart = "EventKeyOnDragStart";
        public static readonly string EventKeyOnDragEnd = "EventKeyOnDragEnd";
        public bool IsDrag => isDrag;
        public event Action OnDragStart = () => { };
        public event Action OnDragEnd = () => { };
        public event Action OnClick = () => { };

        [HideInInspector]
        public new Collider2D collider;
        [HideInInspector]
        public new Rigidbody2D rigidbody2D;
        [ShowInInspector, ReadOnly, DetailedInfoBox("Troubleshoot...", "Require collider2D(trigger) + SpriteRenderer or Image")]
        private bool isDrag;
        [ShowInInspector, ReadOnly]
        private bool interactable;

        public bool PositionChanged { get; private set; }
        public bool RotationChanged { get; private set; }
        private Vector2 startDragMousePos;
        private Vector2 startBodyPos;
        private Vector2 prevBodyPos;
        private Vector2 cameraPosOnPointerDown;
        private RectTransform rect;
        private Canvas canvas;
        private float startAngleDiff;
        private float prevAngle;
        #endregion
        private void OnEnable()
        {
            this.Log("Enabled");
            prevBodyPos = transform.localPosition;
            prevAngle = transform.rotation.z;
            canvas = GetComponentInParent<Canvas>();
            collider = GetComponent<Collider2D>();
            rigidbody2D = GetComponent<Rigidbody2D>();
            rect = GetComponent<RectTransform>();
        }
        private void Update()
        {
            try
            {

                Vector2 curPos = Vector2.zero;
                if (canvas != null)
                {
                    curPos = rect.transform.localPosition;
                    RotationChanged = prevAngle != rect.transform.rotation.z;
                    prevAngle = rect.transform.rotation.z;
                }
                else
                {
                    curPos = transform.localPosition;
                    RotationChanged = prevAngle != transform.rotation.z;
                    prevAngle = transform.rotation.z;
                }
                PositionChanged = prevBodyPos != curPos;
                prevBodyPos = curPos;

                if (!isDrag || !interactable)
                {
                    return;
                }
                if (canvas != null)
                {
                    Vector2 move = UIHelper.GetScreenSpaceOverlayMousePosition(canvas) - startDragMousePos - ((Vector2)(rect.transform.position) - startBodyPos);

                    if (limitSpeed)
                    {
                        Vector2 distToMouse = UIHelper.GetScreenSpaceOverlayMousePosition(canvas) - (Vector2)rect.transform.position;
                        move = (Vector3)move.normalized * LayerDefault.DeltaTime * speed;
                        if (move.magnitude > distToMouse.magnitude)
                            move = distToMouse;
                    }
                    rect.transform.position += (Vector3)move;
                }
                else
                {
                    Vector2 mousePosition = CameraManager.Default.cam.ScreenToWorldPoint(Input.mousePosition);
                    if (rotating)
                    {
                        float angleToMouse = MathHelper.SignedAngle2D(transform.position, mousePosition, transform.position + transform.up);
                        transform.eulerAngles += Vector3.back * (angleToMouse - startAngleDiff);
                    }
                    else
                    {
                        Vector2 distToDest = mousePosition - startDragMousePos - ((Vector2)transform.position - startBodyPos);
                        Vector2 move = Vector2.zero;

                        if (limitSpeed)
                            move = distToDest.normalized * LayerDefault.DeltaTime * speed;
                        else
                            move = distToDest;
                        if (move.magnitude > distToDest.magnitude)
                            move = distToDest;

                        Vector2 newPos = transform.position + (Vector3)move;
                        if (rigidbody2D && physicsMove)
                            rigidbody2D.MovePosition(newPos);
                        else
                            transform.position = newPos;
                    }
                }
            }
            catch (Exception ex)
            {
                this.LogError(ex.Message);
            }
        }
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (canvas != null)
            {
                startBodyPos = rect.transform.position;
                startDragMousePos = UIHelper.GetScreenSpaceOverlayMousePosition(canvas);
            }
            else
            {
                startBodyPos = transform.position;
                startDragMousePos = CameraManager.Default.cam.ScreenToWorldPoint(Input.mousePosition);
                startAngleDiff = MathHelper.SignedAngle2D(startBodyPos, startDragMousePos, startBodyPos + (Vector2)transform.up);
            }
            if (!(limitSpeed && speed == 0) || rotating)
            {
                isDrag = true;
            }
            OnDragStart();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDrag = false;
            OnDragEnd();
        }

        public void OnDrag(PointerEventData eventData)
        {

        }

        public void OnPointerClick(PointerEventData eventData)
        {

        }

        public void OnPointerDown(PointerEventData eventData)
        {
            cameraPosOnPointerDown = CameraManager.Default.cam.transform.position;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (interactable &&
                Mathf.Abs(cameraPosOnPointerDown.x -
                CameraManager.Default.cam.transform.position.x) < 0.1f)
            {
                if ((canvas != null && !isDrag) || canvas == null)
                    OnClick.Invoke();
            }
            else
                this.Log("Is not interactable!");
        }
    }
}
