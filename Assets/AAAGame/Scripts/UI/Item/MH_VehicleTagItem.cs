using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public partial class MH_VehicleTagItem : UIItemBase, IPointerClickHandler
{
    private Image image;
    private Color originalColor;
    private Color swapedColor = new(0.5f, 0.5f, 0.5f);
    public ModHistory ModifyHistory;
    private int vehicleId;
    private int modifyId;
    public int VehicleId { get { return vehicleId; } set { vehicleId = value; } }
    public int ModifyId { get { return modifyId; } set { modifyId = value; } }
    public TextMeshProUGUI VarText_VehicleName { get { return varVehicleName; } set { varVehicleName.text = value.text; } }
    public Image VarCarImage { get { return varCarImage; } set { varCarImage = value; } }

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
            foreach (var item in ModifyHistory.MH_VehicleTagItems)
            {
                if (item != this)
                {
                    item.SetOriginalColor();
                }
            }
            ModifyHistory.ModifyHistoryCarSelected(vehicleId, modifyId);
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
