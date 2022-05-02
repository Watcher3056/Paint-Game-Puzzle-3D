using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace TeamAlpha.Source
{
    public class PanelStars : MonoBehaviour
    {
        public static PanelStars Default { get; private set; }

        [Serializable]
        public class Star
        {
            [Required]
            public Image image;
            public AudioSimpleData audio;
        }

        [Required]
        public Panel panel;
        public AudioSimpleData audioOnWin;
        public List<Star> stars;
        public float animSpeed;

        public PanelStars() => Default = this;

        private void Awake()
        {
            panel.OnPanelShow += HandlePanelShow;
        }
        private void HandlePanelShow()
        {
            foreach (Star star in stars)
            {
                star.image.color = new Color(1f, 1f, 1f, 0f);
                star.image.transform.localScale = Vector3.one * 3f;
            }

            StartCoroutine(_Anim());
        }
        private IEnumerator _Anim()
        {
            for (int i = 0; i < LevelController.Current.StarsAchieved; i++)
            {
                yield return new WaitForSeconds(1f / animSpeed);
                Star star = stars[i];
                star.image.transform.DOScale(1f, 1f / animSpeed);
                star.image.DOColor(Color.white, 1f / animSpeed).onComplete = () =>
                {
                    star.audio.Play(ProcessorSoundPool.PoolLevel.GameLevel);
                };
                if (i + 1 == LevelController.Current.StarsAchieved)
                {
                    if (LevelController.Current.LevelCompleted)
                    {
                        foreach (ParticleSystem particle in LayerDefault.Default.particlesOnWin)
                            particle.Play(true);
                        LayerDefault.Default.audioOnWin.Play(ProcessorSoundPool.PoolLevel.GameLevel);
                    }
                }
            }

            yield return new WaitForSeconds(0.5f / animSpeed);
            if (LevelController.Current.LevelCompleted)
                audioOnWin.Play(ProcessorSoundPool.PoolLevel.GameLevel);
        }
    }
}
