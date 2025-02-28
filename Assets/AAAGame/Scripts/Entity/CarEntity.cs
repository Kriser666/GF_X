using GameFramework.Resource;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

public class CarEntity : EntityBase
{
    [Tooltip("旋转速度")]
    [SerializeField]
    private float m_RotationSpeed = 10f;
    [Tooltip("是否启用平滑差值")]
    [SerializeField]
    private bool smoothRotation = true; // 启用平滑插值
    [Tooltip("平滑系数")]
    [SerializeField]
    private float smoothFactor = 10f;   // 平滑系数

    private GameObject carModel;
    public List<GameObject> CarComponents; // 车体组件数组

    private Quaternion m_TargetRotation;

    private LoadAssetCallbacks assetCallbacks;
    private Camera modelViewCamera; // 指向渲染RenderTexture的专用摄像机
    public GameObject rawImage;

    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        m_TargetRotation = transform.rotation;
        carModel = transform.GetChild(0).gameObject;
        assetCallbacks = new(LoadAssetSucceedCallBack, LoadAssetFailedCallback);
    }

    private void LoadAssetFailedCallback(string assetName, LoadResourceStatus status, string errorMessage, object userData)
    {
        Log.Debug("资源加载失败回调：" + assetName + "错误信息：" + errorMessage);
    }

    private void LoadAssetSucceedCallBack(string assetName, object asset, float duration, object userData)
    {
        Log.Debug("资源加载成功回调：" + assetName);
        int i = (int)userData;
        if (CarComponents != null && CarComponents.Count > 0)
        {
            GameObject oldGO = CarComponents[i];
            GameObject newGO = asset as GameObject;
            newGO = Instantiate(newGO);
            // 需要复制的组件
            /*MeshFilter oldMeshFilter = oldGO.GetComponent<MeshFilter>();
            MeshRenderer oldMeshRenderer = oldGO.GetComponent<MeshRenderer>();
            MeshFilter newMeshFilter = newGO.GetComponent<MeshFilter>();
            MeshRenderer newMeshRenderer = newGO.GetComponent<MeshRenderer>();
            ComponentTool.CopyComponentValues(newMeshFilter, oldMeshFilter);
            ComponentTool.CopyComponentValues(newMeshRenderer, oldMeshRenderer);

            if (oldGO.TryGetComponent(out MeshCollider oldMeshCollider))
            {
                MeshCollider newMeshCollider = newGO.GetComponent<MeshCollider>();
                ComponentTool.CopyComponentValues(newMeshCollider, oldMeshCollider);
            }
*/
            /*MeshFilter newMeshFilter = newGO.GetComponent<MeshFilter>();
            MeshRenderer newMeshRenderer = newGO.GetComponent<MeshRenderer>();
            ComponentTool.CloneComponentByReflection(newMeshFilter, oldGO, true);
            ComponentTool.CloneComponentByReflection(newMeshRenderer, oldGO, true);
            if (oldGO.TryGetComponent(out MeshCollider _))
            {
                MeshCollider newMeshCollider = newGO.GetComponent<MeshCollider>();
                ComponentTool.CloneComponentByReflection(newMeshCollider, oldGO, true);
            }*/
            oldGO.SetActive(false);
            newGO.transform.SetParent(carModel.transform);
            newGO.transform.SetPositionAndRotation(oldGO.transform.position, oldGO.transform.rotation);
            newGO.name = oldGO.name;
            newGO.transform.SetSiblingIndex(i);
            CarComponents[i] = newGO;
            Destroy(oldGO);
        }
        
    }

    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
        CarComponents ??= new(71);
        if (CarComponents.Count == 0)
        {
            int count = carModel.transform.childCount;
            for (int i = 0; i < count; i++)
            {
                CarComponents.Add(carModel.transform.GetChild(i).gameObject);
            }
        }
        modelViewCamera = CameraController.Instance.ModelRendererCamera;
        if (Params.TryGet<VarGameObject>(Const.RAW_IMAGE, out var rawImageObj))
        {
            rawImage = rawImageObj;
        }

        // 旋转车辆
        Quaternion rotation = Quaternion.Euler(5f, -120f, 0f);
        transform.SetLocalPositionAndRotation(transform.position, rotation);
    }
    protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        // 仅在有效区域且未遮挡时响应
        if (!RawImageZoneChecker.IsPointerInZone) return;
        if (Input.GetMouseButton(0))
        {
            /*RectTransform modelZoneRect = rawImage.GetComponent<RectTransform>();
            // 将鼠标坐标转换为模型区域局部坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                modelZoneRect,
                Input.mousePosition,
                modelViewCamera, // 使用模型摄像机而非主摄像机
                out Vector2 localPos
            );

            // 计算标准化偏移量（-0.5~0.5）
            Vector2 normalizedOffset = new (
                localPos.x / modelZoneRect.rect.width,
                localPos.y / modelZoneRect.rect.height
            );

            // 基于区域比例的灵敏度补偿
            float aspectRatio = modelZoneRect.rect.width / modelZoneRect.rect.height;
            float mouseX = normalizedOffset.x * m_RotationSpeed * aspectRatio;
            float mouseY = normalizedOffset.y * m_RotationSpeed;*/
            float mouseX = Input.GetAxis("Mouse X") * m_RotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * m_RotationSpeed;
            // 使用模型摄像机的坐标系进行旋转
            Vector3 worldUp = modelViewCamera.transform.up;
            Vector3 worldRight = modelViewCamera.transform.right;

            Quaternion yRot = Quaternion.AngleAxis(-mouseX, worldUp);
            Quaternion xRot = Quaternion.AngleAxis(mouseY, worldRight);

            m_TargetRotation = yRot * xRot * m_TargetRotation;
        }

        // 应用旋转
        if (smoothRotation)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                m_TargetRotation,
                Time.deltaTime * smoothFactor
            );
        }
        else
        {
            transform.rotation = m_TargetRotation;
        }

        // 按下v进行替换
        if (Input.GetKeyDown(KeyCode.V))
        {
            Log.Debug("按下了V");
            ReplaceCom("YouHouLunGu");
        }
    }
    protected override void OnHide(bool isShutdown, object userData)
    {

        base.OnHide(isShutdown, userData);
    }
    /// <summary>
    /// 替换部件
    /// </summary>
    /// <param name="prefabName">部件名，预制体部件命名为游戏对象名+后缀</param>
    public void ReplaceCom(string prefabName)
    {
        GameObject oldCom = null;
        // 记录旧的部件的索引
        int i = 0;
        foreach (var com in CarComponents)
        {
            if (com.name.Contains(prefabName))
            {
                oldCom = com;
                break;
            }
            ++i;
        }
        if (oldCom != null)
        {
            // 加载新部件
            string prefebFullPath = UtilityBuiltin.AssetsPath.GetPrefab("Entity/" + prefabName);
            GF.Resource.LoadAsset(prefebFullPath, assetCallbacks, i);
        }
        else
        {
            Log.Debug($"{GetType()}: /ReplaceCom=> 未找到相关的部件！");
        }
    }

}

