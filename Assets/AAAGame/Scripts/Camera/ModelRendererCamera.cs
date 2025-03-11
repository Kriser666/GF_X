using UnityEngine;

public class ModelRendererCamera : MonoBehaviour
{
    [SerializeField]
    private GameObject carModel;
    public GameObject CarModel { get { return carModel; } set { carModel = value; } }
    private Vector3 cameraDistance;

    private void Start()
    {
        // 10为CameraViewTable表中关于视角的ID
        var dataRow = GF.DataTable.GetDataTable<CameraViewTable>().GetDataRow(10);
        cameraDistance = dataRow.Offset;
        GetComponent<Camera>().fieldOfView = dataRow.FOV;
    }

    private void Update()
    {
        if (carModel != null)
        {
            transform.position = carModel.transform.position + cameraDistance;
        }
    }

}

