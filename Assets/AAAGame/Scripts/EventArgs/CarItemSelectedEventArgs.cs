using GameFramework.Event;
using GameFramework;

public enum CarUIItemSelectedDataType
{
    Changed = 0,
    Choosed,
    Saved
}

public class CarItemSelectedEventArgs : GameEventArgs
{
    public static readonly int EventId = typeof(CarItemSelectedEventArgs).GetHashCode();
    public override int Id => EventId;
    public CarUIItemSelectedDataType DataType { get; private set; }
    public int IdValue { get; private set; }

    public static CarItemSelectedEventArgs Create(CarUIItemSelectedDataType type, int newV)
    {
        var instance = ReferencePool.Acquire<CarItemSelectedEventArgs>();
        instance.DataType = type;
        instance.IdValue = newV;
        return instance;
    }
    public override void Clear()
    {
        DataType = default;
        IdValue = 0;
    }
}
