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
    private int curModifyId;
    [JsonProperty]
    // 车辆ID-改装ID-改装的部件列表
    private readonly Dictionary<int, Dictionary<int, List<ModifiedParts>>> carWithModifyIdWithModifyParams;

    [JsonIgnore]
    public Dictionary<int, Dictionary<int, List<ModifiedParts>>> CarWithModifyIdWithModifyParams
    {
        get { return carWithModifyIdWithModifyParams; }
    }

    [JsonIgnore]
    public int CurCarId { get { return curCarId; } set { curCarId = value; } }
    [JsonIgnore]
    public int CurModifyId 
    { 
        get 
        { 
            return curModifyId; 
        } 
        set
        { 
            if(value - curModifyId != 1)
            {
                ++curModifyId;
            }
            else
            {
                curModifyId = value;
            }
            // 改装历史自增到新的改装后需要添加初始化结构维护进存档结构中
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
            carWithModifyIdWithModifyParams[curCarId].Add(curModifyId, modifiedPartsList);
        }
    }
    [JsonIgnore]
    public List<ModifiedParts> PartsIds
    {
        get { return carWithModifyIdWithModifyParams[CurCarId][curModifyId]; }
        set 
        {
            carWithModifyIdWithModifyParams[CurCarId][curModifyId] = value;
        }
    }

    public CarDataModel()
    {
        carWithModifyIdWithModifyParams = new();
    }
    protected override void OnCreate(RefParams userdata)
    {
        base.OnCreate(userdata);
        GF.Event.Subscribe(CarItemSelectedEventArgs.EventId, CarItemSelectedEventHandler);
        GF.Event.Subscribe(GFEventArgs.EventId, OnGFEventCallback);
    }
    protected override void OnRelease()
    {
        GF.Event.Unsubscribe(CarItemSelectedEventArgs.EventId, CarItemSelectedEventHandler);
        GF.Event.Unsubscribe(GFEventArgs.EventId, OnGFEventCallback);

        base.OnRelease();
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
            // 保存的时候进行改装件自增的操作
            ++CurModifyId;
        }
        // 选择了车辆，就需要根据车辆已有的改装ID列表去重置当前的改装ID
        if (eventArgs.DataType == CarUIItemSelectedDataType.Choosed)
        {
            curCarId = eventArgs.IdValue;
            var curModifyIdWithParams = carWithModifyIdWithModifyParams[curCarId];
            int maxModifyId = curModifyIdWithParams.Count - 1;
            curModifyId = maxModifyId;
        }
    }

    protected override void OnInitialDataModel()
    {
        base.OnInitialDataModel();
        var dataTables = GF.DataTable.GetDataTable<VehicleInfoTable>();
        curCarId = dataTables.ElementAt(0).Id;

        for (int i = 0; i < dataTables.Count; ++i)
        {
            Dictionary<int, List<ModifiedParts>> modifyIdWithModifyParams = new();
            curModifyId = 0;
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

            modifyIdWithModifyParams.Add(curModifyId, modifiedPartsList);
            carWithModifyIdWithModifyParams.Add(dataTables[i].Id, modifyIdWithModifyParams);
        }
    }

}
