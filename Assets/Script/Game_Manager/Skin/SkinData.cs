using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[CreateAssetMenu(menuName = "Skin Data")]
public class SkinData : ScriptableObject
{
    public string skinID;
    public SkinType type;
    

    public string displayName;
    public int cost;

    public Sprite icon;

    [Header("Outfit Data")]
    public Mesh mesh;
    public Material material;

    [Header("VFX_Hit_Data")]
    public VisualEffectAsset hitEffect;
    public GameObject hitEffect_Obj;

    [Header("VFX_Dead_Data")]
    public VisualEffectAsset deadEffect;
    public GameObject deadEffect_Obj;

    public GameObject hatPrefab;
    public GameObject backPrefab;
    public GameObject trailPrefab;
}
public enum SkinType
{
    Outfit,
    Head,
    Back,
    Trail,
    Hit,
    Dead,
}
