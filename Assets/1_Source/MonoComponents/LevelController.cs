using Cinemachine;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAnalyticsSDK;
using static TeamAlpha.Source.LayerDefault;

namespace TeamAlpha.Source
{
    public class LevelController : MonoBehaviour
    {
        #region SAVE_KEYS
        public string SAVE_KEY_EQUALITY => guid.id + ".EQUALITY";
        public string SAVE_KEY_PLAYED => guid.id + ".PLAYED";
        #endregion
        public class Statistic
        {
            public int TotalDiamondsReward => LayerDefault.Default.CurLevel.reward * diamondsBonusMultiplier;

            public bool playerWon;
            public int diamondsAchievedByCombo;
            public int diamondsBonusMultiplier = 1;
        }
        public int StarsAchieved
        {
            get
            {
                int starsAchieved = 0;
                if (equality >= 0.5f)
                    starsAchieved++;
                if (equality >= 0.7f)
                    starsAchieved++;
                if (equality >= 0.85f)
                    starsAchieved++;
                return starsAchieved;
            }
        }

        public bool LevelPlayed
        {
            get => levelPlayed;
            set
            {
                levelPlayed = value;
                Origin.levelPlayed = value;
                ProcessorSaveLoad.Save(SAVE_KEY_PLAYED, levelPlayed);
            }
        }
        private bool levelPlayed;
        public bool LevelCompleted => Equality >= 0.5f;
        public float Equality
        {
            get => equality;
            set
            {
                bool levelNotPassed = !Origin.LevelCompleted;
                equality = value;
                Origin.equality = value;
                ProcessorSaveLoad.Save(SAVE_KEY_EQUALITY, equality);

                if (Origin.LevelCompleted)
                {
                    LayerDefault.Default.HandleCurrentLevelPassed(levelNotPassed);
                }
            }
        }
        private float equality;
        public static LevelController Current => _current;
        private static LevelController _current;

        [HideLabel]
        public ComponentGUID guid;
        [Required]
        public Transform dynamicHolder;
        [Required, AssetsOnly]
        public MonoModel modelPrefab;

        public int paintExtraAmount;
        public int PaintLeft { get; set; }
        public TimeSpan TimeSpanLeft => new TimeSpan(0, 0, (int)TimeLeft);
        public float TimeLeft { get; set; }

        public static event Action OnLevelChangedChanged = () => { };
        public Statistic LevelStats => levelStats;
        private Statistic levelStats = new Statistic();
        public static Statistic lastLevelStats;

        [NonSerialized]
        public MonoModel modelPresentation;
        [NonSerialized]
        public MonoModel modelPlayable;
        public LevelController Origin => LayerDefault.Default.AllLevels.Find(l => l.prefab.guid.id == guid.id).prefab;

