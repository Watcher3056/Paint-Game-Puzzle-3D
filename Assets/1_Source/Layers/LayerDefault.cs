using Animancer;
using Pixeye.Actors;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameAnalyticsSDK;
using Firebase.Analytics;
using Firebase;
using GoogleMobileAds.Api;
using Facebook.Unity;
using AdaptySDK;

namespace TeamAlpha.Source
{

    public partial class LayerDefault : Layer<LayerDefault>
    {


        [Serializable]
        public class Level
        {
            [Required, AssetsOnly]
            public LevelController prefab;
            public int reward;
            public bool showSubscriptionOffer;

            public bool Unlocked => CheckLevelUnlocked();
            public virtual int IndexLocal => LayerDefault.Default.levelsDefault.IndexOf(this);
            public int IndexGlobal => LayerDefault.Default.AllLevels.IndexOf(this);
#if UNITY_EDITOR
            [HideInInspector]
            public bool _invalid;

            [Sirenix.OdinInspector.Button, ShowIf("_invalid")]
            [InfoBox("Found duplicates. Press [Remove Other Duplticates] to fix", InfoMessageType.Error, "_valid")]
            private void RemoveOtherDuplicates()
            {
                LayerDefault layer = GameObject.FindObjectOfType<LayerDefault>();
                layer.levelsDefault.RemoveAll(l => l.prefab.guid.id.Equals(this.prefab.guid.id) && !ReferenceEquals(l, this));
                layer.levelsEpic.RemoveAll(l => l.prefab.guid.id.Equals(this.prefab.guid.id) && !ReferenceEquals(l, this));
                layer.levelsLegendary.RemoveAll(l => l.prefab.guid.id.Equals(this.prefab.guid.id) && !ReferenceEquals(l, this));
                layer._EditorOnLevelsChanged();
            }
            private void _EditorOnSkyboxChanged()
            {
                LayerDefault layer = FindObjectOfType<LayerDefault>();
                layer._EditorUpdateCurLevelIndex();
            }
            [Sirenix.OdinInspector.Button("Select")]
            private void _EditorSelectLevel()
            {
                LayerDefault layer = FindObjectOfType<LayerDefault>();
                layer._EditorCurLevelIndex = layer.AllLevels.IndexOf(this);
                layer._EditorUpdateCurLevelIndex();
            }
#endif
            protected virtual bool CheckLevelUnlocked()
            {
                int indexLastLevelCompleted =
                    LayerDefault.Default.levelsDefault.FindLastIndex(l => l.prefab.LevelCompleted);
                int index = LayerDefault.Default.levelsDefault.IndexOf(this);
                if (index == 0)
                    return true;
                else if (indexLastLevelCompleted >= 0)
                    return index <= indexLastLevelCompleted || index - 1 == indexLastLevelCompleted;
                return false;
            }
        }
        [Serializable]
        public class LevelEpic : Level
        {
            public int requiredDiamonds;
            public override int IndexLocal => LayerDefault.Default.levelsEpic.IndexOf(this);

            protected override bool CheckLevelUnlocked()
            {
                return DataGameMain.Default.Diamonds >= requiredDiamonds;
            }
        }
        [Serializable]
        public class LevelLegendary : Level
        {
            public int promoteAfterLevelsNumber;
            public override int IndexLocal => LayerDefault.Default.levelsLegendary.IndexOf(this);

            protected override bool CheckLevelUnlocked()
            {
                return prefab.LevelPlayed;
            }
        }

        private static string SaveKeyLastLevelIndex = "LastLevelIndex";

        public static LayerDefault Default => _default;
        private static LayerDefault _default;

        [LabelText("Choose level from editor?"), SerializeField]
        private bool _EditorChooseLevel;
        [ShowIf("_EditorChooseLevel"), SerializeField, OnValueChanged("_EditorUpdateCurLevelIndex"),
            InlineButton("_EditorCurLevelIncrease", ">>"),
            InlineButton("_EditorCurLevelDecrease", "<<")]
        public int _EditorCurLevelIndex;

        [OnValueChanged("_EditorOnLevelsChanged")]
        public List<Level> levelsDefault;
        [OnValueChanged("_EditorOnLevelsChanged")]
        public List<LevelEpic> levelsEpic;
        [OnValueChanged("_EditorOnLevelsChanged")]
        public List<LevelLegendary> levelsLegendary;
        public List<Level> AllLevels
        {
            get
            {
                if (allLevels == null || !Application.isPlaying)
                {
                    allLevels = new List<Level>();
                    allLevels.AddRange(levelsDefault.ToArray());
                    allLevels.AddRange(levelsEpic.ToArray());
                    allLevels.AddRange(levelsLegendary.ToArray());
                }
                return allLevels;
            }
        }
        [NonSerialized]
        private List<Level> allLevels;

