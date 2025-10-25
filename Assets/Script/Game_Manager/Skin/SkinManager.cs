using System.Collections.Generic;
using UnityEngine;

public class SkinManager : MonoBehaviour
{
    public static SkinManager Instance;

    [Header("Danh sách Skin khả dụng")]
    public List<SkinData> allSkins;

    private HashSet<string> unlockedSkins = new HashSet<string>();
    private Dictionary<SkinType, string> equippedSkins = new Dictionary<SkinType, string>();

    private bool dataLoaded = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Nếu Firebase sẵn sàng thì load, còn không thì chờ callback
        if (FirebaseManager.IsReady)
            LoadFromFirebase();
        else
            FirebaseManager.OnFirebaseReady += LoadFromFirebase;
    }

    private void LoadFromFirebase()
    {
        FirebaseManager.Instance.LoadPlayerSkinData((tokens, unlockedList, equippedMap) =>
        {
            // 🔹 Dữ liệu Firebase
            unlockedSkins = new HashSet<string>(unlockedList);
            equippedSkins = new Dictionary<SkinType, string>(equippedMap);

            // 🔹 Nếu rỗng → fallback PlayerPrefs (offline)
            if (unlockedSkins.Count == 0)
            {
                string unlockedJson = PlayerPrefs.GetString("UnlockedSkins", "");
                if (!string.IsNullOrEmpty(unlockedJson))
                    unlockedSkins = new HashSet<string>(unlockedJson.Split(','));
            }

            foreach (SkinType type in System.Enum.GetValues(typeof(SkinType)))
            {
                if (!equippedSkins.ContainsKey(type))
                {
                    string key = $"Equipped_{type}";
                    string skinId = PlayerPrefs.GetString(key, "");
                    if (!string.IsNullOrEmpty(skinId))
                        equippedSkins[type] = skinId;
                }
            }

            dataLoaded = true;
            Debug.Log("✅ Skin data loaded from Firebase");
        });
    }

    // ==========================================================
    // 🔹 KIỂM TRA VÀ MỞ KHÓA
    // ==========================================================
    public bool IsUnlocked(string id) => unlockedSkins.Contains(id);

    public void UnlockSkin(SkinData skin)
    {
        if (!dataLoaded)
        {
            Debug.LogWarning("⏳ Chưa load xong dữ liệu skin, vui lòng chờ...");
            return;
        }

        if (IsUnlocked(skin.skinId))
        {
            Debug.Log($"✅ Skin {skin.displayName} đã được mở khóa trước đó");
            return;
        }

        FirebaseManager.Instance.GetTotalTokens(total =>
        {
            if (total >= skin.cost)
            {
                FirebaseManager.Instance.UpdateTotalTokens(-skin.cost, newTotal =>
                {
                    unlockedSkins.Add(skin.skinId);
                    SaveToFirebase();
                    SaveLocalCache();

                    Debug.Log($"✅ Đã mở khóa skin {skin.displayName}, token còn lại: {newTotal}");
                });
            }
            else
            {
                Debug.LogWarning("❌ Không đủ token để mở khóa skin này!");
            }
        });
    }

    // ==========================================================
    // 🔹 TRANG BỊ SKIN
    // ==========================================================
    public void EquipSkin(SkinData skin)
    {
        if (!dataLoaded)
        {
            Debug.LogWarning("⏳ Dữ liệu chưa tải xong!");
            return;
        }

        if (!IsUnlocked(skin.skinId))
        {
            Debug.LogWarning("❌ Skin chưa được mở khóa!");
            return;
        }

        equippedSkins[skin.type] = skin.skinId;
        SaveToFirebase();
        SaveLocalCache();

        // Áp skin ngay lên Player
        if (PlayerSkinApplier.Instance != null)
            PlayerSkinApplier.Instance.ApplySkin(skin);

        Debug.Log($"🎽 Đã trang bị skin: {skin.displayName}");
    }

    // ==========================================================
    // 🔹 LƯU DỮ LIỆU (Firebase + Local Cache)
    // ==========================================================
    private void SaveToFirebase()
    {
        FirebaseManager.Instance.SaveUnlockedSkins(new List<string>(unlockedSkins));
        FirebaseManager.Instance.SaveEquippedSkins(new Dictionary<SkinType, string>(equippedSkins));
    }

    private void SaveLocalCache()
    {
        PlayerPrefs.SetString("UnlockedSkins", string.Join(",", unlockedSkins));
        foreach (var kvp in equippedSkins)
            PlayerPrefs.SetString($"Equipped_{kvp.Key}", kvp.Value);
        PlayerPrefs.Save();
    }

    // ==========================================================
    // 🔹 LẤY SKIN ĐANG TRANG BỊ
    // ==========================================================
    public SkinData GetEquippedSkin(SkinType type)
    {
        if (equippedSkins.TryGetValue(type, out string id))
            return allSkins.Find(s => s.skinId == id);
        return null;
    }
}
