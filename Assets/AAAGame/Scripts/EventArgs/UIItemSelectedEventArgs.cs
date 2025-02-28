using GameFramework.Event;
using GameFramework;

public enum UIItemSelectedDataType
{
    Changed = 0,
    Choosed
}

public class UIItemSelectedEventArgs : GameEventArgs
{
    public static readonly int EventId = typeof(UIItemSelectedEventArgs).GetHashCode();
    public override int Id => EventId;
    public UIItemSelectedDataType DataType { get; private set; }
    public int IdValue { get; private set; }

    public static UIItemSelectedEventArgs Create(UIItemSelectedDataType type, int newV)
    {
        var instance = ReferencePool.Acquire<UIItemSelectedEventArgs>();
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
