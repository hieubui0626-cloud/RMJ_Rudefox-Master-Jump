using System;
using UnityEngine;

public class SkinUnlockService
{
    // Simple singleton used by UI code
    public static SkinUnlockService Instance { get; } = new SkinUnlockService();

    // Prevent external construction
    private SkinUnlockService() { }

    // Accepts an optional completion callback so callers (UI) can refresh after unlock
    // Callback now returns a bool indicating success (true) or failure (false)
    public void Unlock(SkinData skin, Action<bool> onComplete = null)
    {
        if (skin == null)
        {
            Debug.LogWarning("SkinUnlockService.Unlock called with null skin");
            onComplete?.Invoke(false);
            return;
        }

        if (SkinInventory.Instance == null)
        {
            Debug.LogWarning("SkinInventory.Instance is null");
            onComplete?.Invoke(false);
            return;
        }

        if (SkinInventory.Instance.IsOwned(skin.skinID))
        {
            Debug.Log("Đã sở hữu rồi");
            onComplete?.Invoke(true); // already owned => treat as success so UI can update
            return;
        }

        if (FirebaseManager.Instance == null)
        {
            Debug.LogWarning("FirebaseManager.Instance is null");
            onComplete?.Invoke(false);
            return;
        }

        FirebaseManager.Instance.GetTotalTokens(total =>
        {
            if (total < skin.cost)
            {
                Debug.Log("Không đủ token");
                onComplete?.Invoke(false);
                return;
            }

            FirebaseManager.Instance.UpdateTotalTokens(-skin.cost, newTotal =>
            {
                SkinInventory.Instance.Add(skin.skinID);

                Debug.Log($"Unlock thành công {skin.displayName}");
                onComplete?.Invoke(true);
            });
        });
    }
}