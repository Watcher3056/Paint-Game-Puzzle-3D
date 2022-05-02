using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TeamAlpha.Source
{
    public class PanelLoadingScreen : MonoBehaviour
    {
        public static PanelLoadingScreen Default { private set; get; }

        [Required]
        public Panel panel;
        [Required]
        public Slider slider;
        [Required]
        public Camera tempCamera;

        private bool loadingScreenOpened;
        // Use this for initialization

        public PanelLoadingScreen() => Default = this;
        private void Start()
        {
            slider.value = 0f;

            Load(
                () =>
                {
                    Destroy(tempCamera.gameObject);
                    SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(1));
                },
                () => SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive));
        }
        public void Load(Action onComplete = default, params Func<AsyncOperation>[] loadActions)
        {
            StartCoroutine(_Load(onComplete, loadActions));
        }
        private IEnumerator _Load(Action onComplete = default, params Func<AsyncOperation>[] loadActions)
        {
            ToggleLoadingScreen(true);
            while (!loadingScreenOpened)
                yield return null;

            float curProgress = 0f;
            float finalProgress = loadActions.Length;

            foreach (Func<AsyncOperation> loadAction in loadActions)
            {
                AsyncOperation asyncOperation = loadAction.Invoke();
                bool unusedAssetsUnloaded = false;
            unloadUnusedAssets:
                while (asyncOperation.progress < 1f)
                {
                    yield return null;
                    float progress = (curProgress + asyncOperation.progress) / finalProgress;
                    if (progress == 1f)
                        SetProgress(progress);
                }
                if (!unusedAssetsUnloaded)
                {
                    GC.Collect();
                    asyncOperation = Resources.UnloadUnusedAssets();
                    unusedAssetsUnloaded = true;
                    goto unloadUnusedAssets;
                    curProgress += 1f;
                }
            }
            //ToggleLoadingScreen(false);
            if (onComplete != null)
                onComplete.Invoke();
        }
        public void ToggleLoadingScreen(bool arg, Action onComplete = default)
        {
            StartCoroutine(_ToggleLoadingScreen(arg, onComplete));
        }
        private IEnumerator _ToggleLoadingScreen(bool arg, Action onComplete = default)
        {
            if (arg)
                panel.OpenPanel();
            else
            {
                yield return new WaitForSeconds(0.5f);
                panel.ClosePanel();
            }
            yield return null;
            panel.animancer.States.Current.Events.OnEnd = () =>
            {
                if (onComplete != null)
                    onComplete.Invoke();
                loadingScreenOpened = arg;
            };
        }
        public void SetProgress(float progress)
        {
            slider.DOKill();
            slider.DOValue(progress, 1f).onComplete = () => ToggleLoadingScreen(false);
        }
    }
}