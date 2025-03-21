using GameFramework.DataTable;
using GameFramework.Event;
using GameFramework.Fsm;
using GameFramework.Procedure;
using GameFramework.Resource;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityGameFramework.Runtime;
public class MenuProcedure : ProcedureBase
{
    public int menuUIFormId;
    int carEntityId;
    private CarEntity carEntity;
    LoadAssetCallbacks assetCallbacks;
    IDataTable<VehicleInfoTable> vehicleInfoTable;
    List<Sprite> carSprites;
    List<Sprite> carLogoSprites;
    IDataTable<VehiclePartTable> partInfoTable;
    List<Sprite> partSprites;
    IFsm<IProcedureManager> procedure;
    int loadingObjCount;
    int totalObjCount;
    float progress;
    private GameObject RawImageGo;
    private GameObject gameBackGround = null;
    public List<Sprite> CarSprites { get { return carSprites; } }
    public List<Sprite> CarLogoSprites { get { return carLogoSprites; } }
    public List<Sprite> PartSprites { get { return partSprites; } }
    private string urlMsg;
    public string UrlMsg { get { return urlMsg; } }
    // 底座的实体ID
    private int pedestalEntityId;
    protected override void OnInit(IFsm<IProcedureManager> procedureOwner)
    {
        base.OnInit(procedureOwner);
    }
    protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
    {
        base.OnEnter(procedureOwner);
        procedure = procedureOwner;
#if UNITY_WEBGL
        urlMsg = procedure.GetData<VarString>(Const.URL_PARAMS);
#elif UNITY_EDITOR || UNITY_EDITOR_64 || UNITY_EDITOR_WIN
        urlMsg = "?simulate_param=editor_test"; // 编辑器模拟
#else
        UrlMsg = "";
#endif
        progress = 0f;
        loadingObjCount = 0;
        totalObjCount = 0;
        carEntityId = -1;
        pedestalEntityId = -2;
        GF.BuiltinView.ShowLoadingProgress(progress);
        vehicleInfoTable = GF.DataTable.GetDataTable<VehicleInfoTable>();
        carSprites = new(vehicleInfoTable.Count);
        carLogoSprites = new(vehicleInfoTable.Count);
        for (int i = 0; i < vehicleInfoTable.Count; ++i)
        {
            carSprites.Add(null);
            carLogoSprites.Add(null);
        }

        partInfoTable = GF.DataTable.GetDataTable<VehiclePartTable>();
        partSprites = new(partInfoTable.Count);
        for (int i = 0; i < partInfoTable.Count; ++i)
        {
            partSprites.Add(null);
        }
        ShowMenu();// 加载菜单
        ShowPedestal(); // 加载底座
        assetCallbacks = new(LoadAssetSucceed, LoadAssetFailed);
        ++loadingObjCount;
        totalObjCount = 5;
        LoadCarImages();
        LoadPartImages();

        //var res = await GF.WebRequest.AddWebRequestAsync("https://blog.csdn.net/final5788");
        //Log.Info(Utility.Converter.GetString(res.Bytes));
        GF.Event.Subscribe(OpenUIFormSuccessEventArgs.EventId, UIFormOpenedSuccess);
        GF.Event.Subscribe(OpenUIFormFailureEventArgs.EventId, UIFormOpenedFailed);
        GF.Event.Subscribe(ShowEntitySuccessEventArgs.EventId, EntityOpenedSuccess);
        GF.Event.Subscribe(ShowEntityFailureEventArgs.EventId, EntityOpenedFailed);
    }

    private void CheckAssetsAllLoaded()
    {
        if (loadingObjCount == 0)
        {
            GF.BuiltinView.HideLoadingProgress();
        }
    }

    private void LoadAssetFailed(string assetName, LoadResourceStatus status, string errorMessage, object userData)
    {
        Log.Error($"{GetType()}: 加载资源 {assetName} 失败，错误信息：{errorMessage}");
    }

