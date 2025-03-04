public partial class ModifyPartDetailItem : UIItemBase
{
    private VehiclePartTable vehiclePartRow;
    public VehiclePartTable VehiclePartTable { get { return vehiclePartRow; } }

    public void SetText(VehiclePartTable vehiclePartRow)
    {
        this.vehiclePartRow = vehiclePartRow;
        varPartName.text = GF.Localization.GetString(vehiclePartRow.PartName);
        varPartType.text = GF.Localization.GetString("PART_TYPE." + ((int)vehiclePartRow.PartType).ToString());
        varPartPerformance.text = vehiclePartRow.Performance.ToString();
        varPartCost.text = vehiclePartRow.Cost.ToString();
    }
}
