
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
        if (trailAnchor.childCount > 0) trail = trailAnchor.GetChild(0).gameObject;
        SkinManager.Instance.OnSkinChanged += ApplyAll;
        ApplyAll();
        Debug.Log("PlayerSkinApplier: Start() - Applied all skins.");
    }

    public void ApplyAll()
    {
        var db = SkinDataBase.Instance;
        if (db == null || SkinManager.Instance == null) return;

        // 1. Outfit
        SkinSlotData outfitSlot = db.GetSlotByID(SkinManager.Instance.Get(SkinType.Outfit));
        ApplyOutfitSlot(outfitSlot);

        // 2. Head & Back (Optional)
        SkinSlotData headSlot = db.GetSlotByID(SkinManager.Instance.Get(SkinType.Head));
        ApplyOptionalSkin(ref head, headSlot?.prefab, hatAnchor);

        SkinSlotData backSlot = db.GetSlotByID(SkinManager.Instance.Get(SkinType.Back));
        ApplyOptionalSkin(ref back, backSlot?.prefab, backAnchor);

        // 3. Trail & VFX (Required - có Fallback)
        SkinSlotData trailSlot = db.GetSlotByID(SkinManager.Instance.Get(SkinType.Trail));
        ApplyRequiredSkin(ref trail, trailSlot?.prefab, trailBasePrefab, trailAnchor);

        SkinSlotData hitSlot = db.GetSlotByID(SkinManager.Instance.Get(SkinType.Hit));
        ApplyRequiredSkin(ref vfx_hit, hitSlot?.vfxObj, vfxHitBase, hitAnchor);

        SkinSlotData deadSlot = db.GetSlotByID(SkinManager.Instance.Get(SkinType.Dead));
        ApplyRequiredSkin(ref vfx_dead, deadSlot?.vfxObj, vfxDeadBase, deadAnchor);
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
    private void ApplyOutfitSlot(SkinSlotData slot)
    {
        if (outfitRenderer == null) return;

        if (slot != null && slot.mesh != null)
        {
            outfitRenderer.enabled = true;
            outfitRenderer.sharedMesh = slot.mesh;
            outfitRenderer.material = slot.material;
        }
        else
        {
            outfitRenderer.sharedMesh = null;
            outfitRenderer.material = null;
            outfitRenderer.enabled = false;
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

    private void ClearVisual(ref GameObject current, Transform anchor)
    {
        // 1. Destroy and reference nullify current tracking variable
        if (current != null)
        {
            Destroy(current);
            current = null;
        }

        // 2. Safe cleanup for remaining children on the anchor
        if (anchor != null)
        {
            for (int i = anchor.childCount - 1; i >= 0; i--)
            {
                Transform child = anchor.GetChild(i);
                if (child != null)
                {
                    Destroy(child.gameObject);
                }
            }
        }
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