    private void LoadAssetSucceed(string assetName, object asset, float duration, object userData)
    {
        Log.Debug($"{GetType()}: 加载资源{assetName}成功");
        if (asset is RenderTexture rt)
        {
            // 验证RenderTexture参数
            Debug.Log($"RenderTexture Dimension: {rt.dimension}");

            var ModelRendererCam = CameraController.Instance.ModelRendererCamera;

            // 创建新的RenderTexture（临时解决方案）
            var safeRT = new RenderTexture(rt.width, rt.height, rt.depth);
            ModelRendererCam.targetTexture = safeRT;

            // 修正Culling Mask赋值
            ModelRendererCam.cullingMask = 1 << carEntity.gameObject.layer;
            ModelRendererCam.depth = 9;

            // CameraController.Instance.SetFollowTarget(carEntity.CachedTransform);
            // CameraController.Instance.SetCameraView(10, false);

            progress += (totalObjCount - loadingObjCount) / (float)totalObjCount;
            GF.BuiltinView.SetLoadingProgress(progress);
        }
        else if (asset is GameObject go) // 加载的背景预制体
        {
            GameObject gameObject = Object.Instantiate(go);
            gameObject.name = "GameBackGround";
            gameBackGround = gameObject;
        }
        else if (asset is Texture2D texture) // 背景的贴图
        {
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            if (sprite != null)
            {

                if (userData is string t)
                {
                    LevelBackGround levelBackGround = gameBackGround.GetComponent<LevelBackGround>();
                    if (t == "BackGroundSprite")
                    {
                        levelBackGround.ChangeBackGroundSprite(sprite);
                    }
                }
                else if (userData is (int, string))
                {
                    // ID对应的图的类型
                    (int, string) spriteType = ((int, string))userData;

                    // 汽车缩略图
                    if (spriteType.Item2 == "Car")
                    {
                        carSprites[spriteType.Item1] = sprite;
                    }
                    else if (spriteType.Item2 == "CarLogo")
                    {
                        carLogoSprites[spriteType.Item1] = sprite;
                    }
                    // 部件缩略图
                    else if (spriteType.Item2 == "Part")
                    {
                        partSprites[spriteType.Item1] = sprite;
                    }
                }

            }
        }
        else if (asset is Sprite sprite) // 背景的贴图
        {
            if (sprite != null)
            {

                if (userData is string t)
                {
                    LevelBackGround levelBackGround = gameBackGround.GetComponent<LevelBackGround>();
                    if (t == "BackGroundSprite")
                    {
                        levelBackGround.ChangeBackGroundSprite(sprite);
                    }
                }
                else if (userData is (int, string))
                {
                    // ID对应的图的类型
                    (int, string) spriteType = ((int, string))userData;

                    // 汽车缩略图
                    if (spriteType.Item2 == "Car")
                    {
                        carSprites[spriteType.Item1] = sprite;
                    }
                    else if (spriteType.Item2 == "CarLogo")
                    {
                        carLogoSprites[spriteType.Item1] = sprite;
                    }
                    // 部件缩略图
                    else if (spriteType.Item2 == "Part")
                    {
                        partSprites[spriteType.Item1] = sprite;
                    }
                }

            }
        }
        --loadingObjCount;
        CheckAssetsAllLoaded();
    }

    private void EntityOpenedFailed(object sender, GameEventArgs e)
    {
        var eventArgs = e as ShowEntityFailureEventArgs;
        if (eventArgs.EntityId == carEntityId)
        {
            Log.Error($"{GetType()}: UIFormOpenedFailed=> 错误信息: {eventArgs.ErrorMessage}");
        }
    }

    private void EntityOpenedSuccess(object sender, GameEventArgs e)
    {
        var eventArgs = e as ShowEntitySuccessEventArgs;
        if (eventArgs.Entity.Id == carEntityId)
        {
            carEntity = eventArgs.Entity.Logic as CarEntity;
            // 回调里面设置汽车模型相机的目标
            // CameraController.Instance.SetModelRendererCameraTarget(carEntity.gameObject);
            CameraController.Instance.SetFollowTarget(carEntity.transform);
            int modifyId = carEntity.Params.Get<VarInt32>(Const.MODIFY_ID);
            // 不想改装
            if (modifyId == -1)
            {
                progress += (totalObjCount - loadingObjCount) / (float)totalObjCount;
                GF.BuiltinView.SetLoadingProgress(progress);
            }
            else
            {
                var carData = GF.DataModel.GetDataModel<CarDataModel>();
                foreach (var item in carData.CarWithModifyIdWithModifyParams[carEntity.CurCarId][modifyId])
                {
                    foreach (var partId in item.partsIds)
                    {
                        var prefebName = partInfoTable.GetDataRow(partId).PrefebName;
                        ChangeCarPart(item.whichType, prefebName);
                    }
                }
            }

            GF.Resource.LoadAsset(UtilityBuiltin.AssetsPath.GetTexturePath("RenderTextures/CarRendTex.renderTexture"), assetCallbacks);
        }
        --loadingObjCount;
        CheckAssetsAllLoaded();

    }

