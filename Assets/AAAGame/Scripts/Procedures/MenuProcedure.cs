using GameFramework.DataTable;
using GameFramework.Event;
using GameFramework.Fsm;
using GameFramework.Procedure;
using GameFramework.Resource;
using UnityEngine;
using UnityGameFramework.Runtime;
public class MenuProcedure : ProcedureBase
{
    int menuUIFormId;
    int carEntityId;
    private CarEntity carEntity;
    LoadAssetCallbacks assetCallbacks;
    IDataTable<VehicleInfoTable> vehicleInfoTable;
    IFsm<IProcedureManager> procedure;
    int loadingObjCount;
    float progress;
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
        ShowMenu();// 加载菜单
        assetCallbacks = new(LoadAssetSucceed, LoadAssetFailed);
        ++loadingObjCount;
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
        if (asset is RenderTexture)
        {
            // 验证RenderTexture参数
            var rt = asset as RenderTexture;
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
        base.OnLeave(procedureOwner, isShutdown);
    }

    private void ShowCar(GameObject rawImageGo, int i = 0)
    {
        //动态创建关卡，默认读取第0个车
        vehicleInfoTable = GF.DataTable.GetDataTable<VehicleInfoTable>();
        var lvRow = vehicleInfoTable.GetDataRow(i);
        var carParams = EntityParams.Create(Vector3.zero, Vector3.zero, Vector3.one);
        carParams.Set<VarGameObject>(Const.RAW_IMAGE, rawImageGo);
        carEntityId = GF.Entity.ShowEntity<CarEntity>(lvRow.PrefabName, Const.EntityGroup.Vehicle, carParams);
        ++loadingObjCount;
    }
    public void EnterGame()
    {
        procedure.SetData<VarUnityObject>("CarEntity", carEntity);

        ChangeState<GameProcedure>(procedure);
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

    public void ChangeVehiclePrefeb(int i)
    {
        var ui = GF.UI.GetUIForm(menuUIFormId).Logic as MainMenu;
        GF.Entity.HideEntity(carEntityId);
        carEntity = null;
        ShowCar(ui.RawImageGo, i);
    }
}
