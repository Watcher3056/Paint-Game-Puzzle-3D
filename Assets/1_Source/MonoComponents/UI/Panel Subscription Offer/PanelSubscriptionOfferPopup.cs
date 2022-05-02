using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace TeamAlpha.Source
{
    public class PanelSubscriptionOfferPopup : PanelSubscriptionOffer
    {
        public static PanelSubscriptionOfferPopup Default { get; private set; }

        public PanelSubscriptionOfferPopup() => Default = this;
        protected override void Awake()
        {
            base.Awake();
        }
    }
}
