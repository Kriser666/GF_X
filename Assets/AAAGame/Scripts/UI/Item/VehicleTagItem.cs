using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class VehicleTagItem : UIItemBase, IPointerClickHandler
{
    public ChooseVehicle ChooseVehicle;
    private int vehicleId;
    
    public int VehicleId { get { return vehicleId; } set { vehicleId = value; } }
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
            foreach (var item in ChooseVehicle.VehicleTagItems)
            {
                if (item != this)
                {
                    item.ChooseCancel();
                }
            }
            GF.Event.Fire(this, CarItemSelectedEventArgs.Create(CarUIItemSelectedDataType.Changed, vehicleId));
        }
        else
        {
            selected = false;
            varBoarder.SetActive(false);
            GF.Event.Fire(this, CarItemSelectedEventArgs.Create(CarUIItemSelectedDataType.Changed, -1));
        }
    }

    public void ChooseCancel()
    {
        selected = false;
        varBoarder.SetActive(false);
    }

}
