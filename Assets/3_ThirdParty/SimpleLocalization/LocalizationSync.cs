using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;

namespace SimpleLocalization
{
    /// <summary>
    /// Downloads spritesheets from Google Spreadsheet and saves them to Resources. My laziness made me to create it.
    /// </summary>
    public class LocalizationSync : SerializedMonoBehaviour
    {
        [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.Foldout)]
        public Dictionary<string, string> dicLangFont;

#if UNITY_EDITOR
        /// <summary>
        /// Table id on Google Spreadsheet.
        /// Let's say your table has the following url https://docs.google.com/spreadsheets/d/1RvKY3VE_y5FPhEECCa5dv4F7REJ7rBtGzQg9Z_B_DE4/edit#gid=331980525
        /// So your table id will be "1RvKY3VE_y5FPhEECCa5dv4F7REJ7rBtGzQg9Z_B_DE4" and sheet id will be "331980525" (gid parameter)
        /// </summary>
        public string TableId;

        /// <summary>
        /// Table sheet contains sheet name and id. First sheet has always zero id. Sheet name is used when saving.
        /// </summary>
        public Sheet[] Sheets;

        /// <summary>
        /// Folder to save spreadsheets. Must be inside Resources folder.
        /// </summary>
        public UnityEngine.Object SaveFolder;

        private const string UrlPattern = "https://docs.google.com/spreadsheets/d/{0}/export?format=csv&gid={1}";
#endif

        /// <summary>
        /// Sync spreadsheets.
        /// </summary>
        public void Awake()
        {
            LocalizationManager.LocalizationChanged += OnLocalizationChangedHandler;
        }
        private void OnLocalizationChangedHandler()
        {
            LocalizationManager.CurFont = Resources.Load<TMP_FontAsset>("Fonts/" + dicLangFont[LocalizationManager.Language]);
        }
#if UNITY_EDITOR
        [Button]
        public void Sync()
        {
            StopAllCoroutines();
            StartCoroutine(SyncCoroutine());
        }
        private IEnumerator SyncCoroutine()
        {
            var folder = UnityEditor.AssetDatabase.GetAssetPath(SaveFolder);

            Debug.Log("<color=yellow>Sync started, please wait for confirmation message...</color>");

#if UNITY_5

			Debug.Log("<color=yellow>You should do Sync in Play mode in Unity 5!</color>");

#endif

            var downloaders = new List<WWW>();

            foreach (var sheet in Sheets)
            {
                var url = string.Format(UrlPattern, TableId, sheet.Id);

                Debug.LogFormat("Downloading: {0}...", url);

                downloaders.Add(new WWW(url));
            }

            foreach (var downloader in downloaders)
            {
                if (!downloader.isDone)
                {
                    yield return downloader;
                }

                if (downloader.error == null)
                {
                    var sheet = Sheets.Single(i => downloader.url == string.Format(UrlPattern, TableId, i.Id));
                    var path = System.IO.Path.Combine(folder, sheet.Name + ".csv");

                    System.IO.File.WriteAllBytes(path, downloader.bytes);
                    UnityEditor.AssetDatabase.Refresh();
                    Debug.LogFormat("Sheet {0} downloaded to {1}", sheet.Id, path);
                }
                else
                {
                    throw new Exception(downloader.error);
                }
            }

            Debug.Log("<color=green>Localization successfully synced!</color>");
        }
#endif
    }
}