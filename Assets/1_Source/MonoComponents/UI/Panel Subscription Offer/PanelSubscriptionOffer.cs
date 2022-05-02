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
    public class PanelSubscriptionOffer : MonoBehaviour
    {
        [Required]
        public Panel panel;
        [Required]
        public Button buttonContinue;
        [Required]
        public Button buttonClose;
        [Required]
        public Button buttonPrivacyPolicy;
        [Required]
        public Button buttonTermsOfUse;

        private Sequence sequence;
        protected virtual void Awake()
        {
            sequence = DOTween.Sequence();
            Tweener tweener = buttonContinue.transform.DOShakePosition(1f, Vector3.right * 100f, 7, 0f);
            sequence.Append(tweener);
            sequence.SetDelay(4f).SetLoops(-1);
            sequence.Play();

            buttonClose.onClick.AddListener(() =>
            {
                DataGameMain.Default.audioOnButtonClick.Play(ProcessorSoundPool.PoolLevel.Global);
                panel.ClosePanel();
            });
            buttonContinue.onClick.AddListener(() =>
            {
                DataGameMain.Default.audioOnButtonClick.Play(ProcessorSoundPool.PoolLevel.Global);
                ProcessorIAPManager.Default.BuyProductID(ProcessorIAPManager.SubscriptionProductID);
            });
            buttonPrivacyPolicy.onClick.AddListener(() =>
            {
                DataGameMain.Default.audioOnButtonClick.Play(ProcessorSoundPool.PoolLevel.Global);
                Application.OpenURL(DataGameMain.Default.URLPrivacyPolicy);
            });
            buttonTermsOfUse.onClick.AddListener(() =>
            {
                DataGameMain.Default.audioOnButtonClick.Play(ProcessorSoundPool.PoolLevel.Global);
                Application.OpenURL(DataGameMain.Default.URLTermsOfUse);
            });

            panel.OnPanelShow += HandlePanelShow;
        }
        private void HandlePanelShow()
        {
            //buttonClose.gameObject.SetActive(false);

            //ProcessorDeferredOperation.Default.Add(() =>
            //{
            //    buttonClose.gameObject.SetActive(true);
            //}, true, false, 3f);
        }
    }
}