    private void UIFormOpenedFailed(object sender, GameEventArgs e)
    {
        var eventArgs = e as OpenUIFormFailureEventArgs;
        if (eventArgs.SerialId == menuUIFormId)
        {
            Log.Error($"{GetType()}: UIFormOpenedFailed=> MainMenu UI打开错误信息: {eventArgs.ErrorMessage}");
        }
    }

    private void UIFormOpenedSuccess(object sender, GameEventArgs e)
    {
        var eventArgs = e as OpenUIFormSuccessEventArgs;
        if (eventArgs.UIForm.SerialId == menuUIFormId)
        {
            MainMenu mainMenu = eventArgs.UIForm.Logic as MainMenu;
            RawImageGo = mainMenu.RawImageGo;
            ShowCar(RawImageGo, vehicleInfoTable.ElementAt(0).Id); // 加载汽车

            // 关卡背景
            GF.Resource.LoadAsset(UtilityBuiltin.AssetsPath.GetPrefab("BackGroundGame/GameBackGround"), assetCallbacks);
            ++loadingObjCount;
            ++totalObjCount;
        }
        progress += (totalObjCount - loadingObjCount) / (float)totalObjCount;
        GF.BuiltinView.SetLoadingProgress(progress);
        --loadingObjCount;
        CheckAssetsAllLoaded();
    }

    protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
    {
        if (!isShutdown)
        {
            GF.UI.CloseUIForm(menuUIFormId);
        }
        GF.Event.Unsubscribe(OpenUIFormSuccessEventArgs.EventId, UIFormOpenedSuccess);
        GF.Event.Unsubscribe(OpenUIFormFailureEventArgs.EventId, UIFormOpenedFailed);
        GF.Event.Unsubscribe(ShowEntitySuccessEventArgs.EventId, EntityOpenedSuccess);
        GF.Event.Unsubscribe(ShowEntityFailureEventArgs.EventId, EntityOpenedFailed);
        carSprites = null;
        base.OnLeave(procedureOwner, isShutdown);
    }

    private void ShowCar(GameObject rawImageGo, int i = 0, int modifyId = -1)
    {
        //动态创建关卡，默认读取第一个车
        // 先隐藏第一个车
        if (carEntityId != -1)
        {
            GF.Entity.HideEntity(carEntityId);
        }
        var carRow = vehicleInfoTable.GetDataRow(i);
        Vector3 eulerAngle = new(0f, 230f, 0f);
        var carParams = EntityParams.Create(new(0f, 0f, 3f), eulerAngle, Vector3.one);
        carParams.Set<VarGameObject>(Const.RAW_IMAGE, rawImageGo);
        carParams.Set<VarInt32>(Const.VEHICLE_ID, i);
        carParams.Set<VarInt32>(Const.MODIFY_ID, modifyId);
        carEntityId = GF.Entity.ShowEntity<CarEntity>(carRow.PrefabName, Const.EntityGroup.Vehicle, carParams);
        ++loadingObjCount;
        ++totalObjCount;
    }

