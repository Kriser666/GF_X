using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class MH_VehicleHistoryItem : UIItemBase, IPointerClickHandler
{
    private int curCarId;
    public int CurCarId { get { return curCarId; } set { curCarId = value; } }
    public TextMeshProUGUI VarCarName { get { return varCarName; } set { varCarName = value; } }
    private Image image;
    private Color originalColor;
    private Color swapedColor = new(0.5f, 0.5f, 0.5f);
    public ModHistory ModifyHistory;
    private int vehicleId;

    public int VehicleId { get { return vehicleId; } set { vehicleId = value; } }
    public TextMeshProUGUI VarText_VehicleName { get { return varCarName; } set { varCarName.text = value.text; } }

    protected override void OnInit()
    {
        base.OnInit();
        image = GetComponent<Image>();
        originalColor = image.color;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (originalColor == image.color)
        {
            image.color = swapedColor;
            ModifyHistory.RefreshCarInfoInTail(curCarId);
            foreach (var item in ModifyHistory.MH_VehicleHistoryItems)
            {
                if (item != this)
                {
                    item.SetOriginalColor();
                }
            }
        }
        else
        {
            image.color = originalColor;
        }
    }

    public void SetOriginalColor()
    {
        image.color = originalColor;
    }
}