        [Required]
        public Transform levelHolder;
        [Required]
        public Transform pointModelPresentation;
        [Required]
        public Transform pointModelPlayable;
        [Required]
        public Transform pointCompareModelPresentation;
        [Required]
        public Transform pointCompareModelPlayable;
        [Required]
        public List<ParticleSystem> particlesOnWin;
        [Required]
        public MeshRenderer rendererBG;
        public AudioSimpleData audioOnWin;

        public LevelLegendary LevelLegendFirstLocked => levelsLegendary.Find(l => !l.Unlocked);
        public LevelEpic LevelEpicLastUnlocked => levelsEpic.FindLast(l => l.Unlocked);
        public Level CurLevel => AllLevels[curLevelIndex];
        public Level PrevLevel => AllLevels[prevLevelIndex];
        public event Action OnAnimationGlobalSpeedChanged = () => { };
        public GameTheme CurTheme { get; private set; }

        public static float TimeScale { get; set; }
        public static float DeltaTime => UnityEngine.Time.deltaTime * TimeScale;
        public static float DeltaTimeFixed => UnityEngine.Time.fixedDeltaTime * TimeScale;
        public bool Playing
        {
            get => playing && !Paused &&
                PanelLegendaryLevelPopup.Default.panel.CurState != Panel.State.Opened &&
                PanelLevels.Default.panel.CurState != Panel.State.Opened &&
                PanelSettings.Default.panel.CurState != Panel.State.Opened &&
                PanelSubscriptionOfferFullscreen.Default.panel.CurState != Panel.State.Opened &&
                PanelSubscriptionOfferPopup.Default.panel.CurState != Panel.State.Opened &&
                PanelLoadingScreen.Default?.panel.CurState != Panel.State.Opened;
            set
            {
                playing = value;
            }
        }
        public bool Paused { get; set; }
        public float AnimSpeedGlobal
        {
            get => animSpeedGlobal;
            set
            {
                foreach (AnimancerComponent animancer in animancers)
                    animancer.Playable.Speed = value;
                animSpeedGlobal = value;
                OnAnimationGlobalSpeedChanged();
            }
        }
        private float animSpeedGlobal;
        private bool playerWon;
        private bool playing;
        private int curLevelIndex;
        private int prevLevelIndex;
        private List<AnimancerComponent> animancers = new List<AnimancerComponent>();
        private bool firstStartPassed;
        private int levelsBeforeLegendLevelPopup;
        private const int LevelsBeforeInterstitial = 1;
        private int levelsBeforeInterstitialLeft = LevelsBeforeInterstitial;

        public LayerDefault() => _default = this;
        protected override void Setup()
        {
            Application.quitting += HandleApplicationQuitting;

            FB.Init();
            GameAnalytics.Initialize();
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
            });
            MobileAds.Initialize(initStatus =>
            {
                if (!DataGameMain.Default.SubscriptionActivated)
                    Interstitial.Default.RequestInterstitial();
                RewardedVideo.Default.RequestRewardedVideo();
            });

            Time.scale = 1f;
            TimeScale = 1f;
            //Add<ProcessorDebug>();
            //Add<ProcessorDeferredOperation>();
            //Add<ProcessorSaveLoad>();
            //Add<ProcessorTweens>();
            //Add<ProcessorSoundPool>();

            UIManager.Default.CurState = UIManager.State.MainMenu;

            curLevelIndex = ProcessorSaveLoad.Load(SaveKeyLastLevelIndex, 0);

            foreach (Level level in AllLevels)
                level.prefab.Init();

