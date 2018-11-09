using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using LitJson;
using isg;

public class purchaseController : MonoBehaviour, IStoreListener
{
	private static IStoreController storeController;
	private static IExtensionProvider extensionProvider;

	private storeController stc = null;
	private socketController sc = null;
	private rootController rc = new rootController ();

	// 상품 ID
	public const string p1Telescope10 = "p_1_telescope_10";
	public const string p1Telescope30 = "p_1_telescope_30";
	public const string p1Telescope60 = "p_1_telescope_60";
	public const string p1Telescope100 = "p_1_telescope_100";
	public const string p2Mapreset1 = "p_2_mapreset_1";

	void Start ()
	{
		stc = new isg.storeController ();
		sc = new socketController ();
		InitializePurchasing ();
	}

	private bool IsInitialized ()
	{
		return (storeController != null && extensionProvider != null);
	}

	public void InitializePurchasing ()
	{
		if (IsInitialized ())
			return;

		var module = StandardPurchasingModule.Instance ();

		ConfigurationBuilder builder = ConfigurationBuilder.Instance (module);

		builder.AddProduct (p1Telescope10, ProductType.Consumable, new IDs {
			{ p1Telescope10, AppleAppStore.Name },
			{ p1Telescope10, GooglePlay.Name },
		});
		builder.AddProduct (p1Telescope30, ProductType.Consumable, new IDs {
			{ p1Telescope30, AppleAppStore.Name },
			{ p1Telescope30, GooglePlay.Name },
		});
		builder.AddProduct (p1Telescope60, ProductType.Consumable, new IDs {
			{ p1Telescope60, AppleAppStore.Name },
			{ p1Telescope60, GooglePlay.Name },
		});
		builder.AddProduct (p1Telescope100, ProductType.Consumable, new IDs {
			{ p1Telescope100, AppleAppStore.Name },
			{ p1Telescope100, GooglePlay.Name },
		});
		builder.AddProduct (p2Mapreset1, ProductType.Consumable, new IDs {
			{ p2Mapreset1, AppleAppStore.Name },
			{ p2Mapreset1, GooglePlay.Name },
		});

		UnityPurchasing.Initialize (this, builder);
	}

	public void BuyProductID (string productId)
	{
		storeData.isStoreClick = true;

		try {
			if (IsInitialized ()) {
				Product p = storeController.products.WithID (productId);

				if (p != null && p.availableToPurchase) {
					Debug.Log (string.Format ("[InPurchase] product asychronously: '{0}'", p.definition.id));
					storeController.InitiatePurchase (p);
				} else {
					Debug.Log ("[InPurchase] BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
				}
			} else {
				Debug.Log ("[InPurchase] BuyProductID FAIL. Not initialized.");
			}
		} catch (Exception e) {
			Debug.Log ("[InPurchase] BuyProductID: FAIL. Exception during purchase. " + e);
		}
	}

	public void RestorePurchase ()
	{
		if (!IsInitialized ()) {
			Debug.Log ("[InPurchase] RestorePurchases FAIL. Not initialized.");
			return;
		}

		if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer) {
			Debug.Log ("[InPurchase] RestorePurchases started");

			var apple = extensionProvider.GetExtension<IAppleExtensions> ();

			apple.RestoreTransactions
			(
				(result) => {
					Debug.Log ("[InPurchase] RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
				}
			);
		} else {
			Debug.Log ("[InPurchase] RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
		}
	}

	public void OnInitialized (IStoreController sc, IExtensionProvider ep)
	{
		Debug.Log ("[InPurchase] OnInitialized : PASS");

		storeController = sc;
		extensionProvider = ep;
	}

	public void OnInitializeFailed (InitializationFailureReason reason)
	{
		Debug.Log ("[InPurchase] OnInitializeFailed InitializationFailureReason:" + reason);
	}

	public PurchaseProcessingResult ProcessPurchase (PurchaseEventArgs args)
	{
		Debug.Log (string.Format ("[InPurchase] ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));

		if (!storeData.isStoreClick) {
			return PurchaseProcessingResult.Complete;
		}

		var validator = new CrossPlatformValidator (GooglePlayTangle.Data (),
			                AppleTangle.Data (), Application.identifier);

		try {
			var result = validator.Validate (args.purchasedProduct.receipt);

			Debug.Log ("Receipt is valid. Contents:");

			foreach (IPurchaseReceipt productReceipt in result) {
				GooglePlayReceipt google = productReceipt as GooglePlayReceipt;

				if (null != google) {
					validationIAB (args.purchasedProduct.definition.id, google.purchaseToken);
				}
			}

		} catch (IAPSecurityException) {
			Debug.Log ("Invalid receipt, not unlocking content");
		}

		return PurchaseProcessingResult.Complete;
	}

	public void OnPurchaseFailed (Product product, PurchaseFailureReason failureReason)
	{
		Debug.Log (string.Format ("[InPurchase] OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
	}

	private void validationIAB (string productId, string token)
	{
		JsonData req = new JsonData ();
		req ["step"] = "pr1";
		req ["uid"] = userData.userId;
		req ["productId"] = productId;
		req ["token"] = token;

		sc.wsSend (req);
	}
}