using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace TeamAlpha.Source
{
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    [RequireComponent(typeof(RectTransform))]
    public class SimpleGrid : MonoBehaviour
    {
        public bool dynamicSize;
        public float cellScale = 1f;
        [MinValue(1)]
        public int columnCount;
        [MinValue(1)]
        public int rowCount;
        public float offsetByCellX;
        public float offsetByCellY;
        public float offsetTop, offsetlLeft;
        public bool multiPaged;
        [ShowIf("multiPaged"), InlineProperty]
        public Vector2 offsetByPage;

        public int ItemsPerPage => columnCount * rowCount;
        private bool wasUpdated, prevGOState;
        private RectTransform localRect;
        private void Update()
        {
            wasUpdated = false;
            localRect = GetComponent<RectTransform>();
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UpdateView();
#endif
        }
        public void UpdateView()
        {
            if (wasUpdated || !gameObject.activeSelf)
                return;

            Vector2 startPoint = Vector2.zero;
            startPoint.x += offsetlLeft * cellScale;
            startPoint.y -= offsetTop * cellScale;

            Vector2 newLocalSize = new Vector2();
            RectTransform rt = null;

            int curPage = 0;
            int curColumn = 0;
            int curRow = 0;
            int cellsActive = 0;
            for (int i = 0; i < transform.childCount; i++, curColumn++)
            {
                rt = transform.GetChild(i).GetComponent<RectTransform>();
                if (!rt.gameObject.activeInHierarchy)
                {
                    curColumn--;
                    continue;
                }
                Vector2 newPos = new Vector2();
                if (curColumn == columnCount)
                {
                    curColumn = 0;
                    curRow++;
                }

                if (multiPaged)
                {
                    bool nextPage = cellsActive == ItemsPerPage * (curPage + 1);
                    if (nextPage)
                    {
                        curPage++;
                        curRow = 0;
                        curColumn = 0;
                    }
                    newPos += offsetByPage * curPage;
                }
                newPos.x += startPoint.x + offsetByCellX * cellScale * curColumn;
                newPos.y += startPoint.y - offsetByCellY * cellScale * curRow;

                rt.anchorMax = new Vector2(0, 1f);
                rt.anchorMin = new Vector2(0, 1f);
                rt.anchoredPosition = newPos;
                rt.localScale = Vector3.one * cellScale;

                if (dynamicSize)
                {
                    newLocalSize.y += (cellsActive % rowCount == 1 ? rt.sizeDelta.y : 0) * cellScale;
                }
                cellsActive++;
            }
            if (dynamicSize)
                localRect.sizeDelta = newLocalSize;
            wasUpdated = true;
        }
    }
}
