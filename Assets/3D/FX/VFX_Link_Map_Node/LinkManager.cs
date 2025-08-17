using UnityEngine;
using UnityEngine.VFX;


[ExecuteAlways]
public class VFXLinkPoints : MonoBehaviour
{
    public VisualEffect vfx;
    public Transform pointA;
    public Transform pointB;

    void Update()
    {
        if (vfx != null)
        {
            vfx.SetVector3("Star_Point", pointA.position);
            vfx.SetVector3("End_Point", pointB.position);
        }
    }
}