using GameFramework;
using GameFramework.DataTable;
using GameFramework.Event;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityGameFramework.Runtime;

public partial class ChooseVehicle : UIFormBase
{
    IDataTable<VehicleInfoTable> vehicleInfoTable;
    GameObject vehicleTagTemplate;
    public List<VehicleTagItem> VehicleTagItems;
    private int selectedVehicleId;
    GameObject rawImage;
    private GameFrameworkAction<GameObject> frameworkAction;
    private MenuProcedure procedure;

    public TMP_InputField.SubmitEvent InputTextChangedEvent { get; private set; }

    protected override void OnInit(object userData)
    {
        base.OnInit(userData);

        vehicleInfoTable = GF.DataTable.GetDataTable<VehicleInfoTable>();
        vehicleTagTemplate = varContent.transform.GetChild(0).gameObject;
        VehicleTagItems = new();
        InputTextChangedEvent = new();
    }

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        procedure = GF.Procedure.CurrentProcedure as MenuProcedure;
        if (Params.TryGet<VarGameObject>(Const.RAW_IMAGE, out var rawImageObj))
        {
            rawImage = rawImageObj;
            rawImage.transform.SetParent(varMiddle.transform);
            rawImage.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
        if (Params.TryGet<VarObject>(Const.SET_RAW_IMAGE_CALLBACK, out var callback))
        {
            frameworkAction = (callback.Value) as GameFrameworkAction<GameObject>;
        }
        int i = 0;
        string namePrefix = vehicleTagTemplate.name.Split('_')[0];
        foreach (var vehicle in vehicleInfoTable)
        {
            var clonedItem = SpawnItem<UIItemObject>(vehicleTagTemplate, varContent.transform);
            var vehicleTag = clonedItem.itemLogic as VehicleTagItem;
            vehicleTag.VehicleId = vehicle.Id;
            vehicleTag.VarCarImage.sprite = procedure.CarSprites[vehicle.Id];
            vehicleTag.ChooseVehicle = this;
            vehicleTag.name = namePrefix + i;
            VehicleTagItems.Add(vehicleTag);
            ++i;
        }
        vehicleTagTemplate.SetActive(false);
        
        varInputText.onEndEdit = InputTextChangedEvent;

        // -1代表没有选中物体
        selectedVehicleId = -1;
        GF.Event.Subscribe(CarItemSelectedEventArgs.EventId, ItemSelectedHandler);
        InputTextChangedEvent.AddListener(InputTextChanged);
    }

    protected override void OnClose(bool isShutdown, object userData)
    {
        GF.Event.Unsubscribe(CarItemSelectedEventArgs.EventId, ItemSelectedHandler);
        frameworkAction?.Invoke(rawImage);
        base.OnClose(isShutdown, userData);
    }

    private void ItemSelectedHandler(object sender, GameEventArgs e)
    {
        var eventArgs = e as CarItemSelectedEventArgs;
        if (eventArgs.DataType == CarUIItemSelectedDataType.Changed)
        {
            selectedVehicleId = eventArgs.IdValue;
            // 如果选中其他车，就更新车辆文本、Logo和模型
            if (eventArgs.IdValue != -1)
            {
                var dataRow = vehicleInfoTable.GetDataRow(selectedVehicleId);
                varCarNameText.text = GF.Localization.GetString(dataRow.CarName);
                varCarDesc.text = GF.Localization.GetString(dataRow.CarDesc);
                varCarLogo.sprite = procedure.CarLogoSprites[dataRow.Id];
                procedure.ChangeVehiclePrefeb(selectedVehicleId);
            }
        }
    }


    private void InputTextChanged(string arg0)
    {
        foreach (var item in VehicleTagItems)
        {
            if (!item.VarText_VehicleName.text.Contains(arg0))
            {
                item.gameObject.SetActive(false);
            }
            else
            {
                item.gameObject.SetActive(true);
            }
        }
    }

    protected override void OnButtonClick(object sender, string btId)
    {
        base.OnButtonClick(sender, btId);
        switch(btId)
        {
            case "SelectButton":
                if (selectedVehicleId > -1)
                {
                    GF.Event.Fire(this, CarItemSelectedEventArgs.Create(CarUIItemSelectedDataType.Choosed, selectedVehicleId));
                    frameworkAction?.Invoke(rawImage);
                    GF.UI.Close(this.Id);
                }
                break;
        }
    }

}
