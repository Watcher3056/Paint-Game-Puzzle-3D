using System;
using UnityEngine;


namespace TeamAlpha.Source
{

    public class UIHelper
    {
        public static Vector2 GetScreenSpaceOverlayMousePosition(Canvas canvasHolder)
        {
            Vector2 mouseScreenPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle
                    (canvasHolder.transform as RectTransform,
                    Input.mousePosition, canvasHolder.worldCamera,
                    out mouseScreenPos);
            mouseScreenPos = canvasHolder.transform.TransformPoint(mouseScreenPos);

            return mouseScreenPos;
        }
        //public static Vector2 WorldToScreenSpaceOverlayPoint(Canvas canvasHolder, Vector2 worldPoint)
        //{
        //    Vector2 worldScreenPoint;
        //    RectTransformUtility.ScreenPointToLocalPointInRectangle
        //            (canvasHolder.transform as RectTransform,
        //            RectTransformUtility.WorldToScreenPoint(MonoCamera.Default.cam, worldPoint), MonoCamera.Default.cam,
        //            out worldScreenPoint);
        //    worldScreenPoint = canvasHolder.transform.TransformPoint(worldScreenPoint);

        //    return worldScreenPoint;
        //}
    }

}