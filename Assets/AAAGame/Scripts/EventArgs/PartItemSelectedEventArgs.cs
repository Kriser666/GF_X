using GameFramework.Event;
using GameFramework;

public enum PartUIItemSelectedDataType
{
    Selected = 0,
    CancelSelected,
}

public class PartItemSelectedEventArgs : GameEventArgs
{
    public static readonly int EventId = typeof(PartItemSelectedEventArgs).GetHashCode();
    public override int Id => EventId;
    public PartUIItemSelectedDataType DataType { get; private set; }
    public VehiclePartTypeEnum VehiclePartType { get; private set; } // 部件类型
    public int PartId { get; private set; } // 部件ID
    public int PartOfIdx { get; private set; } // 当前部件类型下对应的列表的索引号

    public static PartItemSelectedEventArgs Create(PartUIItemSelectedDataType type, int newPartId, VehiclePartTypeEnum vehiclePartTypeEnum, int typeOfIdx = -1)
    {
        var instance = ReferencePool.Acquire<PartItemSelectedEventArgs>();
        instance.DataType = type;
        instance.PartId = newPartId;
        instance.VehiclePartType = vehiclePartTypeEnum;
        instance.PartOfIdx = typeOfIdx;
        return instance;
    }
    public override void Clear()
    {
        DataType = default;
        PartId = 0;
        VehiclePartType = default;
        PartOfIdx = 0;
    }
}
