using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static TeamAlpha.Source.LayerDefault;

namespace TeamAlpha.Source
{
    public class PanelLevels : MonoBehaviour
    {
        public static PanelLevels Default { get; private set; }

        [Required]
        public Panel panel;
        [Required]
        public Button buttonClose;
        [Required]
        public Button buttonGetDiamonds;
        [Required]
        public TextMeshProUGUI textDiamondsRewardAmount;
        [Required]
        public Image imageDiamond;
        [Required]
        public Button buttonUnlockLegendLevel;
        [Required]
        public Button buttonUnlockGameTheme;
        [Required]
        public PanelLevelsTab tabLevelsDefault;
        [Required]
        public PanelLevelsTab tabLevelsEpic;
        [Required]
        public PanelLevelsTab tabLevelsLegendary;
        [Required, AssetsOnly]
        public PanelLevelsCell prefabCell;
        [Required]
        public SimpleGrid holder;
        [Required]
        public PageIndicator pageIndicator;
        [Required]
        public Sprite spriteLegendaryBG;
        [Required]
        public Sprite spriteLegendLevelUnknown;
        public float animTime = 1f;
        public int diamondsAsReward;
        public Color colorTabTextSelected;
        public Color colorTabTextDefault;

        public PanelLevels() => Default = this;

        [NonSerialized]
        public List<PanelLevelsTab> tabs = new List<PanelLevelsTab>();
        [NonSerialized]
        public PanelLevelsTab curTab;
        public PanelLevelsCell CellFirstLocked => curTab.cells.Find(c => !c.linkedLevel.Unlocked);

