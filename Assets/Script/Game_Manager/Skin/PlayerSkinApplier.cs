using UnityEngine;

public class PlayerSkinApplier : MonoBehaviour
{
    public static PlayerSkinApplier Instance;

    public SkinnedMeshRenderer outfitRenderer;

    public Transform hatAnchor;
    public Transform backAnchor;
    public Transform trailAnchor;

    GameObject head, back, trail;

    void Awake() => Instance = this;

    void Start()
    {
        SkinManager.Instance.OnSkinChanged += ApplyAll;
        ApplyAll();
    }

    public void ApplyAll()
    {
        var db = SkinDataBase.Instance;

        ApplyOutfit(db.Get(SkinManager.Instance.Get(SkinType.Outfit)));
        ApplyPrefab(ref head, db.Get(SkinManager.Instance.Get(SkinType.Head))?.hatPrefab, hatAnchor);
        ApplyPrefab(ref back, db.Get(SkinManager.Instance.Get(SkinType.Back))?.backPrefab, backAnchor);
        ApplyPrefab(ref trail, db.Get(SkinManager.Instance.Get(SkinType.Trail))?.trailPrefab, trailAnchor);
    }

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

    void ApplyPrefab(ref GameObject current, GameObject prefab, Transform anchor)
    {
        if (current != null) Destroy(current);
        if (prefab != null) current = Instantiate(prefab, anchor);
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
            case SkinType.Trail:
                if (trail != null)
                    trail.SetActive(true);
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
            case SkinType.Trail:
                if (trail != null)
                    trail.SetActive(false);
                break;
        }
    }
}
