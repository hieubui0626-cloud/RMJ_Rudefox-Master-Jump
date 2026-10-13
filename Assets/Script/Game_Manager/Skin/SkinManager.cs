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

    public void Equip(SkinSlotData skin)
    {
        if (skin == null) return;
        if (!SkinInventory.Instance.IsOwned(skin.skinID))
        {
            Debug.LogWarning("Skin chưa unlock!");
            return;
        }

        equipped[skin.type] = skin.skinID;

        SkinPersistence.Instance.SaveEquipped(equipped);

        OnSkinChanged?.Invoke();
    }

    public void Unequip(SkinSlotData skin)
    {
        if (skin == null) return;

        if (!equipped.TryGetValue(skin.type, out var currentId) || currentId != skin.skinID)
            return;

        equipped.Remove(skin.type);


        SkinPersistence.Instance.SaveEquipped(equipped);
        PlayerPrefs.DeleteKey($"Equipped_{skin.type}"); // tránh cache local còn giữ skin đã gỡ
        PlayerPrefs.Save();
        OnSkinChanged?.Invoke();
    }
    public string Get(SkinType type)
    {
        return equipped.TryGetValue(type, out var id) ? id : null;
    }
}