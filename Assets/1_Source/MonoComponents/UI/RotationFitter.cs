using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;

[ExecuteInEditMode]
#endif
[RequireComponent(typeof(RectTransform))]
public class RotationFitter : MonoBehaviour, ILayoutSelfController
{

    private RectTransform canvasTransform;

    void Awake()
    {
        Canvas canvas = GameObject.FindGameObjectWithTag("Root Canvas").GetComponent<Canvas>();
        canvasTransform = canvas ? canvas.gameObject.transform as RectTransform : null;
        UpdateView();
    }
    public void OnEnable()
    {
        UpdateView();
    }
    public void SetLayoutHorizontal()
    {
        UpdateView();
    }
    public void SetLayoutVertical()
    {
        UpdateView();
    }
    public void UpdateView()
    {
        if (canvasTransform == null)
            return;
        RectTransform rt = transform as RectTransform;
        rt.sizeDelta = new Vector2(canvasTransform.sizeDelta.y, canvasTransform.sizeDelta.x);
    }
#if UNITY_EDITOR
    void Update()
    {
        if (canvasTransform == null)
            canvasTransform = GetComponentInParent<Canvas>().gameObject.transform as RectTransform;

        UpdateView();
    }
#endif
}