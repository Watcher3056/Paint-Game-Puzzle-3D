using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;

namespace TeamAlpha.Source
{
    [Serializable]
    public class AudioSimpleData
    {
        [Required]
        public AudioClip clip;

        public float Volume
        {
            get => volume;
        }
        [Range(0f, 1f), SerializeField]
        private float volume = 1f;
        public bool Loop
        {
            get => loop;
        }
        [SerializeField]
        private bool loop;

        public AudioSource Play(ProcessorSoundPool.PoolLevel poolLevel)
        {
            return ProcessorSoundPool.PlaySound(clip, volume, poolLevel, loop);
        }
    }
}