using UnityEngine;
using System.Collections.Generic;
public class SkinPersistence : MonoBehaviour
{
    public static SkinPersistence Instance;

    void Awake() => Instance = this;

    public void SaveInventory(HashSet<string> owned)
    {
        FirebaseManager.Instance.SaveUnlockedSkins(new List<string>(owned));

        PlayerPrefs.SetString("UnlockedSkins", string.Join(",", owned));
        PlayerPrefs.Save();
    }

    public void SaveEquipped(Dictionary<SkinType, string> equipped)
    {
        FirebaseManager.Instance.SaveEquippedSkins(equipped);

        foreach (var kvp in equipped)
            PlayerPrefs.SetString($"Equipped_{kvp.Key}", kvp.Value);

        PlayerPrefs.Save();
    }

    public void LoadAll(System.Action<List<string>, Dictionary<SkinType, string>> callback)
    {
        FirebaseManager.Instance.LoadPlayerSkinData((tokens, unlocked, equipped) =>
        {
            // fallback local
            if (unlocked.Count == 0)
            {
                string cache = PlayerPrefs.GetString("UnlockedSkins", "");
                if (!string.IsNullOrEmpty(cache))
                    unlocked = new List<string>(cache.Split(','));
            }

            callback(unlocked, equipped);
        });
    }
}
