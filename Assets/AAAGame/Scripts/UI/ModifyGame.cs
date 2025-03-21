using GameFramework;
using GameFramework.DataTable;
using GameFramework.Event;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

public partial class ModifyGame : UIFormBase
{
    IDataTable<VehiclePartTable> vehiclePartTable;
    GameObject vehiclePartTagTemplate;
    GameObject partTypeTagTemplate;
    public Dictionary<VehiclePartTypeEnum, List<VehiclePartTagItem>> VehiclePartTagItems;
    public List<Button> partTypeButtons;
    GameObject rawImage;
    private GameFrameworkAction<GameObject> frameworkAction;
    private MenuProcedure procedure;
    private float curPerformance = 0f;
    private float curCost = 0f;
    private int vehicleId = -1;
    private int modifyId = -1;
    private CarDataModel carData;

    protected override void OnInit(object userData)
    {
        base.OnInit(userData);

        vehiclePartTable = GF.DataTable.GetDataTable<VehiclePartTable>();
        vehiclePartTagTemplate = varPartContent.transform.GetChild(0).gameObject;
        partTypeTagTemplate = varPartTypeContent.transform.GetChild(0).gameObject;
        VehiclePartTagItems = new();
        partTypeButtons = new();
    }

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        procedure = GF.Procedure.CurrentProcedure as MenuProcedure;
        procedure.ChangeBackGroundSprite("BackGroundSprite", "UI/OriginalCarGarage/cheku_beijing@3x.png");
        // CameraController.Instance.SetCameraView(11);
        vehicleId = Params.Get<VarInt32>(Const.VEHICLE_ID, 0);
        modifyId = Params.Get<VarInt32>(Const.MODIFY_ID, -1);
        carData = GF.DataModel.GetOrCreate<CarDataModel>();
        if (modifyId != -1)
        {
            // 计算当前改装下的总性能和花费
            float performance = 0f;
            float cost = 0f;
            if (carData.CarWithModifyIdWithModifyParams[0][0][0].partsIds.Count > 0)
            {
                var partsList = carData.CarWithModifyIdWithModifyParams[vehicleId][modifyId];
                foreach (var ModifiedPart in partsList)
                {
                    foreach (var partId in ModifiedPart.partsIds)
                    {
                        var vehiclePartRow = vehiclePartTable.GetDataRow(partId);
                        float perf_t = vehiclePartRow.Brake + vehiclePartRow.Acceleration + vehiclePartRow.Power;
                        performance += perf_t;
                        cost += vehiclePartRow.Cost;
                    }
                }

                ChangeCostAndPerformanceText(cost, performance);
            }
            
        }
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
        // 生成部件种类按钮
        string btnNamePrefix = partTypeTagTemplate.name.Split('_')[0];
        for (int i = 0; i < (int)VehiclePartTypeEnum.Count; ++i)
        {
            var clonedItem = SpawnItem<UIItemObject>(partTypeTagTemplate, varPartTypeContent.transform);
            clonedItem.gameObject.name = btnNamePrefix + i;
            clonedItem.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = GF.Localization.GetString("PART_TYPE." + i);
            Button button = clonedItem.gameObject.GetComponent<Button>();
            partTypeButtons.Add(button);
        }
        partTypeTagTemplate.SetActive(false);
        // 生成部件缩略图，默认生成轮胎即btn为0
        SpawnPartByType(vehiclePartTagTemplate, (VehiclePartTypeEnum)0, vehicleId);

        GF.Event.Subscribe(PartItemSelectedEventArgs.EventId, ItemSelectedHandler);

