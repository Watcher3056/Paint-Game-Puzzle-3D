using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using Pixeye.Actors;


// Placing the Purchaser class in the CompleteProject namespace allows it to interact with ScoreManager, 
// one of the existing Survival Shooter scripts.
namespace TeamAlpha.Source
{
    // Deriving the Purchaser class from IStoreListener enables it to receive messages from Unity Purchasing.
    public class ProcessorIAPManager : MonoBehaviour, IStoreListener
    {
        public static ProcessorIAPManager Default => _default;
        private static ProcessorIAPManager _default;

        public static event Action onInitialized = () => { };

        public bool IsSubscribed
        {
            get
            {
                if (Application.isEditor)
                    return true;
                else
                    return subscriptionManager?.getSubscriptionInfo()?.isSubscribed() == Result.True;
            }
        }
        public DateTime SubscriptionDateExpired
        {
            get
            {
                if (Application.isEditor)
                    return DateTime.MaxValue;
                else
                {
                    DateTime result = DateTime.MinValue;
                    if (subscriptionManager != null)
                    {
                        SubscriptionInfo info = subscriptionManager.getSubscriptionInfo();
                        result = info.getExpireDate();
                    }
                    return result;
                }
            }
        }
        private static bool unityPurchasingInitialized;

        protected IStoreController controller;       // The Unity Purchasing system.
        protected IExtensionProvider extensions;     // The store-specific Purchasing subsystems.s;
        protected ProductCatalog catalog;
        protected SubscriptionManager subscriptionManager;

        // Allows outside sources to know whether the full initialization has taken place.
        public static bool initializationComplete;

        public static readonly string SubscriptionProductID = "com.phogames.pixelpaintpuzzle";
        //public static event Action<int, string> OnItemPurchased = (a1, a2) => { };

        // Google Play Store-specific product identifier subscription product.
        //private static string kProductNameGooglePlaySubscription = "com.unity3d.subscription.original";

        public ProcessorIAPManager() => _default = this;
        private void Awake()
        {
            catalog = ProductCatalog.LoadDefaultCatalog();
            // If we haven't set up the Unity Purchasing reference
            if (controller == null)
            {
                // Begin to configure our connection to Purchasing
                InitializePurchasing();
            }
        }
        private bool IsInitialized()
        {
            // Only say we are initialized if both the Purchasing references are set.
            return controller != null && extensions != null;
        }
        private void InitializePurchasing()
        {
            StandardPurchasingModule module = StandardPurchasingModule.Instance();
            module.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;

            ConfigurationBuilder builder = ConfigurationBuilder.Instance(module);
            builder.AddProduct(SubscriptionProductID, ProductType.Subscription);

            //IAPConfigurationHelper.PopulateConfigurationBuilder(ref builder, _default.catalog);
            catalog = ProductCatalog.LoadDefaultCatalog();
            UnityPurchasing.Initialize(_default, builder);

            unityPurchasingInitialized = true;
        }
        /// <summary>
        /// Creates the static _default of CodelessIAPStoreListener and initializes purchasing
        /// </summary>

        public IStoreController StoreController
        {
            get { return controller; }
        }

        public IExtensionProvider ExtensionProvider
        {
            get { return extensions; }
        }

        public bool HasProductInCatalog(string productID)
        {
            foreach (var product in catalog.allProducts)
            {
                if (product.id == productID)
                {
                    return true;
                }
            }
            return false;
        }

        public Product GetProduct(string productID)
        {
            if (controller != null && controller.products != null && !string.IsNullOrEmpty(productID))
            {
                return controller.products.WithID(productID);
            }
            this.LogError("CodelessIAPStoreListener attempted to get unknown product " + productID);
            return null;
        }

        public void InitiatePurchase(string productID)
        {
            if (controller == null)
            {
                this.LogError("Purchase failed because Purchasing was not initialized correctly");
                return;
            }

            controller.InitiatePurchase(productID);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            initializationComplete = true;
            this.controller = controller;
            this.extensions = extensions;

            Product productSubscription = StoreController.products.WithID(SubscriptionProductID);
            subscriptionManager = new SubscriptionManager(productSubscription, null);

            onInitialized.Invoke();
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            this.LogError(string.Format("Purchasing failed to initialize. Reason: {0}", error.ToString()));
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
        {
            bool success = true;

            if (e.purchasedProduct.definition.id == SubscriptionProductID)
            {
                //OnItemPurchased(DataGameMain.Default.ProductAmountConsumableCrystalsBowl, e.purchasedProduct.definition.id);
                DataGameMain.Default.HintsLeft += 5;
                DataGameMain.Default.TimeBoostLeft += 5;
                if (PanelSubscriptionOfferFullscreen.Default.panel.CurState != Panel.State.Closed)
                    PanelSubscriptionOfferFullscreen.Default.panel.ClosePanel();
                if (PanelSubscriptionOfferPopup.Default.panel.CurState != Panel.State.Closed)
                    PanelSubscriptionOfferPopup.Default.panel.ClosePanel();
            }
            else
                success = false;
            // Or ... an unknown product has been purchased by this user. Fill in additional products here....
            if (success)
            {
                this.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", e.purchasedProduct.definition.id));
            }
            else
                this.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", e.purchasedProduct.definition.id));

            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            // we expect at least one receiver to get this message
            this.LogError("Failed purchase not correctly handled for product \"" + product.definition.id +
                              "\". Add an active IAPButton to handle this failure, or add an IAPListener to receive any unhandled purchase failures.");

            return;
        }
        public void BuyProductID(string productId)
        {
            // If Purchasing has been initialized ...
            if (IsInitialized())
            {
                // ... look up the Product reference with the general product identifier and the Purchasing 
                // system's products collection.
                Product product = controller.products.WithID(productId);

                // If the look up found a product for this device's store and that product is ready to be sold ... 
                if (product != null && product.availableToPurchase)
                {
                    this.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                    // ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
                    // asynchronously.
                    controller.InitiatePurchase(product);
                }
                // Otherwise ...
                else
                {
                    // ... report the product look-up failure situation  
                    this.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            // Otherwise ...
            else
            {
                // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
                // retrying initiailization.
                this.Log("BuyProductID FAIL. Not initialized.");
            }
        }

        public string GetItemPrice(string itemId)
        {
            try
            {
                var metadata = controller.products.WithID(itemId).metadata;
                return $"{metadata.localizedPrice} {metadata.isoCurrencyCode}";
            }
            catch (Exception e)
            {
                return "...";
            }
        }
        // Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
        // Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
        public void RestorePurchases()
        {
            // If Purchasing has not yet been set up ...
            if (!IsInitialized())
            {
                // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
                this.Log("RestorePurchases FAIL. Not initialized.");
                return;
            }

            // If we are running on an Apple device ... 
            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                // ... begin restoring purchases
                this.Log("RestorePurchases started ...");

                // Fetch the Apple store-specific subsystem.
                var apple = extensions.GetExtension<IAppleExtensions>();
                // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
                // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
                apple.RestoreTransactions((result) =>
                {
                    // The first phase of restoration. If no more responses are received on ProcessPurchase then 
                    // no purchases are available to be restored.
                    this.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
                });
            }
            // Otherwise ...
            else
            {
                // We are not running on an Apple device. No work is necessary to restore purchases.
                this.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
            }
        }


        //  
        // --- IStoreListener
        //
    }
}