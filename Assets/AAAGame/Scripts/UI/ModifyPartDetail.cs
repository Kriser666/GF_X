using GameFramework.DataTable;
using System.Collections.Generic;
using UnityGameFramework.Runtime;

public partial class ModifyPartDetail : UIFormBase
{
    private int carId;
	private List<int> curPartIdList;
    private IDataTable<VehiclePartTable> vehiclePartTables;
    private float totalPower = 0f;
    private float totalBrake = 0f;
    private float totalAcceleration = 0f;

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        vehiclePartTables = GF.DataTable.GetDataTable<VehiclePartTable>();
        carId = Params.Get<VarInt32>(Const.VEHICLE_ID);
        curPartIdList = null;
        if (Params.TryGet<VarObject>(Const.PART_ID_LIST, out VarObject varObject))
        {
            curPartIdList = varObject.Value as List<int>;
        }
        if (curPartIdList != null)
        {
            string namePrefix = varElemTemplate.name.Split('_')[0];
            int i = 0;
            foreach (var curPartId in curPartIdList)
            {
                var curPart = vehiclePartTables.GetDataRow(curPartId);
                var curElement = SpawnItem<UIItemObject>(varElemTemplate, varElemTemplate.transform.parent);
                curElement.gameObject.name = namePrefix + i;
                var itemCom = curElement.itemLogic as ModifyPartDetailItem;
                itemCom.SetText(curPart);
                totalPower += itemCom.Power;
                totalBrake += itemCom.Brake;
                totalAcceleration += itemCom.Acceleration;
                ++i;
            }
            varElemTemplate.SetActive(false);
        }
    }

    protected override void OnButtonClick(object sender, string btId)
    {
        base.OnButtonClick(sender, btId);
        switch(btId)
        {
            case "ModifyComp":
                UIParams compModParams = UIParams.Create(true);
                compModParams.Set<VarFloat>(Const.CUR_TOTAL_POWER, totalPower);
                compModParams.Set<VarFloat>(Const.CUR_TOTAL_BRAKE, totalBrake);
                compModParams.Set<VarFloat>(Const.CUR_TOTAL_ACCELERATION, totalAcceleration);
                compModParams.Set<VarInt32>(Const.VEHICLE_ID, carId);
                OpenSubUIForm(UIViews.ModifyPartComp, 1, compModParams);
                break;
        }
    }
}
