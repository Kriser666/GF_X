using GameFramework.Event;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public struct ModifiedParts
{
    [JsonProperty]
    public VehiclePartTypeEnum whichType; // 改装的部件类型
    [JsonProperty]
    public List<int> partsIds; // 该类型下的部件们的ID
}
public class CarDataModel : DataModelStorageBase
{
    [JsonProperty]
    private int curCarId;
    [JsonProperty]
    private Dictionary<int, List<ModifiedParts>> carWithModifyParams;

    [JsonIgnore]
    public int CurCarId { get { return curCarId; } set { curCarId = value; } }
    [JsonIgnore]
    public List<ModifiedParts> PartsIds
    {
        get { return carWithModifyParams[CurCarId]; }
        set 
        {
            carWithModifyParams[CurCarId] = value;
        }
    }

    public CarDataModel()
    {
        carWithModifyParams = new();
    }
    protected override void OnCreate(RefParams userdata)
    {
        base.OnCreate(userdata);
        GF.Event.Subscribe(CarItemSelectedEventArgs.EventId, CarItemSelectedEventHandler);

        GF.Event.Subscribe(GFEventArgs.EventId, OnGFEventCallback);
    }
    protected override void OnRelease()
    {
        base.OnRelease();
        GF.Event.Unsubscribe(CarItemSelectedEventArgs.EventId, CarItemSelectedEventHandler);
        GF.Event.Unsubscribe(GFEventArgs.EventId, OnGFEventCallback);
    }

    private void OnGFEventCallback(object sender, GameEventArgs e)
    {
        var eventArgs = e as GFEventArgs;
        if (eventArgs.EventType == GFEventType.ApplicationQuit)
        {
            GF.DataModel.ReleaseDataModel<PlayerDataModel>();
        }
    }

    private void CarItemSelectedEventHandler(object sender, GameEventArgs e)
    {
        var eventArgs = e as CarItemSelectedEventArgs;
        if (eventArgs.DataType == CarUIItemSelectedDataType.Saved)
        {
            Save();
        }
    }

    protected override void OnInitialDataModel()
    {
        base.OnInitialDataModel();
        var dataTables = GF.DataTable.GetDataTable<VehicleInfoTable>();
        curCarId = dataTables.ElementAt(0).Id;

        for (int i = 0; i < dataTables.Count; ++i)
        {
            List<ModifiedParts> modifiedPartsList = new();
            // 先创建ModifiedParts，根据部件类型
            for (int j = 0; j < (int)VehiclePartTypeEnum.Count; ++j)
            {
                ModifiedParts modifiedParts = new()
                {
                    whichType = (VehiclePartTypeEnum)j,
                    partsIds = new()
                };
                modifiedPartsList.Add(modifiedParts);
            }

            carWithModifyParams.Add(dataTables[i].Id, modifiedPartsList);
        }
    }

}
