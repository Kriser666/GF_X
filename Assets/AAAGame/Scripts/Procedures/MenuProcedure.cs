using GameFramework.DataTable;
using GameFramework.Event;
using GameFramework.Fsm;
using GameFramework.Procedure;
using GameFramework.Resource;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;
public class MenuProcedure : ProcedureBase
{
    int menuUIFormId;
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
    float progress;
    public List<Sprite> CarSprites { get { return carSprites; } }
    public List<Sprite> CarLogoSprites { get { return carLogoSprites; } }
    public List<Sprite> PartSprites { get { return partSprites; } }
    protected override void OnInit(IFsm<IProcedureManager> procedureOwner)
    {
        base.OnInit(procedureOwner);
    }
    protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
    {
        base.OnEnter(procedureOwner);
        procedure = procedureOwner;
        progress = 0f;
        loadingObjCount = 0;
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
        assetCallbacks = new(LoadAssetSucceed, LoadAssetFailed);
        ++loadingObjCount;

        LoadCarImages();
        LoadPartImages();

        //var res = await GF.WebRequest.AddWebRequestAsync("https://blog.csdn.net/final5788");
        //Log.Info(Utility.Converter.GetString(res.Bytes));
        GF.Event.Subscribe(OpenUIFormSuccessEventArgs.EventId, UIFormOpenedSuccess);
        GF.Event.Subscribe(OpenUIFormFailureEventArgs.EventId, UIFormOpenedFailed);
        GF.Event.Subscribe(ShowEntitySuccessEventArgs.EventId, EntityOpenedSuccess);
        GF.Event.Subscribe(ShowEntityFailureEventArgs.EventId, EntityOpenedFailed);
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

            CameraController.Instance.SetFollowTarget(carEntity.CachedTransform);
            CameraController.Instance.SetCameraView(10);

            progress += 0.3f;
            GF.BuiltinView.SetLoadingProgress(progress);
            --loadingObjCount;
        }
        else if (asset is Texture2D texture)
        {
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            if (sprite != null)
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
                --loadingObjCount;
            }
            
        }
            
        if (loadingObjCount == 0)
        {
            GF.BuiltinView.HideLoadingProgress();
        }
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
            progress += 0.3f;
            GF.BuiltinView.SetLoadingProgress(progress);
            --loadingObjCount;
        }

        GF.Resource.LoadAsset(UtilityBuiltin.AssetsPath.GetTexturePath("RenderTextures/CarRendTex.renderTexture"), assetCallbacks);

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
            progress += 0.3f;
            GF.BuiltinView.SetLoadingProgress(progress);
            --loadingObjCount;
            MainMenu mainMenu = eventArgs.UIForm.Logic as MainMenu;
            ShowCar(mainMenu.RawImageGo); // 加载汽车
        }
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

    private void ShowCar(GameObject rawImageGo, int i = 0)
    {
        //动态创建关卡，默认读取ID为0的车
        var lvRow = vehicleInfoTable.GetDataRow(i);
        var carParams = EntityParams.Create(Vector3.zero, Vector3.zero, Vector3.one);
        carParams.Set<VarGameObject>(Const.RAW_IMAGE, rawImageGo);
        carEntityId = GF.Entity.ShowEntity<CarEntity>(lvRow.PrefabName, Const.EntityGroup.Vehicle, carParams);
        ++loadingObjCount;
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
    }
    private void LoadCarImages()
    {
        for (int i = 0; i < vehicleInfoTable.Count; ++i)
        {
            var userData = (i, "Car");
            GF.Resource.LoadAsset(UtilityBuiltin.AssetsPath.GetSpritesPath(vehicleInfoTable[i].CarImageAssetName), assetCallbacks, userData);
            ++loadingObjCount;
            var userData2 = (i, "CarLogo");
            GF.Resource.LoadAsset(UtilityBuiltin.AssetsPath.GetSpritesPath(vehicleInfoTable[i].CarLogo), assetCallbacks, userData2);
            ++loadingObjCount;
        }
    }
    private void LoadPartImages()
    {
        for (int i = 0; i < partInfoTable.Count; ++i)
        {
            var userData = (i, "Part");
            GF.Resource.LoadAsset(UtilityBuiltin.AssetsPath.GetSpritesPath(partInfoTable[i].PartImage), assetCallbacks, userData);
            ++loadingObjCount;
        }
    }
    public void ChangeVehiclePrefeb(int i)
    {
        var ui = GF.UI.GetUIForm(menuUIFormId).Logic as MainMenu;
        GF.Entity.HideEntity(carEntityId);
        carEntity = null;
        ShowCar(ui.RawImageGo, i);
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
}
