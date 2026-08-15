using System.Collections.Generic;
using UnityEngine;

public class EndlessBiomeBlender : MonoBehaviour
{
    [System.Serializable]
    public class TextureBiome
    {
        public Texture texture;
    }

    [System.Serializable]
    public class ColorBiome
    {
        public Color color = Color.white;
    }

    [Header("Reference")]
    public Transform player;

    [Header("Texture Material")]
    public Renderer textureRenderer;

    [Header("Color Material")]
    public Material colorMaterial_Ground;
    public Material colorMaterial_BG;

    [Header("Biome Settings")]
    [Min(1)]
    public float biomeHeight = 1000f;

    [Min(1)]
    public float blendRange = 200f;

    [Header("Texture Cycle")]
    public List<TextureBiome> textureBiomes = new();

    [Header("Color Cycle")]
    public List<ColorBiome> colorBiomes = new();

    private Material textureMaterial;

    private int lastTextureBiome = -1;
    private int lastColorBiome = -1;

    private void Awake()
    {
        if (textureRenderer != null)
        {
            textureMaterial = textureRenderer.material;
        }
    }

    private void Update()
    {
        if (player == null)
            return;

        UpdateTextureSystem();
        UpdateColorSystem();
    }

    #region TEXTURE

    private void UpdateTextureSystem()
    {
        if (textureMaterial == null)
            return;

        if (textureBiomes.Count < 2)
            return;

        float playerHeight = Mathf.Max(0f, player.position.y);

        float cycleLength =
            textureBiomes.Count * biomeHeight;

        float cycleHeight =
            playerHeight % cycleLength;

        int currentBiome =
            Mathf.FloorToInt(cycleHeight / biomeHeight);

        int nextBiome =
            (currentBiome + 1) % textureBiomes.Count;

        if (currentBiome != lastTextureBiome)
        {
            textureMaterial.SetTexture(
                "_TextureA",
                textureBiomes[currentBiome].texture);

            textureMaterial.SetTexture(
                "_TextureB",
                textureBiomes[nextBiome].texture);

            lastTextureBiome = currentBiome;
        }

        float localHeight =
            cycleHeight % biomeHeight;

        float blend =
            Mathf.InverseLerp(
                biomeHeight - blendRange,
                biomeHeight,
                localHeight);

        textureMaterial.SetFloat("_Blend", blend);
    }

    #endregion

    #region COLOR

    private void UpdateColorSystem()
    {
        if (colorMaterial_Ground == null || colorMaterial_BG == null)
            return;

        if (colorBiomes.Count < 2)
            return;

        float playerHeight = Mathf.Max(0f, player.position.y);

        float cycleLength =
            colorBiomes.Count * biomeHeight;

        float cycleHeight =
            playerHeight % cycleLength;

        int currentBiome =
            Mathf.FloorToInt(cycleHeight / biomeHeight);

        int nextBiome =
            (currentBiome + 1) % colorBiomes.Count;

        if (currentBiome != lastColorBiome)
        {
            colorMaterial_Ground.SetColor(
                "_ColorA",
                colorBiomes[currentBiome].color);

            colorMaterial_Ground.SetColor(
                "_ColorB",
                colorBiomes[nextBiome].color);

            
            colorMaterial_BG.SetColor(
                "_Par_Color_A",
                colorBiomes[currentBiome].color);
            colorMaterial_BG.SetColor(
                "_Par_Color_B",
                colorBiomes[nextBiome].color);

            lastColorBiome = currentBiome;
        }

        float localHeight =
            cycleHeight % biomeHeight;

        float blend =
            Mathf.InverseLerp(
                biomeHeight - blendRange,
                biomeHeight,
                localHeight);

        colorMaterial_Ground.SetFloat("_Blend", blend);
        colorMaterial_BG.SetFloat("_Blend", blend);
    }

    #endregion

#if UNITY_EDITOR
    private void OnValidate()
    {
        blendRange = Mathf.Clamp(
            blendRange,
            1,
            biomeHeight);
    }
#endif
}