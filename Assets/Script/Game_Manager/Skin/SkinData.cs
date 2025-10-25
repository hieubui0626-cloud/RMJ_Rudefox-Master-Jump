using UnityEngine;

[CreateAssetMenu(fileName = "SkinData", menuName = "Scriptable Objects/SkinData")]
public class SkinData : ScriptableObject
{
    public string skinId;
    public string displayName;
    public int cost;
    public SkinType type;
    
    public Sprite icon;

    [Header("Prefab (dành cho Hat, Back, Trail)")]
    public GameObject prefab;

    [Header("Outfit data (dành cho SkinType.Outfit)")]
    public Mesh mesh;
    public Material material;
}

public enum SkinType
{
    Outfit,
    Head,
    Back,
    Trail,
    Hit,
}
