using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace SimpleLocalization
{
    /// <summary>
    /// Localization manager.
    /// </summary>
    public static class LocalizationManager
    {
        /// <summary>
        /// Fired when localization changed.
        /// </summary>
        public static event Action LocalizationChanged = () => { };
        public static event Action FontChanged = () => { };

        private static readonly Dictionary<string, Dictionary<string, string>> Dictionary = new Dictionary<string, Dictionary<string, string>>();
        private static string _language = "English";
        private static TMP_FontAsset curFont;

        /// <summary>
        /// Get or set language.
        /// </summary>
        public static string Language
        {
            get { return _language; }
            set { _language = value; LocalizationChanged(); }
        }
        public static TMP_FontAsset CurFont
        {
            get { return curFont; }
            set { curFont = value;  FontChanged(); }
        }

		/// <summary>
		/// Set default language.
		/// </summary>
        public static void AutoLanguage()
        {
            Language = "English";
        }

		/// <summary>
		/// Read localization spreadsheets.
		/// </summary>
		public static void Read(string path = "Localization")
        {
            if (Dictionary.Count > 0) return;

            var textAssets = Resources.LoadAll<TextAsset>(path);

            foreach (var textAsset in textAssets)
            {
                var text = ReplaceMarkers(textAsset.text);
                var matches = Regex.Matches(text, "\"[\\s\\S]+?\"");

                foreach (Match match in matches)
                {
					text = text.Replace(match.Value, match.Value.Replace("\"", null).Replace(",", "[comma]").Replace("\n", "[newline]"));
                }

                var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
				var languages = lines[0].Split(',').Select(i => i.Trim()).ToList();

				for (var i = 1; i < languages.Count; i++)
                {
                    if (!Dictionary.ContainsKey(languages[i]))
                    {
                        Dictionary.Add(languages[i], new Dictionary<string, string>());
                    }
                }
				
                for (var i = 1; i < lines.Length; i++)
                {
					var columns = lines[i].Split(',').Select(j => j.Trim()).Select(j => j.Replace("[comma]", ",").Replace("[newline]", "\n")).ToList();
					var key = columns[0];

                    for (var j = 1; j < languages.Count; j++)
                    {
                        Dictionary[languages[j]].Add(key, columns[j]);
                    }
                }
            }

            AutoLanguage();
        }

		/// <summary>
		/// Get localized value by localization key.
		/// </summary>
        public static string Localize(string localizationKey)
        {
            if (Dictionary.Count == 0)
            {
                LocalizationManager.Read();
                if (Application.systemLanguage == SystemLanguage.Russian ||
                    Application.systemLanguage == SystemLanguage.Belarusian ||
                    Application.systemLanguage == SystemLanguage.Ukrainian)
                    LocalizationManager.Language = "Russian";
                else
                    LocalizationManager.Language = "English";
            }
            return GetValue(localizationKey, Language);
        }
        public static string GetValue(string localizationKey, string column)
        {
            if (!Dictionary.ContainsKey(column)) throw new KeyNotFoundException("Language(Column) not found: " + Language);
            if (!Dictionary[column].ContainsKey(localizationKey)) throw new KeyNotFoundException("Translation(value) not found: " + localizationKey);

            return Dictionary[column][localizationKey];
        }
	    /// <summary>
	    /// Get localized value by localization key.
	    /// </summary>
		public static string Localize(string localizationKey, params object[] args)
        {
            var pattern = Localize(localizationKey);

            return string.Format(pattern, args);
        }

        private static string ReplaceMarkers(string text)
        {
            return text.Replace("[Newline]", "\n");
        }
    }
}