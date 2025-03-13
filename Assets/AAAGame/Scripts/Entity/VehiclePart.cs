using UnityEngine;
public enum VehiclePartTypeEnum
{
    LunGu = 0,         // 轮胎
    CheKe = 1,          // 车壳
    Count = 2           // 来个计数
}

public class VehiclePart : EntityBase
{
    [SerializeField]
    private VehiclePartTypeEnum vehiclePartTypeEnum;
    [SerializeField]
    private int partId;
    public int PartId { get { return partId; } set { partId = value; } }
    public VehiclePartTypeEnum PartTypeEnum { get { return vehiclePartTypeEnum; } set { vehiclePartTypeEnum = value; } }
}
