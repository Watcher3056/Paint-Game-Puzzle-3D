using Pixeye.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TeamAlpha.Source
{
    public class DataGameMain : Pluggable
    {
        #region SAVE LOAD KEYS
        public const string SAVE_KEY_DIAMONDS_AMOUNT = "DIAMONDS_AMOUNT";
        public const string SAVE_KEY_VIBRATIONS_IS_ENABLED = "VIBRATIONS_IS_ENABLED";
        public const string SAVE_KEY_NEXT_GIFT_RECHARGE = "NEXT_GIFT_RECHARGE";
        public const string SAVE_KEY_HINTS_LEFT = "HINTS_LEFT";
        public const string SAVE_KEY_TIME_BOOST_LEFT = "TIME_BOOST_LEFT";

        #endregion
        #region RUNTIME DATA
        public static DataGameMain Default => _default;
        private static DataGameMain _default;

        public int Diamonds
        {
            get => diamonds;
            set
            {
                diamonds = value;
                ProcessorSaveLoad.Save(SAVE_KEY_DIAMONDS_AMOUNT, diamonds);
            }
        }
        private int diamonds;
        public int HintsLeft
        {
            get => hintsLeft;
            set
            {
                hintsLeft = value;
                ProcessorSaveLoad.Save(SAVE_KEY_HINTS_LEFT, hintsLeft);
            }
        }
        private int hintsLeft;
        public int TimeBoostLeft
        {
            get => timeBoostLeft;
            set
            {
                timeBoostLeft = value;
                ProcessorSaveLoad.Save(SAVE_KEY_TIME_BOOST_LEFT, timeBoostLeft);
            }
        }
        private int timeBoostLeft;
        public bool VibrationsIsEnabled
        {
            get => vibrationsIsEnabled;
            set
            {
                vibrationsIsEnabled = value;
                ProcessorSaveLoad.Save(SAVE_KEY_VIBRATIONS_IS_ENABLED, vibrationsIsEnabled);
            }
        }
        private bool vibrationsIsEnabled;
        public bool SubscriptionActivated
        {
            get => ProcessorIAPManager.Default.IsSubscribed;
        }
        private DateTime dtSubscriptionEnd;

        public List<GameTheme> ThemesAll
        {
            get
            {
                if (themesAll == null || Application.isEditor)
                {
                    themesAll = new List<GameTheme>();
                    themesAll.AddRange(themes);
                    themesAll.AddRange(themesDefault);
                }

                return themesAll;
            }
        }
        [NonSerialized]
        private List<GameTheme> themesAll;
        public GameTheme GameThemeFirstLocked => themes.Find(theme => !theme.Unlocked);
        public string URLPrivacyPolicy => "https://pixel-paint-puzzle.flycricket.io/privacy.html";
        public string URLTermsOfUse => "https://pixel-paint-puzzle.flycricket.io/terms.html";
        #endregion
        #region SETTINGS
        [Required]
        public List<GameTheme> themesDefault;
        [Required]
        public List<GameTheme> themes;
        [Required, AssetsOnly]
        public SpriteRenderer prefabScanPlate;

        public AudioSimpleData audioOnPanelShow;
        public AudioSimpleData audioOnPanelHide;
        public AudioSimpleData audioOnButtonClick;
        public AudioSimpleData audioOnMove;
        public AudioSimpleData audioOnScan;
        public AudioSimpleData audioBGMMain;

        [Required]
        public Material matBorder;
        #endregion


        public override void Plug()
        {
            _default = this;
            ProcessorSaveLoad.OnLocalDataUpdated += HandleLocalDataUpdated;
            ProcessorIAPManager.onInitialized += HandleIAPInitialized;

            HandleLocalDataUpdated();

            foreach (GameTheme theme in ThemesAll)
                theme.Init();
        }
        private void HandleLocalDataUpdated()
        {
            diamonds = ProcessorSaveLoad.Load(SAVE_KEY_DIAMONDS_AMOUNT, 0);
            hintsLeft = ProcessorSaveLoad.Load(SAVE_KEY_HINTS_LEFT, 0);
            vibrationsIsEnabled = ProcessorSaveLoad.Load(SAVE_KEY_VIBRATIONS_IS_ENABLED, true);
        }
        private void HandleIAPInitialized()
        {
            if (SubscriptionActivated && Banner.Default.bannerView != null)
                Banner.Default.bannerView.Hide();

            bool firstLaunch = true;
            DateTime dtNextGift = DateTime.MinValue;
            if (ProcessorSaveLoad.Exists(SAVE_KEY_NEXT_GIFT_RECHARGE))
            {
                dtNextGift = ProcessorSaveLoad.Load<DateTime>(SAVE_KEY_NEXT_GIFT_RECHARGE);
                firstLaunch = false;
            }
            if (dtNextGift < DateTime.Now)
            {
                DateTime dtLastGiftReceived = dtNextGift;

                if (firstLaunch)
                {
                    HintsLeft++;
                    TimeBoostLeft++;
                }
                else
                    while (dtLastGiftReceived.AddHours(24d) < DateTime.Now)
                    {
                        if (dtLastGiftReceived < ProcessorIAPManager.Default.SubscriptionDateExpired)
                        {
                            HintsLeft += 5;
                            TimeBoostLeft += 5;
                        }
                        else
                        {
                            HintsLeft++;
                            TimeBoostLeft++;
                        }
                        dtLastGiftReceived = dtLastGiftReceived.AddHours(24d);
                    }
                ProcessorSaveLoad.Save(SAVE_KEY_NEXT_GIFT_RECHARGE, DateTime.Now.AddHours(24d));
            }

            if (!DataGameMain.Default.SubscriptionActivated)
            {
                PanelSubscriptionOfferFullscreen.Default.panel.OpenPanel();
            }
        }
    }
}
