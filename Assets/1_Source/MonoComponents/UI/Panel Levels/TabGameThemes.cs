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
    public class TabGameThemes : MonoBehaviour
    {
        public static TabGameThemes Default { get; private set; }

        [Required]
        public Button button;
        [Required]
        public Image imageBG;
        [Required]
        public Sprite spriteDefaultThemes;
        [Required, AssetsOnly]
        public TabGameThemesCell cellPrefab;


        [NonSerialized]
        public List<TabGameThemesCell> cells = new List<TabGameThemesCell>();
        [NonSerialized]
        public TabGameThemesCell cellDefaultTheme;

        public TabGameThemesCell CurCellSelected => cells.Find(c => c.linkedTheme == LayerDefault.Default.CurTheme);

        public int PagesCount
        {
            get
            {
                int result = cells.Count / PanelLevels.ItemsPerPage;
                if (cells.Count % PanelLevels.ItemsPerPage > 0)
                    result++;

                return result;
            }
        }

        public TabGameThemes() => Default = this;
        public void Init()
        {
            foreach (GameTheme theme in DataGameMain.Default.themes)
                AddTheme(theme);
            cellDefaultTheme =
                Instantiate(cellPrefab.gameObject, PanelLevels.Default.holder.transform).GetComponent<TabGameThemesCell>();
            cellDefaultTheme.imagePreview.sprite = spriteDefaultThemes;
            cellDefaultTheme.imagePreview.gameObject.SetActive(true);
            cellDefaultTheme.viewUnknown.gameObject.SetActive(false);

            button.onClick.AddListener(HandleButtonClick);
            PanelLevels.Default.panel.OnPanelShow += UpdateView;
        }
        public void UpdateView()
        {
            foreach (TabGameThemesCell cell in cells)
            {
                if (cell.linkedTheme == LayerDefault.Default.CurTheme)
                    cell.viewSelected.SetActive(true);
                else
                    cell.viewSelected.SetActive(false);

                if (cell.linkedTheme.Unlocked)
                {
                    cell.imagePreview.gameObject.SetActive(true);
                    cell.viewUnknown.SetActive(false);
                }
                else
                {
                    cell.imagePreview.gameObject.SetActive(false);
                    cell.viewUnknown.SetActive(true);
                }
            }
            if (CurCellSelected == null)
                cellDefaultTheme.viewSelected.SetActive(true);
            else
                cellDefaultTheme.viewSelected.SetActive(false);
        }
        public void AddTheme(GameTheme theme)
        {
            TabGameThemesCell cell =
                Instantiate(cellPrefab.gameObject, PanelLevels.Default.holder.transform).GetComponent<TabGameThemesCell>();
            cell.linkedTheme = theme;
            cell.imagePreview.sprite = theme.spritePreview;
            cells.Add(cell);
        }
        private void HandleButtonClick()
        {
            DataGameMain.Default.audioOnButtonClick.Play(ProcessorSoundPool.PoolLevel.Global);

            if (PanelLevels.Default.curTab != null)
            {
                foreach (PanelLevelsCell cell in PanelLevels.Default.curTab.cells)
                    cell.gameObject.SetActive(false);
                foreach (PanelLevelsCell cell in PanelLevels.Default.curTab.fakeCells)
                    cell.gameObject.SetActive(false);
            }
            PanelLevels.Default.curTab = null;
            transform.SetAsLastSibling();

            foreach (TabGameThemesCell cell in cells)
                cell.gameObject.SetActive(true);
            cellDefaultTheme.gameObject.SetActive(true);
            cellDefaultTheme.transform.SetAsFirstSibling();

            PanelLevels.Default.pageIndicator.PagesCount = PagesCount;
            PanelLevels.Default.pageIndicator.CurIndex = 0;

            Color colorDark = Color.white;

            List<Image> _tabsBG = new List<Image>();
            _tabsBG.Add(imageBG);
            foreach (PanelLevelsTab tab in PanelLevels.Default.tabs)
                _tabsBG.Add(tab.imageBG);
            _tabsBG.Sort((t1, t2) => t1.transform.position.x.CompareTo(t2.transform.position.x));
            foreach (Image tab in _tabsBG)
            {
                tab.DOKill();
                if (tab == imageBG)
                {
                    tab.DOColor(Color.white, 0.25f);
                }
                else
                {
                    float reducingPerTab = 1f / _tabsBG.Count;
                    reducingPerTab *= 0.5f;
                    colorDark.r -= reducingPerTab;
                    colorDark.g -= reducingPerTab;
                    colorDark.b -= reducingPerTab;
                    tab.DOColor(colorDark, 0.25f);
                }
            }

            PanelLevels.Default.SelectPage(0);
            UpdateView();
            PanelLevels.Default.UpdateView();
        }
    }
}
