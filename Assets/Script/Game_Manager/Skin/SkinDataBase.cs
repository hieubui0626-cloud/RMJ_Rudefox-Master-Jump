using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class SkinDataBase : MonoBehaviour
{
    public static SkinDataBase Instance;

    public List<SkinData> skins;
    private Dictionary<string, SkinData> lookup;

    void Awake()
    {
        Instance = this;
        lookup = new Dictionary<string, SkinData>();

        foreach (var s in skins)
            lookup[s.skinID] = s;
    }

    public SkinData Get(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return lookup.TryGetValue(id, out var s) ? s : null;
    }
    public List<SkinData> GetSkinsByType(SkinType type)
    {
        if (skins == null) return new List<SkinData>();
        return skins.Where(s => s.type == type).ToList();
    }
}
