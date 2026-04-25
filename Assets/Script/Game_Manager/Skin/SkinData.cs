using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skin Data")]
public class SkinData : ScriptableObject
{
    public string skinID;
    public SkinType type;
    

    public string displayName;
    public int cost;

    public Sprite icon;

    public Mesh mesh;
    public Material material;

    public GameObject hatPrefab;
    public GameObject backPrefab;
    public GameObject trailPrefab;
}
public enum SkinType
{
    Outfit,
    Head,
    Back,
    Trail
}
