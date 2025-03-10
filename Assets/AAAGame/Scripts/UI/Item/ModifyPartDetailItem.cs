using System.Collections.Generic;
using UnityEngine;

public partial class ModifyPartDetailItem : UIItemBase
{
    private float power;
    public float Power { get { return power; } }
    private float brake;
    public float Brake { get { return brake; } }
    private float acceleration;
    public float Acceleration { get { return acceleration; } }
    public void SetText(VehiclePartTable vehiclePartRow)
    {
        varPartName.text = GF.Localization.GetString(vehiclePartRow.PartName);
        varPartType.text = GF.Localization.GetString("PART_TYPE." + ((int)vehiclePartRow.PartType).ToString());
        power = vehiclePartRow.Power;
        varPartPower.text = power.ToString();
        brake = vehiclePartRow.Brake;
        varPartBrake.text = brake.ToString();
        acceleration = vehiclePartRow.Acceleration;
        varPartAcceleration.text = acceleration.ToString();
        varPartCost.text = vehiclePartRow.Cost.ToString();
    }

}
