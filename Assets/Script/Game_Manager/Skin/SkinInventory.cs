using UnityEngine;
using System.Collections.Generic;
public class SkinInventory : MonoBehaviour
{
    public static SkinInventory Instance;

    private HashSet<string> owned = new();
    // removed unused fields unlockedSkins and equippedSkins

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple SkinInventory instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool IsOwned(string id) => owned.Contains(id);

    public void Set(List<string> ids)
    {
        owned = new HashSet<string>(ids);
    }

    public void Add(string id)
    {
        if (owned.Add(id))
        {
            if (SkinPersistence.Instance != null)
                SkinPersistence.Instance.SaveInventory(owned);
            else
                Debug.LogWarning("SkinPersistence.Instance is null — cannot save inventory");
        }
    }

    public List<string> GetAll() => new List<string>(owned);
}