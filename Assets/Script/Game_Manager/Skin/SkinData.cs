using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public enum SkinType
{
    Outfit, Head, Back, Trail, Hit, Dead
}

[System.Serializable]
public class SkinSlotData
{
    [Header("General Meta Info (Cho riêng slot này)")]
    public string skinID;        // Ví dụ: "head_dragon_fire"
    public SkinType type;
    public string displayName;   // Ví dụ: "Nón Rồng Lửa"
    public int cost;              // Giá mua lẻ nón
    public Sprite icon;          // Icon hiển thị trên UI Shop

    [Header("Visual Data")]
    public Mesh mesh;            // Outfit
    public Material material;    // Outfit
    public GameObject prefab;    // Head, Back, Trail
    public GameObject vfxObj;    // Hit, Dead VFX
}

[CreateAssetMenu(fileName = "NewSkinTheme", menuName = "Skin Data/Skin Theme Container")]
public class SkinData : ScriptableObject
{
    [Header("Theme Meta (Tùy chọn)")]
    public string themeName;     // Ví dụ: "Bộ Trang Phục Rồng Lửa"

    [Header("Slots In Theme")]
    public List<SkinSlotData> slots = new List<SkinSlotData>();

    // Helper lấy slot theo Type
    public SkinSlotData GetSlot(SkinType type) => slots.Find(s => s.type == type);

    // Helper tìm slot trong Theme này theo ID mua lẻ
    public SkinSlotData GetSlotByID(string slotID) => slots.Find(s => s.skinID == slotID);
}