using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class SkinShopUI : MonoBehaviour
{
    public static SkinShopUI Instance;
    [Header("References")]
    public GameObject skinUIPrefab;
    public Transform gridParent;
    public Button outfitTabButton;
    public Button hatTabButton;
    public Button backTabButton;
    public Button trailTabButton;
    public SkinDataBase database;

    private List<GameObject> activeItems = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        outfitTabButton.onClick.AddListener(() => PopulateShopByType(SkinType.Outfit));
        hatTabButton.onClick.AddListener(() => PopulateShopByType(SkinType.Head));
        backTabButton.onClick.AddListener(() => PopulateShopByType(SkinType.Back));
        trailTabButton.onClick.AddListener(() => PopulateShopByType(SkinType.Trail));

        if (SkinManager.Instance != null)
            SkinManager.Instance.OnSkinChanged += RefreshAllItems;

        PopulateShopByType(SkinType.Outfit);
    }

    private void OnDestroy()
    {
        if (SkinManager.Instance != null)
            SkinManager.Instance.OnSkinChanged -= RefreshAllItems;
    }

    private void RefreshAllItems()
    {
        foreach (var item in activeItems)
        {
            if (item != null)
                item.GetComponent<SkinUIElement>().RefreshState();
        }
    }

    public void PopulateShopByType(SkinType type)
    {
        foreach (var item in activeItems)
            Destroy(item);
        activeItems.Clear();

        foreach (var theme in database.allSkins)
        {
            var slot = theme?.GetSlot(type);
            if (slot == null) continue;

            GameObject item = Instantiate(skinUIPrefab, gridParent);
            item.GetComponent<SkinUIElement>().Setup(slot);
            activeItems.Add(item);
        }
    }

}
