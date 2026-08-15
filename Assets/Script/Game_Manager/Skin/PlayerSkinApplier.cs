
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.VFX;

public class PlayerSkinApplier : MonoBehaviour
{
    public static PlayerSkinApplier Instance;

    public SkinnedMeshRenderer outfitRenderer;

    public Transform hatAnchor;
    public Transform backAnchor;
    public Transform trailAnchor;
    public Transform hitAnchor;
    public Transform deadAnchor;

    [SerializeField] private GameObject trailBasePrefab;
    [SerializeField] private GameObject vfxHitBase;
    [SerializeField] private GameObject vfxDeadBase;
    GameObject head, back, trail, vfx_hit, vfx_dead;

    
    void Awake() => Instance = this;

    void Start()
    {
        if (trailAnchor.childCount > 0)
        {
            trail = trailAnchor.GetChild(0).gameObject;
        }
        SkinManager.Instance.OnSkinChanged += ApplyAll;
        ApplyAll();
    }

    public void ApplyAll()
    {
        var db = SkinDataBase.Instance;

        ApplyOutfit(db.Get(SkinManager.Instance.Get(SkinType.Outfit)));
        ApplyOptionalSkin(ref head, db.Get(SkinManager.Instance.Get(SkinType.Head))?.hatPrefab, hatAnchor);
        ApplyOptionalSkin(ref back, db.Get(SkinManager.Instance.Get(SkinType.Back))?.backPrefab, backAnchor);


        ApplyRequiredSkin(ref trail, db.Get(SkinManager.Instance.Get(SkinType.Trail))?.trailPrefab, trailBasePrefab, trailAnchor);
        ApplyRequiredSkin(ref vfx_hit, db.Get(SkinManager.Instance.Get(SkinType.Hit))?.hitEffect_Obj, vfxHitBase, hitAnchor);
        ApplyRequiredSkin(ref vfx_dead, db.Get(SkinManager.Instance.Get(SkinType.Dead))?.deadEffect_Obj, vfxDeadBase, deadAnchor);
    }
    /*
    void ApplydeadEffect(SkinData data)
    {
        // 1. Default from prefab (if any)
        if (deadEffectPrefab != null)
        {
            var prefabVfx = deadEffectPrefab.GetComponent<VisualEffect>();
            deadEffect = prefabVfx != null ? prefabVfx.visualEffectAsset : null;
        }
        else
        {
            // No prefab default -> clear so unquip properly resets
            deadEffect = null;
        }

        // 2. Override with SkinData if provided
        if (data != null && data.deadEffect != null)
        {
            deadEffect = data.deadEffect;
        }

        // 3. Apply to actual VisualEffect component.
        // Prefer explicit inspector-assigned component, fallback to same-GameObject, then any child.
        VisualEffect target = deadVFXComponent != null
            ? deadVFXComponent
            : GetComponent<VisualEffect>();

        if (target == null)
            target = GetComponentInChildren<VisualEffect>(true);

        if (target != null)
            target.visualEffectAsset = deadEffect;
    }
    void ApplyhitEffect(SkinData data)
    {
        /// 1. Default from prefab (if any)
        if (hitEffectPrefab != null)
        {
            var prefabVfx = hitEffectPrefab.GetComponent<VisualEffect>();
            hitEffect = prefabVfx != null ? prefabVfx.visualEffectAsset : null;
        }
        else
        {
            // No prefab default -> clear so unquip properly resets
            hitEffect = null;
        }

        // 2. Override with SkinData if provided
        if (data != null && data.hitEffect != null)
        {
            hitEffect = data.hitEffect;
        }

        // 3. Apply to actual VisualEffect component.
        VisualEffect target = hitVFXComponent != null
            ? hitVFXComponent
            : GetComponent<VisualEffect>();

        if (target == null)
            target = GetComponentInChildren<VisualEffect>(true);

        if (target != null)
            target.visualEffectAsset = hitEffect;

    }
    */
    void ApplyOutfit(SkinData data)
    {
        if (data == null)
        {
            if (outfitRenderer != null)
            {
                outfitRenderer.sharedMesh = null;
                outfitRenderer.material = null;
                outfitRenderer.enabled = false;
            }
            return;
        }

        // Apply outfit data
        if (outfitRenderer != null)
        {
            outfitRenderer.enabled = true;
            outfitRenderer.sharedMesh = data.mesh;
            outfitRenderer.material = data.material;
        }
    }

    void ApplyOptionalSkin(ref GameObject current, GameObject prefab, Transform anchor)
    {
        if (prefab != null)
        {
            UpdateVisual(ref current, prefab, anchor);
        }
        else
        {
            ClearVisual(ref current, anchor);
        }
    }

    // 2. Dùng cho Trail, VFX: Nếu skin null -> Quay về Base
    void ApplyRequiredSkin(ref GameObject current, GameObject prefab, GameObject basePrefab, Transform anchor)
    {
        // Luôn ưu tiên prefab từ data, nếu null thì ép dùng basePrefab
        GameObject target = (prefab != null) ? prefab : basePrefab;

        if (target != null)
        {
            UpdateVisual(ref current, target, anchor);
        }
    }

    // Hàm bổ trợ để tránh lặp code (Helper methods)
    void UpdateVisual(ref GameObject current, GameObject target, Transform anchor)
    {
        if (current == null || current.name != target.name + "(Clone)")
        {
            ClearVisual(ref current, anchor);
            current = Instantiate(target, anchor);
            current.transform.localPosition = Vector3.zero;
            current.transform.localRotation = Quaternion.identity;
            
            VisualEffect vfx = current.GetComponentInChildren<VisualEffect>();
            if (vfx != null)
            {
                vfx.enabled = true; // Đảm bảo component được bật
                vfx.Play();         // Ép hiệu ứng bắt đầu chạy
            }
        }
    }

    void ClearVisual(ref GameObject current, Transform anchor)
    {
        if (current != null) Destroy(current);
        current = null;
        foreach (Transform child in anchor) Destroy(child.gameObject);
    }



    public void EnableSkin(SkinType type)
    {
        switch (type)
        {
            case SkinType.Outfit:
                if (outfitRenderer != null)
                    outfitRenderer.enabled = true;
                break;
            case SkinType.Head:
                if (head != null)
                    head.SetActive(true);
                break;
            case SkinType.Back:
                if (back != null)
                    back.SetActive(true);
                break;
            
        }
    }
    public void DisableSkin(SkinType type)
    {
        switch (type)
        {
            case SkinType.Outfit:
                if (outfitRenderer != null)
                    outfitRenderer.enabled = false;
                break;
            case SkinType.Head:
                if (head != null)
                    head.SetActive(false);
                break;
            case SkinType.Back:
                if (back != null)
                    back.SetActive(false);
                break;
            
        }
    }
}
