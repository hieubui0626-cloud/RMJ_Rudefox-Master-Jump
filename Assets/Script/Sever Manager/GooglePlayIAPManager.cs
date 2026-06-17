using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class GooglePlayIAPManager : MonoBehaviour, IDetailedStoreListener
{
    public static GooglePlayIAPManager Instance;

    private IStoreController storeController;
    private IExtensionProvider storeExtensionProvider;

    // Định nghĩa danh sách các Product ID ứng với số lượng token
    private readonly Dictionary<string, int> tokenPackages = new Dictionary<string, int>()
    {
        { "com.kizzul.rmj.token_100", 100 },
        { "com.kizzul.rmj.token_500", 500 },
        { "com.kizzul.rmj.token_1000", 1000 },
        { "com.kizzul.rmj.token_2500", 2500 },
        { "com.kizzul.rmj.token_5000", 5000 }
    };

    // Event thông báo cho UI cập nhật hoặc hiển thị hiệu ứng nhận quà
    public static event Action<int> OnPurchaseSuccess;
    public static event Action<string> OnPurchaseFailedEvent;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Nếu Firebase đã sẵn sàng thì khởi tạo IAP ngay, nếu chưa thì đợi Event từ Firebase
        if (FirebaseManager.IsReady)
        {
            InitializePurchasing();
        }
        else
        {
            FirebaseManager.OnFirebaseReady += InitializePurchasing;
        }
    }

    private void OnDestroy()
    {
        FirebaseManager.OnFirebaseReady -= InitializePurchasing;
    }

    public void InitializePurchasing()
    {
        if (IsInitialized()) return;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        // Đăng ký các gói hàng với Unity IAP (Tất cả đều là Consumable để mua được nhiều lần)
        foreach (var productID in tokenPackages.Keys)
        {
            builder.AddProduct(productID, ProductType.Consumable);
        }

        UnityPurchasing.Initialize(this, builder);
    }

    private bool IsInitialized()
    {
        return storeController != null && storeExtensionProvider != null;
    }

    /// <summary>
    /// Hàm gọi từ Button UI để tiến hành mua hàng
    /// </summary>
    /// <param name="productId">Ví dụ: "com.Kizzul.RMJ.tokens_100"</param>
    public void BuyProductID(string productId)
    {
        if (IsInitialized())
        {
            Product product = storeController.products.WithID(productId);

            if (product != null && product.availableToPurchase)
            {
                Debug.Log($"[IAP] Đang tiến hành mua sản phẩm: {product.definition.id}");
                storeController.InitiatePurchase(product);
            }
            else
            {
                Debug.LogError("[IAP] Sản phẩm không tồn tại hoặc không hợp lệ để mua.");
                OnPurchaseFailedEvent?.Invoke("Sản phẩm không khả dụng.");
            }
        }
        else
        {
            Debug.LogError("[IAP] Hệ thống IAP chưa được khởi tạo thành công.");
            OnPurchaseFailedEvent?.Invoke("Chưa kết nối được với Google Play.");
        }
    }

    #region Store Listeners
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("✅ [IAP] Khởi tạo Unity IAP thành công!");
        storeController = controller;
        storeExtensionProvider = extensions;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        OnInitializeFailed(error, null);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"❌ [IAP] Khởi tạo thất bại. Lý do: {error}. Message: {message}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        string purchasedProductId = args.purchasedProduct.definition.id;

        if (tokenPackages.ContainsKey(purchasedProductId))
        {
            int tokensToAdd = tokenPackages[purchasedProductId];
            Debug.Log($"✅ [IAP] Mua thành công gói: {purchasedProductId}. Tiến hành cộng {tokensToAdd} Tokens.");

            // GỌI FIREBASE MANAGER ĐỂ CỘNG TOKEN VÀ ĐỒNG BỘ LÊN CLOUD
            if (FirebaseManager.Instance != null)
            {
                FirebaseManager.Instance.UpdateTotalTokens(tokensToAdd, (newTotalTokens) =>
                {
                    Debug.Log($"🔄 [Firebase] Đã cập nhật xong dữ liệu lên Server. Số token mới: {newTotalTokens}");
                    // Kích hoạt event thông báo cho UI (nếu có)
                    OnPurchaseSuccess?.Invoke(tokensToAdd);
                });
            }
            else
            {
                Debug.LogError("❌ [Firebase] Không tìm thấy Instance của FirebaseManager để lưu Token!");
            }
        }
        else
        {
            Debug.LogWarning($"[IAP] Mua thành công sản phẩm lạ không nằm trong danh mục Token: {purchasedProductId}");
        }

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError($"❌ [IAP] Mua sản phẩm {product.definition.storeSpecificId} thất bại. Lý do: {failureReason}");
        OnPurchaseFailedEvent?.Invoke(failureReason.ToString());
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription response)
    {
        Debug.LogError($"❌ [IAP] Mua sản phẩm {product.definition.storeSpecificId} thất bại. Chi tiết: {response.message}");
        OnPurchaseFailedEvent?.Invoke(response.message);
    }
    #endregion
}