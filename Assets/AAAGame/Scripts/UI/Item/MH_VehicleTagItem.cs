﻿using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public partial class MH_VehicleTagItem : UIItemBase, IPointerClickHandler
{
    public ModHistory ModifyHistory;
    private int vehicleId;
    private int modifyId;
    public int VehicleId { get { return vehicleId; } set { vehicleId = value; } }
    public int ModifyId { get { return modifyId; } set { modifyId = value; } }
    public TextMeshProUGUI VarText_VehicleName { get { return varVehicleName; } set { varVehicleName.text = value.text; } }
    public Image VarCarImage { get { return varCarImage; } set { varCarImage = value; } }

    private bool selected;

    protected override void OnInit()
    {
        base.OnInit();
        varBoarder.SetActive(false);
        selected = false;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!selected)
        {
            selected = true;
            varBoarder.SetActive(true);
            foreach (var item in ModifyHistory.MH_VehicleTagItems)
            {
                if (item != this)
                {
                    item.ChooseCancel();
                }
            }
            ModifyHistory.ModifyHistoryCarSelected(vehicleId, modifyId);
        }
        else
        {
            selected = false;
            varBoarder.SetActive(false);
        }
    }

    public void ChooseCancel()
    {
        selected = false;
        varBoarder.SetActive(false);
    }
}
