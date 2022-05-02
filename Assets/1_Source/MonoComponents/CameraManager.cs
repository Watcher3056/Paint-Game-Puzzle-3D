using UnityEngine;
using Sirenix.OdinInspector;
using Cinemachine;
using DG.Tweening;

namespace TeamAlpha.Source
{
    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Default => _default;
        private static CameraManager _default;

        [FoldoutGroup("Setup"), Required]
        public UnityEngine.Camera cam;
        [FoldoutGroup("Setup"), Required]
        public CinemachineBrain cameraBrain;
        [FoldoutGroup("Setup"), Range(0.2f, 1f)]
        public float focusFactor;

        public float OrthographicWidth => CurActiveVCam.m_Lens.OrthographicSize * CurActiveVCam.m_Lens.Aspect;
        public static float TargetUIAspect => 1080f / 1920f;
        public static float CurUIAspect => (float)Screen.width / (float)Screen.height;
        public static Vector2 CurSwipeFrameNormalized => curSwipeFrameNormalized;
        private static Vector2 curSwipeFrameNormalized;
        public static Vector2 CurSwipeNormalized => curSwipeNormalized;
        private static Vector2 curSwipeNormalized;

        public static Vector2 CurSwipeFrame => curSwipeFrameNormalized * new Vector2(Screen.width, Screen.height);
        public static Vector2 CurSwipe => curSwipeNormalized * new Vector2(Screen.width, Screen.height);

        private Vector3 originCamPos;
        private float originCamOrtSize;
        private Transform curCamTarget;
        private Tweener tweenerFocus;
        private Tweener tweenerLookAt;
        public CinemachineVirtualCamera CurActiveVCam
        {
            get => cameraBrain.ActiveVirtualCamera as CinemachineVirtualCamera;
        }

        private Vector2 prevTouchPos;
        public CameraManager()
        {
            _default = this;
        }
        public void Start()
        {
            //originCamPos = CurActiveVCam.transform.position;
            //originCamOrtSize = cam.orthographicSize;
        }
        public void Update()
        {
            if (CurActiveVCam == null)
                return;
            if (!Input.GetMouseButton(0))
            {
                curSwipeFrameNormalized = Vector2.zero;
                curSwipeNormalized = Vector2.zero;
                prevTouchPos = Vector2.zero;
                return;
            }

            Vector2 curTouchPos = Input.mousePosition / new Vector2(Screen.width, Screen.height);
            Vector2 diff = prevTouchPos - curTouchPos;
            if (prevTouchPos != Vector2.zero)
            {
                curSwipeFrameNormalized = diff;
                curSwipeNormalized += diff;
            }

            prevTouchPos = curTouchPos;
        }
    }
}
