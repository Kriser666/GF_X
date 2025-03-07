using GameFramework;
using GameFramework.DataTable;
using GameFramework.Event;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

public partial class MainMenu : UIFormBase
{
    MenuProcedure procedure;
    int toShowVehicleId;
    int currentVehicleId;
    public int curModifyId = -1;
    IDataTable<VehicleInfoTable> vehicleInfoTable;
    public GameObject RawImageGo { get { return varCarModel; } }
    

    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        vehicleInfoTable = GF.DataTable.GetDataTable<VehicleInfoTable>();
        
    }

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);

        procedure = GF.Procedure.CurrentProcedure as MenuProcedure;
        currentVehicleId = 0;
        GF.Event.Subscribe(CarItemSelectedEventArgs.EventId, ItemSelectedHandler);
    }

    protected override void OnReveal()
    {
        base.OnReveal();

        CameraController.Instance.SetCameraView(10);
    }

    protected override void OnClose(bool isShutdown, object userData)
    {
        GF.Event.Unsubscribe(CarItemSelectedEventArgs.EventId, ItemSelectedHandler);
        base.OnClose(isShutdown, userData);
    }

    private void ItemSelectedHandler(object sender, GameEventArgs e)
    {
        var eventArgs = e as CarItemSelectedEventArgs;
        if (eventArgs.DataType == CarUIItemSelectedDataType.Choosed)
        {
            if (eventArgs.IdValue != -1 && eventArgs.IdValue != currentVehicleId)
            {
                toShowVehicleId = eventArgs.IdValue;
                currentVehicleId = toShowVehicleId;
                foreach (var vehicle in vehicleInfoTable)
                {
                    if (vehicle.Id == toShowVehicleId)
                    {
                        procedure.ChangeVehiclePrefeb(toShowVehicleId);
                        break;
                    }
                }
            }
        }
    }


    protected override void OnButtonClick(object sender, string btId)
    {
        base.OnButtonClick(sender, btId);
        switch(btId)
        {
            case "HistoryModification":
                UIParams modifyHisParams = UIParams.Create();
                modifyHisParams.Set<VarInt32>(Const.VEHICLE_ID, currentVehicleId);
                modifyHisParams.Set<VarGameObject>(Const.RAW_IMAGE, varCarModel);
                GameFrameworkAction<GameObject> gameFrameworkAction = SetRawImage;
                modifyHisParams.Set(Const.SET_RAW_IMAGE_CALLBACK, gameFrameworkAction);
                GF.UI.OpenUIForm(UIViews.ModHistory, modifyHisParams);
                break;
            case "ExitGame":
                var exit_time = DateTime.UtcNow.ToString();
                GF.Setting.SetString(ConstBuiltin.Setting.QuitAppTime, exit_time);
                GF.Setting.Save();
                Log.Info("Application Quit:{0}", exit_time);
                GF.Event.Fire(this, GFEventArgs.Create(GFEventType.ApplicationQuit));
                GF.Shutdown(ShutdownType.Quit);
                break;
        }
    }

    protected override void OnButtonClick(object sender, Button btSelf)
    {
        base.OnButtonClick(sender, btSelf);
        if (btSelf == varGameStart)
        {
            UIParams modifyParams = UIParams.Create();
            modifyParams.Set<VarInt32>(Const.VEHICLE_ID, currentVehicleId);
            modifyParams.Set<VarInt32>(Const.MODIFY_ID, curModifyId);
            modifyParams.Set<VarGameObject>(Const.RAW_IMAGE, varCarModel);
            GameFrameworkAction<GameObject> gameFrameworkAction = SetRawImage;
            modifyParams.Set(Const.SET_RAW_IMAGE_CALLBACK, gameFrameworkAction);
            GF.UI.OpenUIForm(UIViews.ModifyGame, modifyParams);
        }
        if (btSelf == varDropdownButtonObj.GetComponent<Button>())
        {
            UIParams chooseVehicleParams = UIParams.Create(true, 1);
            chooseVehicleParams.Set<VarGameObject>(Const.RAW_IMAGE, varCarModel);
            GameFrameworkAction<GameObject> gameFrameworkAction = SetRawImage;
            chooseVehicleParams.Set(Const.SET_RAW_IMAGE_CALLBACK, gameFrameworkAction);
            OpenSubUIForm(UIViews.ChooseVehicle, 1, chooseVehicleParams);
        }
        
    }

    private void SetRawImage(GameObject RowIamge)
    {
        if (RowIamge)
        {
            varCarModel = RowIamge;
            varCarModel.transform.SetParent(varMiddle.transform);
            varCarModel.transform.position = Vector3.zero;
        }
    }
}
