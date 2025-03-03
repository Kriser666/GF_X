using GameFramework;
using GameFramework.DataTable;
using GameFramework.Event;
using System.Collections.Generic;
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
    public List<VehiclePartTagItem> VehiclePartTagItems;
    public List<Button> partTypeButtons;
    GameObject rawImage;
    private GameFrameworkAction<GameObject> frameworkAction;
    private MenuProcedure procedure;
    private float curPerformance = 0f;
    private float curCost = 0f;


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
            partTypeButtons.Add(clonedItem.gameObject.GetComponent<Button>());
        }
        partTypeTagTemplate.SetActive(false);
        // 生成部件缩略图，默认生成轮胎即btn为0
        SpawnPartByType(vehiclePartTagTemplate, (VehiclePartTypeEnum)0);

        GF.Event.Subscribe(PartItemSelectedEventArgs.EventId, ItemSelectedHandler);
    }

    protected override void OnClose(bool isShutdown, object userData)
    {
        GF.Event.Unsubscribe(PartItemSelectedEventArgs.EventId, ItemSelectedHandler);
        frameworkAction?.Invoke(rawImage);
        base.OnClose(isShutdown, userData);
    }

    private void ItemSelectedHandler(object sender, GameEventArgs e)
    {
        var eventArgs = e as PartItemSelectedEventArgs;
        var dataRow = vehiclePartTable.GetDataRow(eventArgs.PartId);

        if (eventArgs.DataType == PartUIItemSelectedDataType.Selected)
        {
            // 如果选中其他部件，就更新部件性能、价格和替换模型
            procedure.ChangeCarPart(eventArgs.VehiclePartType, dataRow.PrefebName);
            curPerformance += dataRow.Performance;
            curCost += dataRow.Cost;
        }
        // 复原部件
        else if (eventArgs.DataType == PartUIItemSelectedDataType.CancelSelected)
        {
            // -1就全部换了
            if (eventArgs.PartOfIdx == -1)
            {
                procedure.ResetPart(eventArgs.VehiclePartType);
                curPerformance -= dataRow.Performance;
                curCost -= dataRow.Cost;
            }
        }
        varCostAndPer.text = curCost + GF.Localization.GetString("MONEY.CURRENCY") + '\n'
                + GF.Localization.GetString("MG.PERFORMANCE") + '+' + curPerformance;
    }

    protected override void OnButtonClick(object sender, Button btSelf)
    {
        base.OnButtonClick(sender, btSelf);
        for (int i = 0; i < partTypeButtons.Count; ++i)
        {
            if(btSelf == partTypeButtons[i])
            {
                for (int j = 0; j < VehiclePartTagItems.Count; ++j)
                {
                    Destroy(VehiclePartTagItems[j]);
                }
                VehiclePartTagItems.Clear();
                SpawnPartByType(vehiclePartTagTemplate, (VehiclePartTypeEnum)i);
                break;
            }
        }
    }

    private void SpawnPartByType(GameObject templateToClone, VehiclePartTypeEnum vehiclePartType)
    {
        string namePrefix = templateToClone.name.Split('_')[0];
        // 根据部件类型筛选
        var whichPartDatas = vehiclePartTable.Where((vehiclePartRow) => { return vehiclePartRow.PartType == vehiclePartType; });
        int i = 0;
        foreach (var part in whichPartDatas)
        {
            var clonedItem = SpawnItem<UIItemObject>(templateToClone, varPartContent.transform);
            var vehiclePartTag = clonedItem.itemLogic as VehiclePartTagItem;
            vehiclePartTag.PartId = part.Id;
            vehiclePartTag.WhichType = part.PartType;
            vehiclePartTag.VarPerformanceNum.text = "+" + part.Performance;
            vehiclePartTag.VarPartImage.sprite = procedure.PartSprites[part.Id];
            vehiclePartTag.VarPartName.text = GF.Localization.GetString(part.PartName);
            vehiclePartTag.name = namePrefix + i;
            VehiclePartTagItems.Add(vehiclePartTag);
            ++i;
        }
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
            case "OneKeyChange":
                // 一键改装每个类型的最高性能
                for (int i = 0; i < (int)VehiclePartTypeEnum.Count; i++)
                {
                    int curType = i;
                    var whichPartDatas = vehiclePartTable.Where((vehiclePartRow) => { return vehiclePartRow.PartType == (VehiclePartTypeEnum)curType; });
                    VehiclePartTable maxPerformance = whichPartDatas.ElementAt(0);
                    foreach (var item in whichPartDatas)
                    {
                        if (item.Performance > maxPerformance.Performance)
                        {
                            maxPerformance = item;
                        }
                    }
                    string prefebName = maxPerformance.PrefebName;
                    procedure.ChangeCarPart((VehiclePartTypeEnum)i, prefebName);
                }
                break;
            case "OneKeyReset":
                for (int i = 0; i < (int)VehiclePartTypeEnum.Count; i++)
                {
                    procedure.ResetPart((VehiclePartTypeEnum)i);
                }
                break;
            case "SelectButton":

                break;
        }    
    }
}
