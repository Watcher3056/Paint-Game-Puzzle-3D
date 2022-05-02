using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace TeamAlpha.Source
{
    public class PageIndicator : MonoBehaviour
    {
        [Required]
        public Transform holder;
        [Required, AssetsOnly]
        public Image itemPrefab;
        public Color colorDefault = Color.gray;
        public Color colorHighlighted = Color.black;
        [NonSerialized]
        public float animTime = 1f;


        public bool IsAnimating { private set; get; }
        public int CurIndex
        {
            get => curIndex;
            set
            {
                curIndex = value;
                UpdateView();
            }
        }
        private int curIndex;
        public int PagesCount
        {
            get => pageCount;
            set
            {
                pageCount = value;
                UpdateView(true);
            }
        }
        private int pageCount;
        [NonSerialized]
        private List<Image> items = new List<Image>();

        private void UpdateView(bool rebuild = false)
        {
            if (rebuild)
            {
                holder.DestroyAllChilds();
                items.Clear();

                for (int i = 0; i < pageCount; i++)
                {
                    Image item = Instantiate(itemPrefab.gameObject, holder).GetComponent<Image>();
                    items.Add(item);
                }
            }
            if (CurIndex >= PagesCount)
                CurIndex = 0;

            IsAnimating = true;
            foreach (Image item in items)
            {
                item.DOKill();
                if (items.IndexOf(item) == curIndex)
                    continue;
                item.DOColor(colorDefault, animTime);
            }
            items[curIndex].DOColor(colorHighlighted, animTime).onComplete = () => IsAnimating = false;
        }
    }
}
