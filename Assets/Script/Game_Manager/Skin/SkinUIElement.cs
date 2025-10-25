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

    private SkinData skinData;
    private Sprite fallbackCreatedSprite;

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

        bool unlocked = SkinManager.Instance.IsUnlocked(skinData.skinId);
        var equipped = SkinManager.Instance.GetEquippedSkin(skinData.type);
        bool isEquipped = equipped != null && equipped.skinId == skinData.skinId;


        if (TokenIcon != null)
            TokenIcon.SetActive(!unlocked);
        // reset button listeners + interactable
        actionButton.onClick.RemoveAllListeners();
        actionButton.interactable = true;

        if (!unlocked)
        {
            buttonText.text = "Unlock";
            actionButton.onClick.AddListener(UnlockSkin);
        }
        else if (!isEquipped)
        {
            buttonText.text = "Equip";
            actionButton.onClick.AddListener(EquipSkin);
            
        }
        else
        {
            buttonText.text = "Equipped";
            actionButton.interactable = false;
        }
    }


    private void UnlockSkin()
    {
        actionButton.interactable = false;
        SkinManager.Instance.UnlockSkin(skinData);
        RefreshState();
    }

    private void EquipSkin()
    {
        SkinManager.Instance.EquipSkin(skinData);
        RefreshState();
    }
}
