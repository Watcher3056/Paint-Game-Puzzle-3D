using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeamAlpha.Source
{
    public class PanelTop : MonoBehaviour
    {
        public static PanelTop Default { get; private set; }

        [Required]
        public Panel panel;
        [Required]
        public Button buttonLevels;
        [Required]
        public Button buttonSettings;
        [Required]
        public TextMeshProUGUI textDiamondsCounter;
        [Required]
        public Image imageDiamonds;
        [Required]
        public GameObject diamondViewTemplate;
        public AudioSimpleData audioDiamondEndMove;
        public AudioSimpleData audioDiamondStartMove;
        public float scoresAnimTime;
        public float diamondsAddIntense;
        public float diamondsAddFlySpeed;

        private int curDiamonds;
        private Tweener tweenerScoresCounter;

        public PanelTop() => Default = this;
        private void Start()
        {
            buttonLevels.onClick.AddListener(HandleButtonLevelsClick);
            buttonSettings.onClick.AddListener(HandleButtonSettingsClick);
            ProcessorObserver.Default.Add(() => DataGameMain.Default.Diamonds, diamounds => UpdateViewDiamondsCounter(), true);
            UpdateViewDiamondsCounter();
        }
        private void HandleButtonLevelsClick()
        {
            DataGameMain.Default.audioOnButtonClick.Play(ProcessorSoundPool.PoolLevel.Global);
            PanelLevels.Default.panel.OpenPanel();
        }
        private void HandleButtonSettingsClick()
        {
            DataGameMain.Default.audioOnButtonClick.Play(ProcessorSoundPool.PoolLevel.Global);
            PanelSettings.Default.panel.OpenPanel();
        }
        private void UpdateViewDiamondsCounter()
        {
            if (tweenerScoresCounter != null)
            {
                imageDiamonds.transform.DOKill();
                tweenerScoresCounter.Kill();
                tweenerScoresCounter = null;
            }

            if (imageDiamonds.transform.localScale.Equals(Vector3.one))
                imageDiamonds.transform.DOScale(1.25f, 0.25f).onComplete = () =>
                {
                    imageDiamonds.transform.DOScale(1f, 0.25f);
                };
            else
                imageDiamonds.transform.DOScale(1f, 0.25f);
            tweenerScoresCounter = DOTween.To(() => curDiamonds, x => curDiamonds = x, DataGameMain.Default.Diamonds, scoresAnimTime)
                .OnUpdate(() =>
                {
                    textDiamondsCounter.text = curDiamonds.ToString();
                });
        }
        public void AddDiamonds(int amount, Vector3 position, Action onComplete = default)
        {
            StartCoroutine(_AddDiamonds(amount, position, onComplete));
        }
        private IEnumerator _AddDiamonds(int amount, Vector3 position, Action onComplete = default)
        {
            for (int i = 0; i < 10 && i < amount; i++)
            {
                yield return new WaitForSeconds(1f / diamondsAddIntense);
                GameObject diamond = Instantiate(diamondViewTemplate, UIManager.Default.mainCanvas.transform);
                //if (amount >= 10)
                //    diamond.transform.localScale += Vector3.one * 0.25f;
                //if (amount >= 100)
                //    diamond.transform.localScale += Vector3.one * 0.25f;
                diamond.SetActive(true);
                diamond.GetComponentInChildren<TextMeshProUGUI>().gameObject.SetActive(false);
                diamond.transform.SetAsLastSibling();
                diamond.transform.position = position;

                bool isFirst = i == 0;
                bool isLast = i + 1 == 10 || i + 1 == amount;

                audioDiamondStartMove.Play(ProcessorSoundPool.PoolLevel.GameLevel);
                Tweener tweener = diamond.transform.DOMove(imageDiamonds.transform.position, 1f / diamondsAddFlySpeed);
                tweener.onComplete = () =>
                {
                    audioDiamondEndMove.Play(ProcessorSoundPool.PoolLevel.GameLevel);
                    Destroy(diamond);
                    if (isFirst)
                    {
                        DataGameMain.Default.Diamonds += amount;
                    }
                    if (isLast)
                    {
                        if (onComplete != null)
                            onComplete.Invoke();
                    }
                };
            }
            yield return null;
        }
    }
}
