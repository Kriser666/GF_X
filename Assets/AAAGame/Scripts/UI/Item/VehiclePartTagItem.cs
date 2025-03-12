using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class VehiclePartTagItem : UIItemBase, IPointerClickHandler
{
    private int partId;
    private VehiclePartTypeEnum whichType;
    public ModifyGame modifyGame;
    
    public int PartId { get { return partId; } set { partId = value; } }
    public TextMeshProUGUI VarPartName { get { return varPartName; } set { varPartName = value; } }
    public TextMeshProUGUI VarPerformanceNum { get { return varPerformanceNum; } set { varPerformanceNum = value; } }
    public Image VarPartImage { get { return varPartImage; } set { varPartImage.sprite = value.sprite; } }
    public VehiclePartTypeEnum WhichType { get { return whichType; } set { whichType = value; } }

    protected override void OnInit()
    {
        base.OnInit();
        varBoarder.SetActive(false);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!varBoarder.activeSelf)
        {
            varBoarder.SetActive(true);
            foreach (var item in modifyGame.VehiclePartTagItems)
            {
                if (item != this)
                {
                    item.ChooseCancel();
                }
            }
            GF.Event.Fire(this, PartItemSelectedEventArgs.Create(PartUIItemSelectedDataType.Selected, partId, whichType));
        }
        else
        {
            varBoarder.SetActive(false);
            GF.Event.Fire(this, PartItemSelectedEventArgs.Create(PartUIItemSelectedDataType.CancelSelected, partId, whichType));
        }
    }
    public void ChooseCancel()
    {
        varBoarder.SetActive(false);
    }
    public void Choose()
    {
        varBoarder.SetActive(true);
    }
}
