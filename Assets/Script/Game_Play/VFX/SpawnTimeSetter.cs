using UnityEngine;

public class SpawnTimeSetter : MonoBehaviour
{
    Renderer rend;

    void Awake()
    {
        rend = GetComponent<SpriteRenderer>();

        MaterialPropertyBlock block = new MaterialPropertyBlock();
        rend.GetPropertyBlock(block);

        block.SetFloat("_SpawnTime", Time.time);

        rend.SetPropertyBlock(block);
    }
}