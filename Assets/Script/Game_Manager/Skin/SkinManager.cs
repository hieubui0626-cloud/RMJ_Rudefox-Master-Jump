using System.Collections.Generic;
using UnityEngine;

public class SkinManager : MonoBehaviour
{
    public static SkinManager Instance;

    public Dictionary<SkinType, string> equipped = new();
    

    public System.Action OnSkinChanged;
    

    void Awake() => Instance = this;

    public void SetEquipped(Dictionary<SkinType, string> data)
    {
        equipped = data;
    }

    public void Equip(SkinData skin)
    {
        if (!SkinInventory.Instance.IsOwned(skin.skinID))
        {
            Debug.LogWarning("Skin chưa unlock!");
            return;
        }

        equipped[skin.type] = skin.skinID;

        SkinPersistence.Instance.SaveEquipped(equipped);

        OnSkinChanged?.Invoke();
    }

    public void Unequip(SkinData skin)
    {
        if (skin == null) return;

        if (equipped.ContainsKey(skin.type))
            equipped.Remove(skin.type);

        SkinPersistence.Instance.SaveEquipped(equipped);

        OnSkinChanged?.Invoke();
    }
    public string Get(SkinType type)
    {
        return equipped.TryGetValue(type, out var id) ? id : null;
    }
}
