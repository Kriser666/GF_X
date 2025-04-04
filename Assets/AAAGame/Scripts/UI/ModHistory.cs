﻿using GameFramework;
using GameFramework.DataTable;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

public partial class ModHistory : UIFormBase
{
    public List<MH_VehicleTagItem> MH_VehicleTagItems;
    public List<MH_VehicleHistoryItem> MH_VehicleHistoryItems;
    private MenuProcedure procedure;
    private CarDataModel carData;
    private GameObject rawImage;
    private IDataTable<VehicleInfoTable> vehicleInfoTable;
    private IDataTable<VehiclePartTable> vehiclePartTable;
    private int selectedVehicleId;
    private int curModifyId = -1;
    private GameFrameworkAction<GameObject> frameworkAction;
    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        MH_VehicleTagItems = new();
        MH_VehicleHistoryItems = new();
    }
    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        selectedVehicleId = -1;
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
        procedure = GF.Procedure.CurrentProcedure as MenuProcedure;
        procedure.ChangeBackGroundSprite("BackGroundSprite", "UI/OriginalCarGarage/cheku_beijing@3x.png");
        carData = GF.DataModel.GetOrCreate<CarDataModel>();
        vehicleInfoTable = GF.DataTable.GetDataTable<VehicleInfoTable>();
        vehiclePartTable = GF.DataTable.GetDataTable<VehiclePartTable>();
        // 初始化左侧车辆历史列表
        int i = 0;
        string namePrefix = varCarHisModTpl.name.Split('_')[0];
        // 读取存档中的数据
        foreach (var vehicle in carData.CarWithModifyIdWithModifyParams)
        {
            bool modified = false;
            // 先扫描一遍，如果没有一个部件改装过，那就不生成了
            foreach (var modifyIdWithParts in vehicle.Value)
            {
                if (modified)
                {
                    break;
                }
                foreach (var parts in modifyIdWithParts.Value)
                {
                    if (parts.partsIds.Count > 0)
                    {
                        modified = true;
                        break;
                    }
                }
            }
            if (modified)
            {
                var clonedItem = SpawnItem<UIItemObject>(varCarHisModTpl, varCarHisModTpl.transform.parent);
                var vehicleHisTag = clonedItem.itemLogic as MH_VehicleHistoryItem;
                vehicleHisTag.VarCarName.text = GF.Localization.GetString(vehicleInfoTable.GetDataRow(vehicle.Key).CarName);
                vehicleHisTag.name = namePrefix + i;
                vehicleHisTag.CurCarId = vehicle.Key;
                MH_VehicleHistoryItems.Add(vehicleHisTag);
                ++i;
            }
        }
        
        varCarHisModTpl.SetActive(false);
        // 初始化底部改装的车辆，默认是显示第一个车的改装信息
        // RefreshCarInfoInTail(MH_VehicleHistoryItems[0].curCarId);

        varCarTemplate.SetActive(false);
    }
    protected override void OnClose(bool isShutdown, object userData)
    {
        frameworkAction?.Invoke(rawImage);
        var mainMenu = GF.UI.GetUIForm(procedure.menuUIFormId).Logic as MainMenu;
        mainMenu.curModifyId = curModifyId;
        MH_VehicleTagItems.Clear();
        MH_VehicleHistoryItems.Clear();
        base.OnClose(isShutdown, userData);
    }

    public void RefreshCarInfoInTail(int curCarId)
    {
        if (curCarId != selectedVehicleId)
        {
            selectedVehicleId = curCarId;
            // 先隐藏掉全部的子物体
            Transform parent = varCarTemplate.transform.parent;
            for (int j = 0; j < parent.childCount; ++j)
            {
                parent.GetChild(j).gameObject.SetActive(false);
            }
            // 再添加
            int i = 0;
            string namePrefix = varCarTemplate.name.Split('_')[0];
            foreach (var item in carData.CarWithModifyIdWithModifyParams[curCarId])
            {
                var clonedItem = SpawnItem<UIItemObject>(varCarTemplate, varCarTemplate.transform.parent);
                var vehicle = clonedItem.itemLogic as MH_VehicleTagItem;
                vehicle.VarCarImage.sprite = procedure.CarSprites[curCarId];
                vehicle.VarText_VehicleName.text = GF.Localization.GetString(vehicleInfoTable.GetDataRow(curCarId).CarName);
                vehicle.name = namePrefix + i;
                vehicle.VehicleId = curCarId;
                vehicle.ModifyId = item.Key;
                vehicle.gameObject.SetActive(true);
                MH_VehicleTagItems.Add(vehicle);
                ++i;
            }
        }

        varCarTemplate.SetActive(false);
    }
    public void ModifyHistoryCarSelected(int curVehicleId, int curModId)
    {
        selectedVehicleId = curVehicleId;
        curModifyId = curModId;
        var dataRow = vehicleInfoTable.GetDataRow(selectedVehicleId);
        varCarNameText.text = GF.Localization.GetString(dataRow.CarName);
        // 计算当前改装下的总性能和花费
        float performance = 0f;
        float cost = 0f;
        var partsList = carData.CarWithModifyIdWithModifyParams[curVehicleId][curModId];
        foreach (var modifiedPart in partsList)
        {
            foreach (var partId in modifiedPart.partsIds)
            {
                var vehiclePartRow = vehiclePartTable.GetDataRow(partId);
                float performance_t = vehiclePartRow.Brake + vehiclePartRow.Acceleration + vehiclePartRow.Power;
                performance += performance_t;
                cost += vehiclePartRow.Cost;
            }
        }
        if (performance >= 0f)
        {
            varPerformanceText.text = Const.ADDITION_SYMBOL + performance.ToString();
            varPerformanceText.color = Const.TEXT_ADD_COLOR;
        }
        else
        {
            varPerformanceText.text = performance.ToString();
            varPerformanceText.color = Color.red;
        }
        if (cost >= 0f)
        {
            varCostText.text = Const.ADDITION_SYMBOL + cost.ToString();
            varCostText.color = Const.TEXT_ADD_COLOR;
        }
        else
        {
            varCostText.text = cost.ToString();
            varCostText.color = Color.red;
        }
        varCarLogo.sprite = procedure.CarLogoSprites[dataRow.Id];

        procedure.ShowCarPrefebWithModParts(curVehicleId, curModId);
    }

    protected override void OnButtonClick(object sender, string btId)
    {
        base.OnButtonClick(sender, btId);

        switch(btId)
        {
            case "ModDetailBtn":
                UIParams modifyPartDetailParams = UIParams.Create();
                var dm = GF.DataModel.GetOrCreate<CarDataModel>();
                List<int> modifiedPartIdList = new();
                if (curModifyId != -1)
                {
                    foreach (var item in dm.CarWithModifyIdWithModifyParams[selectedVehicleId][curModifyId])
                    {
                        foreach (var partId in item.partsIds)
                        {
                            modifiedPartIdList.Add(partId);
                        }
                    }
                }
                modifyPartDetailParams.Set(Const.PART_ID_LIST, modifiedPartIdList);
                modifyPartDetailParams.Set<VarInt32>(Const.VEHICLE_ID, selectedVehicleId);
                OpenSubUIForm(UIViews.ModifyPartDetail, 1, modifyPartDetailParams);
                break;
            case "SelectButton":
                OnClickClose();
                break;
            case "CloseButton":
                curModifyId = -1;
                procedure.ResetAllParts();
                OnClickClose();
                break;
        }
    }
}
