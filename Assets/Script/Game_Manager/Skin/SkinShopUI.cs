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

        PopulateShopByType(SkinType.Outfit);
    }

    public void PopulateShopByType(SkinType type)
    {
        foreach (var item in activeItems)
            Destroy(item);
        activeItems.Clear();

        var skins = database.GetSkinsByType(type);

        foreach (var skin in skins)
        {
            GameObject item = Instantiate(skinUIPrefab, gridParent);
            item.GetComponent<SkinUIElement>().Setup(skin);
            activeItems.Add(item);
        }
    }

}
