using UnityEngine;
using System.Collections;



public enum VehiclePartTypeEnum
{
    LunGu = 0,         // 轮胎
    Count = 1           // 来个计数
}

public class VehiclePart : EntityBase
{
    private VehiclePartTypeEnum vehiclePartTypeEnum;
    private int partId;
    public int PartId { get { return partId; } set { partId = value; } }
    public VehiclePartTypeEnum PartTypeEnum { get { return vehiclePartTypeEnum; } set { vehiclePartTypeEnum = value; } }
}