    public void ShowMenu()
    {
        carEntity = null;
        if (GF.Base.IsGamePaused)
        {
            GF.Base.ResumeGame();
        }
        GF.UI.CloseAllLoadingUIForms();
        GF.UI.CloseAllLoadedUIForms();
        GF.Entity.HideAllLoadingEntities();
        GF.Entity.HideAllLoadedEntities();

        //异步打开主菜单UI
        menuUIFormId = GF.UI.OpenUIForm(UIViews.MainMenu);
        ++loadingObjCount;
        ++totalObjCount;
    }
    private void LoadCarImages()
    {
        for (int i = 0; i < vehicleInfoTable.Count; ++i)
        {
            var userData = (i, "Car");
            GF.Resource.LoadAsset(UtilityBuiltin.AssetsPath.GetSpritesPath(vehicleInfoTable[i].CarImageAssetName), assetCallbacks, userData);
            ++loadingObjCount;
            ++totalObjCount;
            var userData2 = (i, "CarLogo");
            GF.Resource.LoadAsset(UtilityBuiltin.AssetsPath.GetSpritesPath(vehicleInfoTable[i].CarLogo), assetCallbacks, userData2);
            ++loadingObjCount;
            ++totalObjCount;
        }
    }
    private void LoadPartImages()
    {
        for (int i = 0; i < partInfoTable.Count; ++i)
        {
            var userData = (i, "Part");
            GF.Resource.LoadAsset(UtilityBuiltin.AssetsPath.GetSpritesPath(partInfoTable[i].PartImage), assetCallbacks, userData);
            ++loadingObjCount;
            ++totalObjCount;
        }
    }
    public void ChangeVehiclePrefeb(int i)
    {
        var ui = GF.UI.GetUIForm(menuUIFormId).Logic as MainMenu;
        ShowCar(ui.RawImageGo, i);
    }
    public void ShowCarPrefebWithModParts(int vehicleId, int modifyId)
    {
        // 从存档读取改装数据
        // 先显示当前车辆
        ShowCar(RawImageGo, vehicleId, modifyId);
        // 再根据存档的modifyId进行改装组件替换，去回调函数里面写
    }
    /// <summary>
    /// 替换部件
    /// </summary>
    /// <param name="vehiclePartTypeEnum">要替换的部件种类</param>
    /// <param name="newPrefabName">部件预制体名</param>
    /// <param name="partIdx">要替换的当前种类部件的索引，不传代表全部替换</param>
    public void ChangeCarPart(VehiclePartTypeEnum vehiclePartTypeEnum, string newPrefabName, int partIdx = -1)
    {
        var car = GF.Entity.GetEntity(carEntityId).Logic as CarEntity;
        car.ReplaceCom(vehiclePartTypeEnum, newPrefabName, partIdx);
    }

    public void ResetPart(VehiclePartTypeEnum vehiclePartTypeEnum, int partIdx = -1)
    {
        var car = GF.Entity.GetEntity(carEntityId).Logic as CarEntity;
        car.ResetPart(vehiclePartTypeEnum, partIdx);
    }

    public bool CarHasBeenModified()
    {
        var car = GF.Entity.GetEntity(carEntityId).Logic as CarEntity;
        return car.Modified();
    }

    public List<int> CurrentPartIdList()
    {
        var car = GF.Entity.GetEntity(carEntityId).Logic as CarEntity;
        return car.CurrentPartIdList();
    }
    public ModifiedParts CurrentPartIdList(VehiclePartTypeEnum partTypeEnum)
    {
        var car = GF.Entity.GetEntity(carEntityId).Logic as CarEntity;
        return car.CurrentPartIdList(partTypeEnum);
    }

    public List<ModifyParams> OriginalModifyParams()
    {
        var car = GF.Entity.GetEntity(carEntityId).Logic as CarEntity;
        return car.OriginalTypeWithParts;
    }

    public List<ModifyParams> CurrentModifyParams()
    {
        var car = GF.Entity.GetEntity(carEntityId).Logic as CarEntity;
        return car.CurTypeWithParts;
    }
    public void SaveModify()
    {
        var car = GF.Entity.GetEntity(carEntityId).Logic as CarEntity;
        car.SaveModify();
        GF.Event.Fire(this, CarItemSelectedEventArgs.Create(CarUIItemSelectedDataType.Saved, carEntityId));
    }

    public void ChangeBackGroundSprite(string userData, string spritePath = null)
    {
        if (userData != "")
        {
            GF.Resource.LoadAsset(UtilityBuiltin.AssetsPath.GetSpritesPath(spritePath), assetCallbacks, userData);
            ++loadingObjCount;
            ++totalObjCount;
        }
        else
        {
            if (gameBackGround != null)
            {
                LevelBackGround backGround = gameBackGround.GetComponent<LevelBackGround>();
                backGround.ResetBackGround();
            }
        }
    }

    public void ResetAllParts()
    {
        if (CarHasBeenModified())
        {
            for (int i = 0; i < (int)VehiclePartTypeEnum.Count; ++i)
            {
                VehiclePartTypeEnum curLoopTypeEnum = (VehiclePartTypeEnum)i;
                ResetPart(curLoopTypeEnum);
            }
        }
    }
    /// <summary>
    /// 显示底座
    /// </summary>
    public void ShowPedestal()
    {
        if (!GF.Entity.HasEntity(pedestalEntityId))
        {
            var pedestalParams = EntityParams.Create(new(0f, -2.2f, 10f), new(0f, 90f, 0f), new(2f, 2f, 2f));
            pedestalEntityId = GF.Entity.ShowEntity<GarageEntity>(GarageEntity.PREFAB_NAME, Const.EntityGroup.Vehicle, pedestalParams);
            ++loadingObjCount;
            ++totalObjCount;
        }
    }
}
