using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Sirenix.Serialization;
using UnityEngine;


namespace TeamAlpha.Source
{
    public partial class ProcessorSaveLoad : MonoBehaviour
    {
        public static ProcessorSaveLoad Default => _default;
        private static ProcessorSaveLoad _default;

        public static event Action OnLocalDataUpdated = () => { };

        private const string filenameLocal = "saves.txt";
        private const string tagVersion = "SaveFileVersion";
        private const string tagUserID = "UserID";
        //Starts from 0
        private const int lastVersion = 0;

        private bool alreadyInProcess;
        private bool dataWasLoadedFromCloud;

        private void Awake()
        {
            _default = this;
            SetupMigration();

            TryMigrate();
        }
        public static void SavePP<T>(string key, T value)
        {
            ES2.Save(value, key, new ES2Settings { saveLocation = ES2Settings.SaveLocation.PlayerPrefs });
        }
        public static T LoadPP<T>(string key, T defaultValue)
        {
             return ExistsPP(key) ? ES2.Load<T>(key, new ES2Settings { saveLocation = ES2Settings.SaveLocation.PlayerPrefs }) : defaultValue;
        }
        public static bool ExistsPP(string key)
        {
            return ES2.Exists(key, new ES2Settings { saveLocation = ES2Settings.SaveLocation.PlayerPrefs });
        }
        public static void Save<T>(string key, T value)
        {
            ES2.Save<T>(value, filenameLocal + "?tag=" + key);
        }
        public static void SaveAsBytes<T>(string key, T value)
        {
            byte[] data = DataToBytesJSON(value);
            ES2.Save(data, filenameLocal + "?tag=" + key);
        }
        public static T Load<T>(string key)
        {
            return ES2.Load<T>(filenameLocal + "?tag=" + key);
        }
        public static T Load<T>(string key, T defaultValue)
        {
            return Exists(key) ? ES2.Load<T>(filenameLocal + "?tag=" + key) : defaultValue;
        }
        public static T LoadAsBytes<T>(string key, T defaultValue)
        {
            if (Exists(key))
            {
                byte[] data = ES2.Load<byte[]>(filenameLocal + "?tag=" + key);
                return JSONBytesToData<T>(data);
            }
            else
                return defaultValue;
        }
        public static bool Exists(string key)
        {
            return ES2.Exists(filenameLocal + "?tag=" + key);
        }
        public static void CleanSaves()
        {
            ES2.Delete(filenameLocal);
            PlayerPrefs.DeleteAll();
            Default.Log("::: Local saves was deleted!");
        }

        #region Saved Games
        private static byte[] DataToBytesJSON<T>(T data)
        {
            return SerializationUtility.SerializeValue(data, DataFormat.JSON);
        }
        private static byte[] GameDataToBytes(ES2Data gameData)
        {
            return SerializationUtility.SerializeValue(gameData, DataFormat.Binary);
        }
        private static ES2Data BytesToGameData(byte[] gameData)
        {
            return SerializationUtility.DeserializeValue<ES2Data>(gameData, DataFormat.Binary);
        }
        private static T JSONBytesToData<T>(byte[] data)
        {
            return SerializationUtility.DeserializeValue<T>(data, DataFormat.JSON);
        }
        private void SaveDataLocal(ES2Data data)
        {
            this.Log(this + "::: Overwriting local data start...");
            Dictionary<string, object> dic = data.loadedData;
            List<string> keys = dic.Keys.ToList();

            using (ES2Writer writer = ES2Writer.Create(filenameLocal))
            {
                // Write our data to the file.
                for (int i = 0; i < keys.Count; i++)
                {
                    writer.Write(dic[keys[i]], keys[i]);
                    this.Log(this + "::: Overwrite key " + keys[i] + " with value: " + dic[keys[i]]);
                }
                // Remember to save when we're done.
                writer.Save();
            }
            this.Log(this + "::: Overwriting local data end!");
            TryMigrate();
        }
        #endregion /Saved Games

        #region Tests
        public static void TestMigration()
        {
            ProcessorSaveLoad.Save(ProcessorSaveLoad.tagVersion, 0);

            //...
        }
        public static void TestSerialization()
        {
            string filenameTest = "test.txt";
            Guid guid = Guid.NewGuid();

            ES2.Save(guid.ToString(), filenameTest + "?tag=guid");
            if (BytesToGameData(GameDataToBytes(ES2.LoadAll(filenameTest))).Load<string>("guid") == guid.ToString())
                Debug.Log("Test Success!");
            else
                Debug.Log("Test Failed!");
        }
        public static void TestSyncronization()
        {
            ES2Data originalData = ES2.LoadAll(filenameLocal);

            Debug.Log("::: Overwriting local originalData start...");
            Dictionary<string, object> dicOriginal = originalData.loadedData;
            List<string> keys = dicOriginal.Keys.ToList();

            using (ES2Writer writer = ES2Writer.Create(filenameLocal))
            {
                // Write our data to the file.
                for (int i = 0; i < keys.Count; i++)
                {
                    writer.Write(dicOriginal[keys[i]], keys[i]);
                    Debug.Log("::: Overwrite key " + keys[i] + " with value: " + dicOriginal[keys[i]]);
                }
                // Remember to save when we're done.
                writer.Save();
            }
            Debug.Log("::: Overwriting local originalData end!");

            Dictionary<string, object> dicNew = ES2.LoadAll(filenameLocal).loadedData;

            for (int i = 0; i < keys.Count; i++)
            {
                if (dicNew[keys[i]].ToString() != dicOriginal[keys[i]].ToString())
                    Debug.LogError("Key: " + keys[i] + " Original value: " + dicOriginal[keys[i]] + " Writed value: " + dicNew[keys[i]]);
            }
            Debug.Log("Sync test ended!");
        }
        #endregion /Tests
    }
}
