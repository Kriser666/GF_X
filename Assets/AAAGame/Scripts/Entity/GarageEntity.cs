using UnityEngine;

public class GarageEntity : EntityBase
{
    public static readonly string PREFAB_NAME = "Garage";
    /*[SerializeField]
    [Header("网格体")]
    private Renderer meshRenderer;

    private Vector3 initialScale;

    protected override void OnShow(object userData)
    {
        base.OnShow(userData);

        if (meshRenderer == null)
        {
            meshRenderer = transform.GetChild(0).GetComponent<Renderer>();
        }
        initialScale = new (0.5f, 0.5f, 0.5f);

        Vector3 currentScale = transform.GetChild(0).localScale;
        Vector3 tiling = new(
            currentScale.x / initialScale.x,
            currentScale.y / initialScale.y,
            currentScale.z / initialScale.z
        );
        meshRenderer.material.mainTextureScale = tiling;
    }*/
}