        public static int ItemsPerPage => 3 * 4;
        private void Awake()
        {
            pageIndicator.animTime = animTime;
            panel.OnPanelShow += HandlePanelShow;
            panel.OnPanelHide += HandlePanelHide;
            buttonClose.onClick.AddListener(HandleButtonCloseClick);
            buttonGetDiamonds.onClick.AddListener(HandleButtonGetDiamondsClick);
            buttonUnlockLegendLevel.onClick.AddListener(HandleButtonUnlockLegendLevelClick);
            buttonUnlockGameTheme.onClick.AddListener(HandleButtonUnlockGameThemeClick);

            holder.transform.DestroyAllChilds();

            tabs.Add(tabLevelsDefault);
            tabs.Add(tabLevelsEpic);
            tabs.Add(tabLevelsLegendary);

            foreach (PanelLevelsTab tab in tabs)
            {
                PanelLevelsTab _tab = tab;
                tab.button.onClick.AddListener(() => HandleButtonTabClick(_tab));
            }

            foreach (Level level in LayerDefault.Default.levelsDefault)
                tabLevelsDefault.AddLevel(level);
            for (int i = 0; i < tabLevelsDefault.fakeLevelsAmount; i++)
                tabLevelsDefault.AddLevel(null, true);

            foreach (Level level in LayerDefault.Default.levelsEpic)
                tabLevelsEpic.AddLevel(level);

            int diamondsRequiredOnLast = LayerDefault.Default.levelsEpic.Last().requiredDiamonds;
            for (int i = 0; i < tabLevelsEpic.fakeLevelsAmount; i++)
            {
                PanelLevelsCell cell = tabLevelsEpic.AddLevel(null, true);
                cell.sliderDiamondsProgress.value = 0f;
                cell.textDiamondsProgress.text = (diamondsRequiredOnLast + (i + 1) * 1000).ToString();
            }

            foreach (Level level in LayerDefault.Default.levelsLegendary)
                tabLevelsLegendary.AddLevel(level);
            for (int i = 0; i < tabLevelsLegendary.fakeLevelsAmount; i++)
            {

                tabLevelsLegendary.AddLevel(null, true, true);
            }

            TabGameThemes.Default.Init();

            SelectTab(tabLevelsDefault);
        }
        private void LateUpdate()
        {
            if (!pageIndicator.IsAnimating)
            {
                int selectIndex = pageIndicator.CurIndex;
                if (CameraManager.CurSwipeNormalized.x >= 0.2f)
                    selectIndex++;
                else if (CameraManager.CurSwipeNormalized.x <= -0.2f)
                    selectIndex--;

                selectIndex = Mathf.Clamp(selectIndex, 0, pageIndicator.PagesCount - 1);

                if (selectIndex != pageIndicator.CurIndex)
                    SelectPage(selectIndex);
            }
        }
        private void HandleButtonCloseClick()
        {
            DataGameMain.Default.audioOnButtonClick.Play(ProcessorSoundPool.PoolLevel.Global);
            panel.ClosePanel();
        }
        private void HandleButtonGetDiamondsClick()
        {
            DataGameMain.Default.audioOnButtonClick.Play(ProcessorSoundPool.PoolLevel.Global);
            RewardedVideo.Default.ShowRewardedVideo(success =>
            {
                if (success)
                {
                    PanelTop.Default.AddDiamonds(diamondsAsReward, imageDiamond.transform.position, UpdateView);
                }
            });
        }
        private void HandleButtonUnlockLegendLevelClick()
        {
            DataGameMain.Default.audioOnButtonClick.Play(ProcessorSoundPool.PoolLevel.Global);
            if (LayerDefault.Default.LevelLegendFirstLocked != null)
            {
                RewardedVideo.Default.ShowRewardedVideo(success =>
                {
                    if (success)
                    {
                        LayerDefault.Default.LevelLegendFirstLocked.prefab.LevelPlayed = true;
                        UpdateView();
                    }
                });
            }
        }
        private void HandleButtonUnlockGameThemeClick()
        {
            DataGameMain.Default.audioOnButtonClick.Play(ProcessorSoundPool.PoolLevel.Global);
            if (DataGameMain.Default.GameThemeFirstLocked != null)
            {
                RewardedVideo.Default.ShowRewardedVideo(success =>
                {
                    if (success)
                    {
                        DataGameMain.Default.GameThemeFirstLocked.Unlocked = true;
                        TabGameThemes.Default.UpdateView();
                        UpdateView();
                    }
                });
            }
        }
        private void HandlePanelShow()
        {
            UpdateView();
        }
        private void HandlePanelHide()
        {

        }
        public void UpdateView()
        {
            if (curTab != null)
            {
                foreach (PanelLevelsCell cell in curTab.cells)
                {
                    for (int i = 0; i < cell.stars.Count; i++)
                    {
                        GameObject star = cell.stars[i];
                        star.SetActive(i < cell.linkedLevel.prefab.StarsAchieved);
                    }
                    cell.sliderDiamondsProgress.gameObject.SetActive(false);
                    cell.imageLevelView.sprite = cell.linkedLevel.prefab.modelPrefab.spritePreview;
                    if (cell.linkedLevel.Unlocked)
                    {
                        cell.imageLevelView.gameObject.SetActive(true);
                        cell.viewStars.SetActive(true);
                        cell.imageUnknown.gameObject.SetActive(false);
                    }
                    else
                    {
                        cell.imageLevelView.gameObject.SetActive(false);
                        cell.viewStars.SetActive(false);
                        cell.imageUnknown.gameObject.SetActive(true);
                    }
                }

                foreach (PanelLevelsCell cell in curTab.fakeCells)
                {
                    cell.imageLevelView.gameObject.SetActive(false);
                    cell.viewStars.SetActive(false);
                    cell.imageUnknown.gameObject.SetActive(true);
                    cell.sliderDiamondsProgress.gameObject.SetActive(curTab == tabLevelsEpic);
                }

                PanelLevelsCell cellFirstLocked =
                    curTab.cells.Find(c => !c.linkedLevel.Unlocked);

                if (curTab == tabLevelsEpic)
                {
                    foreach (PanelLevelsCell cell in curTab.cells)
                    {
                        if (cell.linkedLevel.Unlocked)
                        {
                            cell.viewStars.SetActive(true);
                            cell.sliderDiamondsProgress.gameObject.SetActive(false);
                        }
                        else
                        {
                            cell.viewStars.SetActive(false);
                            cell.sliderDiamondsProgress.gameObject.SetActive(true);
                        }
                        //cellFirstLocked.imageUnknown.gameObject.SetActive(false);

                        if (cell == cellFirstLocked)
                            cell.sliderDiamondsProgress.value =
                                (float)DataGameMain.Default.Diamonds /
                                (float)(cell.linkedLevel as LevelEpic).requiredDiamonds;
                        else
                            cell.sliderDiamondsProgress.value = 0f;
                        cell.textDiamondsProgress.text = (cell.linkedLevel as LevelEpic).requiredDiamonds.ToString();
                    }
                }
                else if (curTab == tabLevelsLegendary)
                {

                }
            }

            if (curTab == tabLevelsDefault || curTab == tabLevelsEpic)
            {
                buttonGetDiamonds.gameObject.SetActive(true);
                textDiamondsRewardAmount.text = '+' + diamondsAsReward.ToString();
                buttonUnlockLegendLevel.gameObject.SetActive(false);
                buttonUnlockGameTheme.gameObject.SetActive(false);
            }
            else if (curTab == tabLevelsLegendary)
            {
                buttonGetDiamonds.gameObject.SetActive(false);
                buttonUnlockLegendLevel.gameObject.SetActive(true);
                buttonUnlockGameTheme.gameObject.SetActive(false);
                buttonUnlockLegendLevel.interactable = LayerDefault.Default.LevelLegendFirstLocked != null;
            }
            else
            {
                buttonGetDiamonds.gameObject.SetActive(false);
                buttonUnlockLegendLevel.gameObject.SetActive(false);
                buttonUnlockGameTheme.gameObject.SetActive(true);
                buttonUnlockGameTheme.interactable = DataGameMain.Default.GameThemeFirstLocked != null;
            }

            holder.UpdateView();
        }
        private void HandleButtonTabClick(PanelLevelsTab tabClicked)
        {
            DataGameMain.Default.audioOnButtonClick.Play(ProcessorSoundPool.PoolLevel.Global);
            SelectTab(tabClicked);
        }
        private void SelectTab(PanelLevelsTab tabSelected)
        {
            if (curTab != null)
            {
                foreach (PanelLevelsCell cell in curTab.cells)
                    cell.gameObject.SetActive(false);
                foreach (PanelLevelsCell cell in curTab.fakeCells)
                    cell.gameObject.SetActive(false);
            }
            foreach (TabGameThemesCell cell in TabGameThemes.Default.cells)
                cell.gameObject.SetActive(false);
            TabGameThemes.Default.cellDefaultTheme.gameObject.SetActive(false);

            curTab = tabSelected;
            curTab.transform.SetAsLastSibling();

            foreach (PanelLevelsCell cell in tabSelected.cells)
                cell.gameObject.SetActive(true);
            foreach (PanelLevelsCell cell in curTab.fakeCells)
                cell.gameObject.SetActive(true);

            pageIndicator.PagesCount = tabSelected.PagesCount;
            SelectPage(0);

            Color colorDark = Color.white;

            List<Image> _tabsBG = new List<Image>();
            _tabsBG.Add(TabGameThemes.Default.imageBG);
            foreach (PanelLevelsTab tab in tabs)
                _tabsBG.Add(tab.imageBG);
            _tabsBG.Sort((t1, t2) => t1.transform.position.x.CompareTo(t2.transform.position.x));

            foreach (Image tab in _tabsBG)
            {
                tab.DOKill();
                if (tab == curTab.imageBG)
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

            UpdateView();
        }
        public void SelectPage(int index)
        {
            pageIndicator.CurIndex = index;
            Vector3 holderPosition = holder.transform.localPosition;
            holderPosition.x = -pageIndicator.CurIndex * holder.offsetByPage.x;
            holder.transform.DOKill();
            holder.transform.DOLocalMoveX(holderPosition.x, animTime);
        }
    }
}
