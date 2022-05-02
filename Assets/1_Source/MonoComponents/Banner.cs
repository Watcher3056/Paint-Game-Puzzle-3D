using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;


namespace TeamAlpha.Source
{
    public class Banner : MonoBehaviour
    {
        public static Banner Default { get; private set; }

        [Required]
        public Image image;

        public BannerView bannerView;

        public Banner() => Default = this;
        public void RequestBanner()
        {
            if (DataGameMain.Default.SubscriptionActivated)
                return;
            image.gameObject.SetActive(false);
#if UNITY_ANDROID
            string adUnitId = "ca-app-pub-4826715583137572/5078365112";
#elif UNITY_IPHONE
            string adUnitId = "ca-app-pub-4826715583137572/8782323329";
#else
            string adUnitId = "unexpected_platform";
#endif

            // Create a 320x50 banner at the top of the screen.
            AdSize adaptiveSize =
                AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);

            this.bannerView = new BannerView(adUnitId, adaptiveSize, AdPosition.Bottom);

            // Create an empty ad request.
            AdRequest request = new AdRequest.Builder().Build();

            // Load the banner with the request.
            this.bannerView.LoadAd(request);
        }

    }
}
