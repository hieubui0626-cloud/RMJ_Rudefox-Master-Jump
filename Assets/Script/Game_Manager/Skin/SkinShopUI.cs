using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class SkinShopUI : MonoBehaviour
{
    [Header("References")]
    public GameObject skinUIPrefab;
    public Transform gridParent;
    public Button outfitTabButton;
    public Button hatTabButton;
    public Button backTabButton;
    public Button trailTabButton;

    private List<GameObject> activeItems = new List<GameObject>();

    private void Start()
    {
        outfitTabButton.onClick.AddListener(() => PopulateShopByType(SkinType.Outfit));
        hatTabButton.onClick.AddListener(() => PopulateShopByType(SkinType.Head));
        backTabButton.onClick.AddListener(() => PopulateShopByType(SkinType.Back));
        trailTabButton.onClick.AddListener(() => PopulateShopByType(SkinType.Trail));

        PopulateShopByType(SkinType.Outfit);
    }

    public void PopulateShopByType(SkinType type)
    {
        foreach (var item in activeItems)
            Destroy(item);
        activeItems.Clear();

        var skins = SkinManager.Instance.allSkins.Where(s => s.type == type);
        foreach (var skin in skins)
        {
            GameObject item = Instantiate(skinUIPrefab, gridParent);
            item.GetComponent<SkinUIElement>().Setup(skin);
            activeItems.Add(item);
        }
    }
}
