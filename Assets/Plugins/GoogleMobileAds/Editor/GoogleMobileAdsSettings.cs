using System.IO;
using UnityEditor;
using UnityEngine;

namespace GoogleMobileAds.Editor
{

    internal class GoogleMobileAdsSettings : ScriptableObject
    {
        private const string MobileAdsSettingsDir = "Assets/Plugins/GoogleMobileAds";

        private const string MobileAdsSettingsResDir = "Assets/Plugins/GoogleMobileAds/Resources";

        private const string MobileAdsSettingsFile = "GoogleMobileAdsSettings";

        private const string MobileAdsSettingsFileExtension = ".asset";

        private static GoogleMobileAdsSettings instance;

        [SerializeField]
        private string adMobAndroidAppId = "ca-app-pub-4826715583137572~4732247730";

        [SerializeField]
        private string adMobIOSAppId = "ca-app-pub-4826715583137572~4579354437";

        [SerializeField]
        private bool delayAppMeasurementInit = false;

        public string GoogleMobileAdsAndroidAppId
        {
            get { return Instance.adMobAndroidAppId; }

            set { Instance.adMobAndroidAppId = value; }
        }

        public string GoogleMobileAdsIOSAppId
        {
            get { return Instance.adMobIOSAppId; }

            set { Instance.adMobIOSAppId = value; }
        }

        public bool DelayAppMeasurementInit
        {
            get { return Instance.delayAppMeasurementInit; }

            set { Instance.delayAppMeasurementInit = value; }
        }

        public static GoogleMobileAdsSettings Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                instance = Resources.Load<GoogleMobileAdsSettings>(MobileAdsSettingsFile);
                if (instance != null)
                {
                    return instance;
                }

                Directory.CreateDirectory(MobileAdsSettingsResDir);

                instance = ScriptableObject.CreateInstance<GoogleMobileAdsSettings>();

                string assetPath = Path.Combine(MobileAdsSettingsResDir, MobileAdsSettingsFile);
                string assetPathWithExtension = Path.ChangeExtension(
                                                        assetPath, MobileAdsSettingsFileExtension);
                AssetDatabase.CreateAsset(instance, assetPathWithExtension);

                return instance;
            }
        }

        internal void WriteSettingsToFile()
        {
            AssetDatabase.SaveAssets();
        }
    }
}
