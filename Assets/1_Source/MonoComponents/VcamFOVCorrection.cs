using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VcamFOVCorrection : MonoBehaviour
{
    public static float TargetUIAspect => 1080f / 1920f;
    public static float CurUIAspect => (float)Screen.width / (float)Screen.height;
    public float OrthographicWidth => vcam.m_Lens.OrthographicSize * vcam.m_Lens.Aspect;

    private CinemachineVirtualCamera vcam;
    private void Start()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        ConfigureCamera();
    }

    private void ConfigureCamera()
    {
        float multiplier = TargetUIAspect / CurUIAspect;
        vcam.m_Lens.FieldOfView = Mathf.Clamp(vcam.m_Lens.FieldOfView * multiplier, vcam.m_Lens.FieldOfView - 10f, vcam.m_Lens.FieldOfView + 10f);
    }
}
