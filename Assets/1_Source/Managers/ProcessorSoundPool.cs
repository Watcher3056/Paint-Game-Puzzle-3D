using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamAlpha.Source;
using UnityEngine;

namespace TeamAlpha.Source
{
    public class ProcessorSoundPool : MonoBehaviour
    {
        public static ProcessorSoundPool Default => _default;
        private static ProcessorSoundPool _default;
        public enum PoolLevel { GameLevel, Global }
        public class SoundPoolItem
        {
            public AudioSource audioSource;
            public PoolLevel poolLevel;
            public float idleTime;
        }

        public string SAVE_KEY_MUSIC_IS_ENABLED => "MUSIC_IS_ENABLED";
        public string SAVE_KEY_SOUND_IS_ENABLED => "SOUND_IS_ENABLED";
        public bool MusicIsEnabled
        {
            get => musicIsEnabled;
            set
            {
                musicIsEnabled = value;
                ProcessorSaveLoad.Save(SAVE_KEY_MUSIC_IS_ENABLED, musicIsEnabled);
                SetVolume(musicIsEnabled ? 1f : 0f);
            }
        }
        private bool musicIsEnabled;
        public bool SoundIsEnabled
        {
            get => soundIsEnabled;
            set
            {
                soundIsEnabled = value;
                ProcessorSaveLoad.Save(SAVE_KEY_SOUND_IS_ENABLED, soundIsEnabled);
                AudioListener.volume = soundIsEnabled ? 1f : 0f;
            }
        }
        private bool soundIsEnabled;

        private int maxIdleTime = 300;
        private int minPoolSize = 5;
        private AudioSource sourceBGMCur;
        private AudioSource sourceBGMPrev;
        private float curVolume;

        private List<SoundPoolItem> soundPool = new List<SoundPoolItem>(20);

        public ProcessorSoundPool()
        {
            _default = this;
        }
        public void Awake()
        {
            ProcessorSaveLoad.OnLocalDataUpdated += HandleLocalDataUpdated;
            HandleLocalDataUpdated();
        }
        private void HandleLocalDataUpdated()
        {
            MusicIsEnabled = ProcessorSaveLoad.Load(SAVE_KEY_MUSIC_IS_ENABLED, false);
            SoundIsEnabled = ProcessorSaveLoad.Load(SAVE_KEY_SOUND_IS_ENABLED, true);
        }
        private void OnDestroy()
        {
            ClearPull();
        }
        public void Update()
        {
            for (int i = 0; i < soundPool.Count; i++)
            {
                if (!soundPool[i].audioSource.isPlaying)
                {
                    soundPool[i].idleTime += UnityEngine.Time.unscaledDeltaTime;
                    if (soundPool[i].idleTime > maxIdleTime && soundPool.Count > minPoolSize)
                    {
                        DestroyPoolItem(soundPool[i]);
                        i--;
                    }
                }
                else
                    soundPool[i].idleTime = 0f;
            }
        }
        public void ClearPull()
        {
            while (soundPool.Count != 0)
                DestroyPoolItem(soundPool[0]);
        }
        public void ClearPull(PoolLevel poolLevel)
        {
            for (int i = 0; i < soundPool.Count; i++)
            {
                if (soundPool[i].poolLevel == poolLevel)
                {
                    DestroyPoolItem(soundPool[i]);
                    i--;
                }
            }
        }
        public static AudioSource PlaySound(AudioClip clip)
        {
            return PlaySound(clip, 1f, PoolLevel.GameLevel);
        }
        public static AudioSource PlaySound(AudioClip clip, float volume = 1f,
            PoolLevel poolLevel = PoolLevel.GameLevel, bool loop = false)
        {
            SoundPoolItem pool = Default.GetPool();
            pool.audioSource.clip = clip;
            pool.audioSource.loop = false;
            pool.audioSource.volume = volume;
            pool.audioSource.loop = loop;
            pool.audioSource.Play();
            pool.poolLevel = poolLevel;
            return pool.audioSource;
        }
        private SoundPoolItem GetPool()
        {
            for (int i = 0; i < soundPool.Count; i++)
            {
                if (!soundPool[i].audioSource.isPlaying)
                    return soundPool[i];
            }
            SoundPoolItem poolItem = new SoundPoolItem();
            GameObject go = new GameObject("Sound Pool Item");
            go.transform.SetParent(CameraManager.Default.cam.transform);

            poolItem.audioSource = go.AddComponent<AudioSource>();
            soundPool.Add(poolItem);
            return poolItem;
        }
        private void DestroyPoolItem(SoundPoolItem item)
        {
            if (item.audioSource != null && item.audioSource.gameObject != null)
            {
                Destroy(item.audioSource.gameObject);
                if (item.audioSource != null)
                    item.audioSource.Stop();
                item.audioSource = null;
            }
            item.idleTime = 0f;
            soundPool.Remove(item);
        }
        public void SetBackGroundMusic(AudioSimpleData audioBGM)
        {
            IEnumerator CrossFadeBGM(AudioSource from, AudioSource to)
            {
                float progress = 0f;
                float fromStartValue = 0f;
                float toStartValue = 0f;
                if (from != null)
                    fromStartValue = from.volume;
                if (to != null)
                    toStartValue = to.volume;
                while (progress <= 1f)
                {
                    yield return null;
                    progress += LayerDefault.DeltaTime;
                    if (from != null)
                        from.volume = Mathf.Lerp(fromStartValue, 0f, progress);
                    if (to != null)
                        to.volume = Mathf.Lerp(toStartValue, audioBGM.Volume * curVolume, progress);
                }
            }

            if (sourceBGMPrev != null)
                sourceBGMPrev.Stop();
            sourceBGMPrev = sourceBGMCur;
            if (audioBGM != null)
            {
                sourceBGMCur = audioBGM.Play(PoolLevel.Global);
                sourceBGMCur.ignoreListenerVolume = true;
                sourceBGMCur.volume = 0f;
            }
            else
                sourceBGMCur = null;
            StartCoroutine(CrossFadeBGM(sourceBGMPrev, sourceBGMCur));
        }
        public void SetVolume(float volume)
        {
            IEnumerator SmoothChangeVolume(float volumeTo)
            {
                float progress = 0f;
                float volumeFrom = sourceBGMCur.volume;
                while (progress <= 1f)
                {
                    yield return null;
                    progress += LayerDefault.DeltaTime;
                    sourceBGMCur.volume = Mathf.Lerp(volumeFrom, volumeTo, progress);
                }
            }
            curVolume = volume;
            if (sourceBGMCur != null)
                StartCoroutine((SmoothChangeVolume(volume)));
        }
    }
}
