using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamAlpha.Source
{
    public class PanelSubscriptionOfferFullscreen : PanelSubscriptionOffer
    {
        public static PanelSubscriptionOfferFullscreen Default { get; private set; }

        public PanelSubscriptionOfferFullscreen() => Default = this;

        protected override void Awake()
        {
            base.Awake();

            buttonClose.onClick.AddListener(HandleCloseByUser);
            buttonContinue.onClick.AddListener(HandleCloseByUser);
        }
        private void HandleCloseByUser()
        {
            ProcessorSoundPool.Default.SetBackGroundMusic(DataGameMain.Default.audioBGMMain);
            Banner.Default.RequestBanner();
        }
    }
}
