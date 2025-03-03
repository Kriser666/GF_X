using GameFramework.Resource;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

public class ModifyParams
{
    public VehiclePartTypeEnum whichType; // 改装的部件类型
    public List<GameObject> parts; // 该类型下的部件们
}

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
    [SerializeField]
    private List<ModifyParams> originalTypeWithParts; // 默认的哪种类型的部件的索引
    public List<ModifyParams> CurTypeWithParts; // 当前的哪种类型车体组件

    private Quaternion m_TargetRotation;

    private LoadAssetCallbacks assetCallbacks;
    private Camera modelViewCamera; // 指向渲染RenderTexture的专用摄像机
    private GameObject rawImage;
    public GameObject RawImage { get { return rawImage; } }
    [SerializeField]
    private GameObject originalPartList; // 替换掉原始部件后暂存的父节点
    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        m_TargetRotation = transform.rotation;
        if (carModel == null)
            carModel = transform.GetChild(0).gameObject;
        if (originalPartList == null)
            originalPartList = transform.GetChild(1).gameObject;
        assetCallbacks = new(LoadAssetSucceedCallBack, LoadAssetFailedCallback);
    }

    private void LoadAssetFailedCallback(string assetName, LoadResourceStatus status, string errorMessage, object userData)
    {
        Log.Debug("资源加载失败回调：" + assetName + "错误信息：" + errorMessage);
    }

    private void LoadAssetSucceedCallBack(string assetName, object asset, float duration, object userData)
    {
        Log.Debug("资源加载成功回调：" + assetName);
        ModifyParams modifyParams = (ModifyParams)userData;
        if (CurTypeWithParts != null && CurTypeWithParts.Count > 0)
        {
            
            var oldTypeWithPart = CurTypeWithParts.Find((typeWithParts) => { return typeWithParts.whichType == modifyParams.whichType; });
            GameObject newGO = asset as GameObject;
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

            var oldParts = oldTypeWithPart.parts;
            for (int i = 0; i < oldParts.Count; ++i)
            {
                if (modifyParams.parts[i] != null)
                {
                    var oldPart = oldParts[i];
                    oldPart.SetActive(false);
                    GameObject ChangeObj = Instantiate(newGO);
                    ChangeObj.transform.SetParent(carModel.transform);
                    ChangeObj.transform.SetPositionAndRotation(oldPart.transform.position, oldPart.transform.rotation);
                    ChangeObj.name = oldPart.name;
                    ChangeObj.tag = oldPart.tag;
                    ChangeObj.transform.SetSiblingIndex(oldPart.transform.GetSiblingIndex());
                    oldParts[i] = ChangeObj;
                    oldPart.transform.SetParent(originalPartList.transform);
                }
            }
            
            
        }
        
    }

    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
        CurTypeWithParts ??= new(1);
        originalTypeWithParts ??= new(1);
        if (originalTypeWithParts.Count == 0)
        {
            for (int i = 0; i < (int)VehiclePartTypeEnum.Count; ++i)
            {
                // 构造一份默认的配件
                ModifyParams originalModifyParams = new()
                {
                    whichType = (VehiclePartTypeEnum)i,
                    parts = new(GameObject.FindGameObjectsWithTag(Enum.GetName(typeof(VehiclePartTypeEnum), i)))
                };
                originalTypeWithParts.Add(originalModifyParams);
            }
        }
        if (CurTypeWithParts.Count == 0)
        {
            for (int i = 0; i < (int)VehiclePartTypeEnum.Count; ++i)
            {
                ModifyParams modifyParams = new()
                {
                    whichType = (VehiclePartTypeEnum)i,
                    parts = new (GameObject.FindGameObjectsWithTag(Enum.GetName(typeof(VehiclePartTypeEnum), i)))
                };
                CurTypeWithParts.Add(modifyParams);
            }
        }
        modelViewCamera = CameraController.Instance.ModelRendererCamera;
        if (Params.TryGet<VarGameObject>(Const.RAW_IMAGE, out var rawImageObj))
        {
            rawImage = rawImageObj;
        }

        // 旋转车辆
        transform.SetLocalPositionAndRotation(transform.position, Quaternion.identity);
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

    }
    protected override void OnHide(bool isShutdown, object userData)
    {

        base.OnHide(isShutdown, userData);
    }
    /// <summary>
    /// 替换部件
    /// </summary>
    /// <param name="vehiclePartTypeEnum">要替换的部件种类</param>
    /// <param name="newPrefabName">部件预制体名</param>
    /// <param name="partIdx">要替换的当前种类部件的索引，不传代表全部替换</param>
    public void ReplaceCom(VehiclePartTypeEnum vehiclePartTypeEnum, string newPrefabName, int partIdx = -1)
    {
        // 旧的部件的信息
        var filteredTypeWithParts = CurTypeWithParts.Find((typeWithParts) => { return vehiclePartTypeEnum == typeWithParts.whichType; });
        if (filteredTypeWithParts != null)
        {
            // 加载新部件
            string prefebFullPath = UtilityBuiltin.AssetsPath.GetPrefab(newPrefabName);
            ModifyParams modifyParams = new()
            {
                whichType = vehiclePartTypeEnum
            };
            List<GameObject> wantReplacedParts;
            wantReplacedParts = new(filteredTypeWithParts.parts);
            if (partIdx != -1)
            {
                for (int i = 0; i < filteredTypeWithParts.parts.Count; ++i)
                {
                    if (i != partIdx)
                    {
                        wantReplacedParts[i] = null;
                    }
                }
            }
            modifyParams.parts = wantReplacedParts;
            GF.Resource.LoadAsset(prefebFullPath, assetCallbacks, modifyParams);
        }
        else
        {
            Log.Debug($"{GetType()}: /ReplaceCom=> 未找到相关的部件！");
        }
    }
    /// <summary>
    /// 重置部件
    /// </summary>
    /// <param name="vehiclePartTypeEnum">车辆哪个类型的部件</param>
    /// <param name="partIdx">该类型部件下的部件索引</param>
    public void ResetPart(VehiclePartTypeEnum vehiclePartTypeEnum, int partIdx = -1)
    {
        var curParts = CurTypeWithParts.Find((typeWithParts) => { return typeWithParts.whichType == vehiclePartTypeEnum; }).parts;
        var originalParts = originalTypeWithParts.Find((typeWithParts) => { return typeWithParts.whichType == vehiclePartTypeEnum; }).parts;
        if (partIdx == -1)
        {
            for (int i = 0; i < curParts.Count; i++)
            {
                var curPart = curParts[i];
                curPart.SetActive(false);
                GameObject originalObj;
                if (originalParts[i] == null)
                {
                    originalObj = Instantiate(originalParts[i]);
                }
                else
                {
                    originalObj = originalParts[i];
                }
                originalObj.transform.SetParent(carModel.transform);
                originalObj.transform.SetPositionAndRotation(curPart.transform.position, curPart.transform.rotation);
                originalObj.name = curPart.name;
                originalObj.transform.SetSiblingIndex(curPart.transform.GetSiblingIndex());
                originalObj.SetActive(true);
                curParts[i] = originalObj;
                Destroy(curPart);
            }
        }
        else
        {
            var curPart = curParts[partIdx];
            curPart.SetActive(false);
            GameObject originalObj = Instantiate(originalParts[partIdx]);
            originalObj.transform.SetParent(carModel.transform);
            originalObj.transform.SetPositionAndRotation(curPart.transform.position, curPart.transform.rotation);
            originalObj.name = curPart.name;
            originalObj.tag = curPart.tag;
            originalObj.transform.SetSiblingIndex(curPart.transform.GetSiblingIndex());
            curParts[partIdx] = originalObj;
            Destroy(curPart);
        }
    }

}