            Restart();
        }
        private string GetLevelEventPrefix(Level level)
        {
            string levelPrefix = "Level_";
            if (LayerDefault.Default.CurLevel is LevelLegendary)
            {
                levelPrefix += "Legendary_";
            }
            else if (LayerDefault.Default.CurLevel is LevelEpic)
            {
                levelPrefix += "Epic_";
            }
            else
                levelPrefix += "Default_";
            return levelPrefix + (level.IndexLocal + 1);
        }
        private void HandleApplicationQuitting()
        {
            if (!CurLevel.prefab.LevelCompleted)
                GameAnalytics.NewDesignEvent(GetLevelEventPrefix(CurLevel) + " QUIT");
        }
        public void HandleCurrentLevelPassed(bool firstly)
        {
            if (firstly)
                GameAnalytics.NewDesignEvent(GetLevelEventPrefix(CurLevel) + " COMPLETED");
            if (levelsBeforeInterstitialLeft > 0)
            {
                levelsBeforeInterstitialLeft--;
            }
        }
        public Level GetLastLevelToPlay()
        {
            int index = 0;

            index = AllLevels.IndexOf(levelsLegendary.FindLast(l => l.Unlocked && !l.prefab.LevelCompleted));
            if (index < 0)
                index = AllLevels.IndexOf(levelsEpic.FindLast(l => l.Unlocked && !l.prefab.LevelCompleted));
            if (index < 0)
                index = AllLevels.IndexOf(levelsDefault.FindLast(l => l.Unlocked && !l.prefab.LevelCompleted));
            if (index < 0 && curLevelIndex == AllLevels.Count)
                index = 0;

            if (index < 0)
            {
                index = curLevelIndex + 1;
                if (index >= AllLevels.FindAll(l => l.Unlocked).Count)
                    index = 0;
            }

            return AllLevels[index];
        }
        public void Restart()
        {
            prevLevelIndex = curLevelIndex;
            if (LevelController.Current != null && LevelController.Current.LevelCompleted)
            {
                curLevelIndex = AllLevels.IndexOf(GetLastLevelToPlay());
                //Analytics.Events.LevelStarted(curLevelIndex + 1);
            }
#if UNITY_EDITOR
            if (!firstStartPassed)
            {
                if (_EditorChooseLevel)
                    curLevelIndex = _EditorCurLevelIndex;
            }
#endif
            //else
            //    Analytics.Events.LevelRestart(curLevelIndex + 1);
            ProcessorSaveLoad.Save(SaveKeyLastLevelIndex, curLevelIndex);
            UpdateLevel();
            AnimSpeedGlobal = 1f;
        }
        public void LaunchLevel(Level level)
        {
            prevLevelIndex = curLevelIndex;
            curLevelIndex = AllLevels.IndexOf(level);
            UpdateLevel();
        }
        public void SetGameTheme(GameTheme theme)
        {
            CurTheme = theme;
            rendererBG.material = theme.materialBG;
        }
        public void ResetLegendPopupCounter()
        {
            levelsBeforeLegendLevelPopup = LevelLegendFirstLocked.promoteAfterLevelsNumber;
        }
        private void UpdateLevel()
        {
            EnableSelectedLevel(curLevelIndex);

            animancers = new List<AnimancerComponent>(FindObjectsOfType<AnimancerComponent>());
        }
        private void EnableSelectedLevel(int levelIndex)
        {
            if (!firstStartPassed && LevelLegendFirstLocked != null)
            {
                ResetLegendPopupCounter();
            }
            if (levelsBeforeLegendLevelPopup == 0 && LevelLegendFirstLocked != null)
            {
                levelIndex = AllLevels.IndexOf(LevelLegendFirstLocked);
            }
            levelsBeforeLegendLevelPopup--;

            if (levelsBeforeInterstitialLeft == 0 && !DataGameMain.Default.SubscriptionActivated)
            {
                Interstitial.Default.ShowInterstitial((success) =>
                {
                    if (success)
                        levelsBeforeInterstitialLeft = LevelsBeforeInterstitial;
                });
            }

            curLevelIndex = levelIndex;
            while (levelHolder.childCount != 0)
            {
                GameObject go = levelHolder.GetChild(0).gameObject;
                go.transform.SetParent(null);
                DestroyImmediate(go);
            }
            Level curLevel = AllLevels[levelIndex];
#if UNITY_EDITOR
            if (Application.isPlaying)
                Instantiate(curLevel.prefab, levelHolder);
            else
                UnityEditor.PrefabUtility.InstantiatePrefab(curLevel.prefab, levelHolder);
#elif true
                Instantiate(curLevel.prefab, levelHolder);
#endif
            if (Application.isPlaying)
            {
                if (!firstStartPassed)
                {
                    firstStartPassed = true;
                    ProcessorDeferredOperation.Default.Add(() =>
                    {
                        levelHolder.GetComponentInChildren<LevelController>().SetAsCurrent();
                        LevelController.Current.Launch();
                    }, true, 1);
                }
                else
                {
                    levelHolder.GetComponentInChildren<LevelController>().SetAsCurrent();

                    if (CurLevel is LevelLegendary && !CurLevel.Unlocked)
                    {
                        PanelTop.Default.panel.ClosePanel();
                        UIManager.Default.CurState = UIManager.State.None;
                        PanelLegendaryLevelPopup.Default.panel.OpenPanel();
                        return;
                    }

                    bool showSubscriptionOffer =
                        !DataGameMain.Default.SubscriptionActivated &&
                        CurLevel.showSubscriptionOffer &&
                        CurLevel != PrevLevel;
                    if (showSubscriptionOffer)
                    {
                        void OnPanelClose()
                        {
                            PanelTop.Default.panel.OpenPanel();
                            LevelController.Current.Launch();
                        }
                        PanelTop.Default.panel.ClosePanel();
                        UIManager.Default.CurState = UIManager.State.None;
                        PanelSubscriptionOfferPopup.Default.panel.OnPanelHide -= OnPanelClose;
                        PanelSubscriptionOfferPopup.Default.panel.OnPanelHide += OnPanelClose;

                        ProcessorDeferredOperation.Default.Add(() =>
                        {
                            PanelSubscriptionOfferPopup.Default.panel.OpenPanel();
                        }, true, false, 1f);
                    }
                    else
                        LevelController.Current.Launch();
                }
            }
        }
    }
}
