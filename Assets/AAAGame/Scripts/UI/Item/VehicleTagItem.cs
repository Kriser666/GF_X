using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class VehicleTagItem : UIItemBase, IPointerClickHandler
{
    private Image image;
    private Color originalColor;
    private Color swapedColor = new (0.5f, 0.5f, 0.5f);
    public ChooseVehicle ChooseVehicle;
    private int vehicleId;
    
    public int VehicleId { get { return vehicleId; } set { vehicleId = value; } }
    public TextMeshProUGUI VarText_VehicleName { get { return varVehicleName; } set { varVehicleName = value; } }

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
            foreach (var item in ChooseVehicle.VehicleTagItems)
            {
                if (item != this)
                {
                    item.SetOriginalColor();
                }
            }
            GF.Event.Fire(this, UIItemSelectedEventArgs.Create(UIItemSelectedDataType.Changed, vehicleId));
        }
        else
        {
            image.color = originalColor;
            GF.Event.Fire(this, UIItemSelectedEventArgs.Create(UIItemSelectedDataType.Changed, -1));
        }
    }

    public void SetOriginalColor()
    {
        image.color = originalColor;
    }

}
