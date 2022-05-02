using GoogleMobileAds.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamAlpha.Source
{
    public class Interstitial : MonoBehaviour
    {
        public static Interstitial Default { get; private set; }

        private InterstitialAd interstitial;
        private Action<bool> curCallback;

        public Interstitial() => Default = this;
        public void RequestInterstitial()
        {
#if UNITY_ANDROID
            string adUnitId = "ca-app-pub-4826715583137572/7868179983";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-4826715583137572/9468719033";
#else
        string adUnitId = "unexpected_platform";
#endif

            if (interstitial != null)
                interstitial.Destroy();
            // Initialize an InterstitialAd.
            interstitial = new InterstitialAd(adUnitId);
            // Create an empty ad request.
            AdRequest request = new AdRequest.Builder().Build();
            // Load the interstitial with the request.
            interstitial.LoadAd(request);
            interstitial.OnAdClosed += HandleAdClosed;
            interstitial.OnAdFailedToLoad += (s, e) =>
            {
                ProcessorDeferredOperation.Default.Add(() => RequestInterstitial(), true, true, 10f);
            };
        }
        private void HandleAdClosed(object sender, EventArgs e)
        {
            LayerDefault.Default.Paused = false;
        }
        public void ShowInterstitial(Action<bool> onComplete = default)
        {
            LayerDefault.Default.Paused = true;
            curCallback = onComplete;
            if (interstitial.IsLoaded())
            {
                if (onComplete != null)
                    interstitial.OnAdClosed += OnAdClosed;
                interstitial.Show();
            }
            else if (this.curCallback != null)
            {
                this.curCallback.Invoke(false);
                LayerDefault.Default.Paused = false;
            }
        }
        private void OnAdClosed(object sender, EventArgs e)
        {
            if (this.curCallback != null)
                this.curCallback.Invoke(true);
            RequestInterstitial();
        }
    }
}