        public void Init()
        {
            ProcessorSaveLoad.OnLocalDataUpdated += HandleLocalDataUpdated;

            HandleLocalDataUpdated();
        }
        public void SetAsCurrent()
        {
            if (_current != null)
            {

            }
            _current = this;

            TimeLeft = 60f;
            GameObject modelToDestroy = transform.GetComponentInChildren<MonoModel>()?.gameObject;
            if (modelToDestroy != null)
                Destroy(modelToDestroy);
            modelPresentation = Instantiate(modelPrefab, dynamicHolder).GetComponent<MonoModel>();
            modelPresentation.transform.position = LayerDefault.Default.pointModelPresentation.position;
            modelPresentation.transform.localScale = (modelPresentation.scaleInGame * Vector3.one) / 3f;

            modelPlayable = Instantiate(modelPrefab, dynamicHolder).GetComponent<MonoModel>();
            modelPlayable.transform.position = LayerDefault.Default.pointModelPlayable.position;
            modelPlayable.transform.localScale = Vector3.one * modelPlayable.scaleInGame;

            Vector3 cameraPos = CameraManager.Default.transform.position;
            cameraPos.y = modelPlayable.transform.position.y;
            CameraManager.Default.transform.position = cameraPos;

            Vector3 lookAt = CameraManager.Default.transform.position;
            lookAt.y = modelPresentation.transform.position.y;
            modelPresentation.transform.LookAtByY(lookAt);
            modelPlayable.transform.LookAtByY(CameraManager.Default.transform.position);


            PaintLeft = modelPrefab.PartsPlayable.Count * paintExtraAmount;
            modelPlayable.PaintPartsToDefault();

            if (TabGameThemes.Default.CurCellSelected == null)
            {
                int index = DataGameMain.Default.themesDefault.IndexOf(LayerDefault.Default.CurTheme);
                if (index >= 0 || LayerDefault.Default.CurTheme == null)
                {
                    index++;
                    if (index == DataGameMain.Default.themesDefault.Count)
                        index = 0;
                    LayerDefault.Default.SetGameTheme(DataGameMain.Default.themesDefault[index]);
                }
            }

            OnLevelChangedChanged.Invoke();
        }
        public void Launch()
        {
            LevelPlayed = true;
            LayerDefault.Default.Playing = true;
            UIManager.Default.CurState = UIManager.State.Play;
            PlayerController.Default.CurType = modelPlayable.types[0];
        }
        public void Update()
        {
            if (!LayerDefault.Default.Playing)
                return;
            TimeLeft -= UnityEngine.Time.deltaTime;
            if (TimeLeft <= 0f)
            {
                TimeLeft = 0f;
                CompleteLevel();
            }
        }
        public void CheckLevelCompletion()
        {

        }
        public void CompleteLevel()
        {
            StartCoroutine(_CompleteLevel());
        }
        private IEnumerator _CompleteLevel()
        {

            LayerDefault.Default.Playing = false;

            PanelProgress.Default.panel.ClosePanel();

            DataGameMain.Default.audioOnMove.Play(ProcessorSoundPool.PoolLevel.GameLevel);

            float scaleFactorPresentation = 0.6f;
            float scaleCompare = modelPrefab.scaleInGame * scaleFactorPresentation;
            while (modelPrefab.bounds.size.x * scaleCompare / 2f >
                Mathf.Abs(LayerDefault.Default.pointCompareModelPlayable.position.x))
            {
                scaleFactorPresentation -= 0.1f;
                scaleCompare = modelPrefab.scaleInGame * scaleFactorPresentation;
            }

            modelPlayable.transform.DOMove(LayerDefault.Default.pointCompareModelPlayable.position, 1f).onUpdate = () =>
            {
                //modelPlayable.transform.LookAtByY(CameraManager.Default.transform.position);
            };
            modelPlayable.transform.DOScale(scaleCompare, 1f);

            modelPresentation.transform.DOMove(LayerDefault.Default.pointCompareModelPresentation.position, 1f).onUpdate = () =>
            {
                //modelPresentation.transform.LookAtByY(CameraManager.Default.transform.position);
            };
            modelPresentation.transform.DOScale(scaleCompare, 1f);

            equality = (float)modelPlayable.PartsPaintedCorrect / (float)modelPlayable.PartsPlayable.Count;
            levelStats.playerWon = LevelCompleted;
            lastLevelStats = levelStats;

            if (Origin.Equality < equality)
            {
                Origin.Equality = equality;
            }

            UIManager.Default.CurState = UIManager.State.LevelComplete;

            yield return new WaitForSeconds(1f);
            PanelEqualityCheck.Default.panel.OpenPanel();
            DataGameMain.Default.audioOnScan.Play(ProcessorSoundPool.PoolLevel.GameLevel);
            modelPlayable.LaunchScanEffect(5f);
            modelPresentation.LaunchScanEffect(5f);
            yield return new WaitForSeconds(2f);
            PanelStars.Default.panel.OpenPanel();
            yield return new WaitForSeconds(1f * StarsAchieved);

            PanelEqualityCheck.Default.panel.ClosePanel();
            if (LevelCompleted)
            {
                PanelScoresMultiplier.Default.panel.OpenPanel();
                while (PanelScoresMultiplier.Default.panel.CurState == Panel.State.Opened)
                    yield return null;
                PanelStars.Default.panel.ClosePanel();
                if (PanelEpicLevelProgress.Default.LevelTracked != null)
                    PanelEpicLevelProgress.Default.panel.OpenPanel();
                else
                    LayerDefault.Default.Restart();
            }
            else
            {
                UIManager.Default.CurState = UIManager.State.None;
                if (PanelEpicLevelProgress.Default.LevelTracked != null &&
                    PanelEpicLevelProgress.Default.LevelTracked.Unlocked)
                {
                    PanelEpicLevelProgress.Default.panel.OpenPanel();
                }
                else
                    PanelFailed.Default.panel.OpenPanel();
            }
        }
        private void HandleLocalDataUpdated()
        {
            equality = ProcessorSaveLoad.Load(SAVE_KEY_EQUALITY, 0f);
            levelPlayed = ProcessorSaveLoad.Load(SAVE_KEY_PLAYED, false);
        }
    }
}