        CameraController.Instance.SetModelRendererCameraOffset(2, true);

    }

    protected override void OnReveal()
    {
        base.OnReveal();
        CameraController.Instance.SetCameraView(2);
    }

    protected override void OnClose(bool isShutdown, object userData)
    {
        GF.Event.Unsubscribe(PartItemSelectedEventArgs.EventId, ItemSelectedHandler);
        frameworkAction?.Invoke(rawImage);
        CameraController.Instance.SetModelRendererCameraOffset(1, true);
        // CameraController.Instance.SetCameraView(10);
        partTypeButtons.Clear();
        VehiclePartTagItems.Clear();
        base.OnClose(isShutdown, userData);
    }

    private void ItemSelectedHandler(object sender, GameEventArgs e)
    {
        var eventArgs = e as PartItemSelectedEventArgs;
        var dataRow = vehiclePartTable.GetDataRow(eventArgs.PartId);

        float performance = dataRow.Brake + dataRow.Acceleration + dataRow.Power;
        if (eventArgs.DataType == PartUIItemSelectedDataType.Selected)
        {
            // 如果选中其他部件，就更新部件性能、价格和替换模型
            procedure.ChangeCarPart(eventArgs.VehiclePartType, dataRow.PrefebName);
            // 寻找当前部件类型下的组件个数
            var modifyParams = procedure.OriginalModifyParams();
            int partCount = modifyParams[(int)eventArgs.VehiclePartType].parts.Count;
            for (int i = 0; i < partCount; ++i)
            {
                curPerformance += performance;
                curCost += dataRow.Cost;
            }
        }
        // 复原部件
        else if (eventArgs.DataType == PartUIItemSelectedDataType.CancelSelected)
        {
            // -1就全部换了
            if (eventArgs.PartOfIdx == -1)
            {
                procedure.ResetPart(eventArgs.VehiclePartType);
                // 寻找当前部件类型下的组件个数
                var modifyParams = procedure.OriginalModifyParams();
                int partCount = modifyParams[(int)eventArgs.VehiclePartType].parts.Count;
                for (int i = 0; i < partCount; ++i)
                {
                    curPerformance -= performance;
                    curCost -= dataRow.Cost;
                }
            }
        }
        ChangeCostAndPerformanceText(curCost, curPerformance);
    }

    protected override void OnButtonClick(object sender, Button btSelf)
    {
        base.OnButtonClick(sender, btSelf);
        for (int i = 0; i < partTypeButtons.Count; ++i)
        {
            // 当前点击的按钮的类型
            VehiclePartTypeEnum curLoopTypeEnum = (VehiclePartTypeEnum)i;
            if (btSelf == partTypeButtons[i])
            {
                // 显示当前类型下的所有的部件
                if (!VehiclePartTagItems.ContainsKey(curLoopTypeEnum))
                {
                    SpawnPartByType(vehiclePartTagTemplate, curLoopTypeEnum, vehicleId);
                }
                else
                {
                    for (int j = 0; j < VehiclePartTagItems[curLoopTypeEnum].Count; ++j)
                    {
                        VehiclePartTagItems[curLoopTypeEnum][j].gameObject.SetActive(true);
                    }
                }
            }
            else
            {
                // 隐藏其他的类型下所有的部件
                if (VehiclePartTagItems.ContainsKey(curLoopTypeEnum))
                {
                    foreach (var item in VehiclePartTagItems[curLoopTypeEnum])
                    {
                        item.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    private void SpawnPartByType(GameObject templateToClone, VehiclePartTypeEnum vehiclePartType, int carId)
    {
        string namePrefix = templateToClone.name.Split('_')[0];
        // 根据部件类型和能装的车辆ID筛选
        var whichPartDatas = vehiclePartTable.Where((vehiclePartRow) => { return vehiclePartRow.PartType == vehiclePartType; })
            .Where((vehiclePartRow) => { return vehiclePartRow.VehicleId == carId; });
        int i = 0;
        List<VehiclePartTagItem> partItems = new();
        foreach (var part in whichPartDatas)
        {
            var clonedItem = SpawnItem<UIItemObject>(templateToClone, varPartContent.transform);
            var vehiclePartTag = clonedItem.itemLogic as VehiclePartTagItem;
            vehiclePartTag.PartId = part.Id;
            vehiclePartTag.WhichType = part.PartType;
            float performance = part.Brake + part.Acceleration + part.Power;
            if (performance > 0f)
            {
                vehiclePartTag.VarPerformanceNum.text = Const.ADDITION_SYMBOL + performance;
            }
            else
            {
                vehiclePartTag.VarPerformanceNum.text = performance.ToString();
            }
            vehiclePartTag.VarPartImage.sprite = procedure.PartSprites[part.Id];
            vehiclePartTag.VarPartName.text = GF.Localization.GetString(part.PartName);
            vehiclePartTag.name = namePrefix + i;
            vehiclePartTag.modifyGame = this;
            partItems.Add(vehiclePartTag);
            ++i;
        }
        VehiclePartTagItems.Add(vehiclePartType, partItems);
        templateToClone.SetActive(false);
    }

    protected override void OnButtonClick(object sender, string btId)
    {
        base.OnButtonClick(sender, btId);
        switch(btId)
        {
            case "CloseButton":
                frameworkAction?.Invoke(rawImage);
                GF.UI.Close(this.Id);
                break;
            case "ModificationDetailBtn":
                UIParams modifyPartDetailParams = UIParams.Create();
                modifyPartDetailParams.Set(Const.PART_ID_LIST, procedure.CurrentPartIdList());
                modifyPartDetailParams.Set<VarInt32>(Const.VEHICLE_ID, vehicleId);
                OpenSubUIForm(UIViews.ModifyPartDetail, 1, modifyPartDetailParams);
                break;
            case "OneKeyChange":
                OneKeyChange();
                break;
            case "OneKeyReset":
                OneKeyReset();
                break;
            case "SelectButton":
                OpenSubUIForm(UIViews.ModifySaved, 1);
                procedure.SaveModify();
                break;
        }    
    }

    public void OneKeyChange()
    {
        // 一键改装每个类型的最高性能
        curPerformance = 0;
        curCost = 0;
        for (int i = 0; i < (int)VehiclePartTypeEnum.Count; i++)
        {
            VehiclePartTypeEnum curLoopTypeEnum = (VehiclePartTypeEnum)i;
            if (!VehiclePartTagItems.ContainsKey(curLoopTypeEnum))
            {
                SpawnPartByType(vehiclePartTagTemplate, curLoopTypeEnum, vehicleId);
                foreach (var item in VehiclePartTagItems[curLoopTypeEnum])
                {
                    item.gameObject.SetActive(false);
                }
            }
            var curPartTags = VehiclePartTagItems[curLoopTypeEnum];
            int maxPerformanceId = 0;
            float maxPerformance = float.MinValue;
            int maxPerformanceInTagsIdx = 0;
            for (int j = 0; j < curPartTags.Count; ++j)
            {
                var tableRow = vehiclePartTable.GetDataRow(curPartTags[j].PartId);
                float performance = tableRow.Brake + tableRow.Acceleration + tableRow.Power;
                if (performance > maxPerformance)
                {
                    maxPerformance = performance;
                    maxPerformanceId = curPartTags[j].PartId;
                    maxPerformanceInTagsIdx = j;
                }
            }
            // 选中当前类型下的最大性能的标签
            curPartTags[maxPerformanceInTagsIdx].Choose();
            VehiclePartTable maxPerformanceRow = vehiclePartTable.GetDataRow(maxPerformanceId);
            // 寻找当前部件类型下的组件个数
            var modifyParams = procedure.OriginalModifyParams();
            int partCount = modifyParams[i].parts.Count;
            for (int j = 0; j < partCount; ++j)
            {
                curPerformance += maxPerformance;
                curCost += maxPerformanceRow.Cost;
            }
            string prefebName = maxPerformanceRow.PrefebName;
            ChangeCostAndPerformanceText(curCost, curPerformance);
            procedure.ChangeCarPart(curLoopTypeEnum, prefebName);
        }
    }

    public void OneKeyReset()
    {
        if (procedure.CarHasBeenModified())
        {
            for (int i = 0; i < (int)VehiclePartTypeEnum.Count; i++)
            {
                VehiclePartTypeEnum curLoopTypeEnum = (VehiclePartTypeEnum)i;
                procedure.ResetPart(curLoopTypeEnum);
                // 取消选中当前类型下的所有组件
                if (VehiclePartTagItems.ContainsKey(curLoopTypeEnum))
                {
                    foreach (var item in VehiclePartTagItems[curLoopTypeEnum])
                    {
                        item.ChooseCancel();
                    }
                }
            }
            curPerformance = 0;
            curCost = 0;
            ChangeCostAndPerformanceText(curCost, curPerformance);
        }
    }

    private void ChangeCostAndPerformanceText(float cost, float performance)
    {
        if (performance >= 0f)
        {
            varPerfNum.text = Const.ADDITION_SYMBOL + performance.ToString();
            varPerfNum.color = Const.TEXT_ADD_COLOR;
        }
        else
        {
            varPerfNum.text = performance.ToString();
            varPerfNum.color = Color.red;
        }
        if (cost >= 0f)
        {
            varCostNum.text = Const.ADDITION_SYMBOL + cost.ToString();
            varCostNum.color = Const.TEXT_ADD_COLOR;
        }
        else
        {
            varCostNum.text = cost.ToString();
            varCostNum.color = Color.red;
        }
    }
}
