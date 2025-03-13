using TMPro;
using UnityEngine;

public partial class ModifyPartDetailItem : UIItemBase
{
    private float power;
    public float Power { get { return power; } }

    private float brake;
    public float Brake { get { return brake; } }

    private float acceleration;
    public float Acceleration { get { return acceleration; } }

    private float cost;
    public float Cost { get { return cost; } }
    public void SetText(VehiclePartTable vehiclePartRow)
    {
        varPartName.text = GF.Localization.GetString(vehiclePartRow.PartName);
        varPartType.text = GF.Localization.GetString("PART_TYPE." + ((int)vehiclePartRow.PartType).ToString());

        power = vehiclePartRow.Power;
        SetTextColorAndSymbol(varPartPower, power);

        brake = vehiclePartRow.Brake;
        SetTextColorAndSymbol(varPartBrake, brake);

        acceleration = vehiclePartRow.Acceleration;
        SetTextColorAndSymbol(varPartAcceleration, acceleration);

        cost = vehiclePartRow.Cost;
        SetTextColorAndSymbol(varPartCost, cost);
    }
    private void SetTextColorAndSymbol(TextMeshProUGUI textMeshProUGUI, float num)
    {
        if (num > 0)
        {
            textMeshProUGUI.color = Color.green;
            textMeshProUGUI.text = Const.ADDITION_SYMBOL + num.ToString();
        }
        else if (num < 0)
        {
            textMeshProUGUI.color = Color.red;
            textMeshProUGUI.text = num.ToString();
        }
        else
        {
            textMeshProUGUI.color = Color.white;
            textMeshProUGUI.text = num.ToString();
        }
    }
}
