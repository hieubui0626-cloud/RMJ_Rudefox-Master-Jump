using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class SkinBootstrapper : MonoBehaviour
{
    // Optional: open shop automatically for testing
    public bool openShopOnStart = false;
    public SkinType openShopType = SkinType.Outfit;

    void Start()
    {
        if (SkinPersistence.Instance == null)
        {
            Debug.LogWarning("SkinPersistence.Instance is null. Ensure SkinPersistence is in the scene.");
            return;
        }

        // Load unlocked and equipped from Firebase / PlayerPrefs (SkinPersistence handles fallback)
        SkinPersistence.Instance.LoadAll((unlocked, equipped) =>
        {
            // Apply unlocked
            if (SkinInventory.Instance != null)
                SkinInventory.Instance.Set(unlocked ?? new List<string>());

            // Apply equipped
            if (SkinManager.Instance != null)
                SkinManager.Instance.SetEquipped(equipped ?? new Dictionary<SkinType, string>());

            // Apply to player visuals now that managers are populated
            if (PlayerSkinApplier.Instance != null)
                PlayerSkinApplier.Instance.ApplyAll();

            // Populate shop UI so each element reflects current state
            if (SkinShopUI.Instance != null)
            {
                if (openShopOnStart)
                    SkinShopUI.Instance.PopulateShopByType(openShopType);
                else
                    SkinShopUI.Instance.PopulateShopByType(SkinType.Outfit);
            }
        });
    }
}
