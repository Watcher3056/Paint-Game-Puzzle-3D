using GoogleMobileAds.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamAlpha.Source
{
    public class RewardedVideo : MonoBehaviour
    {
        public static RewardedVideo Default { get; private set; }


        private RewardedAd rewardedAd;
        private Action<bool> onComplete;

        public RewardedVideo() => Default = this;
        public void RequestRewardedVideo()
        {
#if UNITY_ANDROID
            string adUnitId = "ca-app-pub-4826715583137572/5106285272";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-4826715583137572/4216392354";
#else
        string adUnitId = "unexpected_platform";
#endif
            if (rewardedAd != null)
                rewardedAd.Destroy();

            this.rewardedAd = new RewardedAd(adUnitId);
            // Create an empty ad request.
            AdRequest request = new AdRequest.Builder().Build();
            // Load the rewarded ad with the request.
            this.rewardedAd.LoadAd(request);

            this.rewardedAd.OnAdFailedToLoad += (s, e) =>
            {
                ProcessorDeferredOperation.Default.Add(RequestRewardedVideo, true, true, 10f);
            };
            // Called when the user should be rewarded for interacting with the ad.
            this.rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
            this.rewardedAd.OnAdClosed += HandleAdClosed;
        }
        private void HandleAdClosed(object sender, EventArgs e)
        {
            LayerDefault.Default.Paused = false;
        }
        public void ShowRewardedVideo(Action<bool> onComplete = default)
        {
            LayerDefault.Default.Paused = true;
            this.onComplete = onComplete;
            if (rewardedAd.IsLoaded())
            {
                LayerDefault.Default.Paused = true;
                rewardedAd.Show();
            }
            else if (this.onComplete != null)
            {
                this.onComplete.Invoke(false);
                LayerDefault.Default.Paused = false;
            }
        }
        private void HandleUserEarnedReward(object sender, Reward e)
        {
            if (this.onComplete != null)
                this.onComplete.Invoke(true);
            RequestRewardedVideo();
        }
    }
}
