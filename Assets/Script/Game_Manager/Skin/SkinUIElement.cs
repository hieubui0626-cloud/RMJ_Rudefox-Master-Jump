using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkinUIElement : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    public Button actionButton;
    public TextMeshProUGUI buttonText;
    public GameObject TokenIcon;
    private SkinInventory inventory => SkinInventory.Instance;

    private SkinData skinData;
    private Sprite fallbackCreatedSprite;

    public void Awake()
    {
        // removed static Instance: there are many UI elements, don't use a single shared Instance
    }
    public void Setup(SkinData data)
    {
        skinData = data;
        nameText.text = data.displayName;
        costText.text = $"{data.cost}";
        // Set icon: ưu tiên dùng Sprite từ SkinData (Inspector)
        if (data.icon != null)
        {
            iconImage.sprite = data.icon;
        }

        RefreshState();
    }



    public void RefreshState()
    {
        if (skinData == null) return;
        if (inventory == null)
        {
            Debug.LogWarning("SkinUIElement.RefreshState: SkinInventory.Instance is null");
            return;
        }

        bool unlocked = inventory.IsOwned(skinData.skinID);
        bool isEquipped = SkinManager.Instance != null && SkinManager.Instance.Get(skinData.type) == skinData.skinID;

        if (TokenIcon != null)
            TokenIcon.SetActive(!unlocked);

        actionButton.onClick.RemoveAllListeners();
        actionButton.interactable = true;

        if (!unlocked)
        {
            buttonText.text = "Unlock";
            actionButton.onClick.AddListener(() => Unlock());
        }
        else if (!isEquipped)
        {
            buttonText.text = "Equip";
            actionButton.onClick.AddListener(() => Equip());
        }
        else
        {
            buttonText.text = "Unequip";
            actionButton.onClick.AddListener(() => Unequip());
        }
    }


    private void Unlock()
    {
        actionButton.interactable = false; // disable to prevent double clicks

        SkinUnlockService.Instance.Unlock(skinData, success =>
        {
            // Always re-enable so user isn't stuck; UI will immediately refresh on success
            actionButton.interactable = true;

            if (success)
            {
                // repopulate so states reflect new ownership
                if (SkinShopUI.Instance != null)
                    SkinShopUI.Instance.PopulateShopByType(skinData.type);
            }
            else
            {
                // On failure keep UI consistent and refresh state (shows tokens/give feedback)
                RefreshState();
            }
        });
    }

    private void Equip()
    {
        SkinManager.Instance.Equip(skinData);
        RefreshState();
    }

    private void Unequip()
    {
        SkinManager.Instance.Unequip(skinData);
        RefreshState();
    }
}