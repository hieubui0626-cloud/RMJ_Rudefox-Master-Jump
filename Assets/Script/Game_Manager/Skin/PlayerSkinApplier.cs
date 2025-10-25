using UnityEngine;

public class PlayerSkinApplier : MonoBehaviour
{
    public static PlayerSkinApplier Instance;

    [Header("References")]
    public SkinnedMeshRenderer bodyRenderer; // Body gốc trong player
    public Transform headParent;
    public Transform backParent;
    public Transform trailParent;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        ApplyEquippedSkins();
    }

    private void OnEnable()
    {
        ApplyEquippedSkins();
    }

    public void ApplyEquippedSkins()
    {
        if (SkinManager.Instance == null) return;

        foreach (SkinType type in System.Enum.GetValues(typeof(SkinType)))
        {
            var skin = SkinManager.Instance.GetEquippedSkin(type);
            if (skin != null)
                ApplySkin(skin);
        }
    }

    public void ApplySkin(SkinData data)
    {
        switch (data.type)
        {
            case SkinType.Outfit:
                ApplyOutfitSkin(data);
                break;
            case SkinType.Head:
                ReplaceChildPrefab(headParent, data.prefab);
                break;
            case SkinType.Back:
                ReplaceChildPrefab(backParent, data.prefab);
                break;
            case SkinType.Trail:
                ReplaceChildPrefab(trailParent, data.prefab);
                break;
        }
    }

    // ✅ Outfit chỉ đổi mesh & material
    private void ApplyOutfitSkin(SkinData outfit)
    {
        if (bodyRenderer == null)
        {
            Debug.LogWarning("❌ Không tìm thấy Body Renderer trong Player!");
            return;
        }

        if (outfit.mesh != null)
            bodyRenderer.sharedMesh = outfit.mesh;

        if (outfit.material != null)
            bodyRenderer.sharedMaterial = outfit.material;

        Debug.Log($"🎽 Outfit {outfit.displayName} đã được áp.");
    }

    // Các slot khác vẫn dùng prefab
    private void ReplaceChildPrefab(Transform parent, GameObject prefab)
    {
        if (parent == null) return;

        foreach (Transform child in parent)
            Destroy(child.gameObject);

        if (prefab != null)
            Instantiate(prefab, parent);
    }
}
