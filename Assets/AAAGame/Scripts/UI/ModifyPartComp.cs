using GameFramework.DataTable;
using TMPro;
using UnityEngine;
using UnityGameFramework.Runtime;
public partial class ModifyPartComp : UIFormBase
{
    private float afterTotalPower = 0f;
    private float afterTotalBrake = 0f;
    private float afterTotalAcceleration = 0f;
    private int carId;
    private IDataTable<VehicleInfoTable> vehicleInfoTable;
    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        vehicleInfoTable = GF.DataTable.GetDataTable<VehicleInfoTable>();

        // 初始化改装前的数值
        carId = Params.Get<VarInt32>(Const.VEHICLE_ID);
        var dataRow = vehicleInfoTable.GetDataRow(carId);
        for (int i = 0; i < varBeforeNumArr.Length; ++i)
        {
            var strKey = varBeforeNumArr[i].transform.parent.GetComponentInChildren<UIStringKey>().Key;
            switch(strKey)
            {
                case "MPC.TOTAL_PERFORMANCE": // 总性能
                    varBeforeNumArr[i].text = (dataRow.Power + dataRow.Brake + dataRow.Acceleration).ToString();
                    break;
                case "MG.POWER": // 动力
                    varBeforeNumArr[i].text = dataRow.Power.ToString();
                    break;
                case "MG.BRAKE": // 制动
                    varBeforeNumArr[i].text = dataRow.Brake.ToString();
                    break;
                case "MG.ACCELERATION": // 加速度
                    varBeforeNumArr[i].text = dataRow.Acceleration.ToString();
                    break;
            }
        }

        // 改装后的数值
        afterTotalPower = Params.Get<VarFloat>(Const.CUR_TOTAL_POWER);
        afterTotalBrake = Params.Get<VarFloat>(Const.CUR_TOTAL_BRAKE);
        afterTotalAcceleration = Params.Get<VarFloat>(Const.CUR_TOTAL_ACCELERATION);
        for (int i = 0; i < varAfterNumArr.Length; ++i)
        {
            var strKey = varAfterNumArr[i].transform.parent.GetComponentInChildren<UIStringKey>().Key;
            float before = 0f;
            float after = 0f;
            switch (strKey)
            {
                case "MPC.TOTAL_PERFORMANCE": // 总性能
                    before = dataRow.Power + dataRow.Brake + dataRow.Acceleration;
                    after = before + afterTotalPower + afterTotalBrake + afterTotalAcceleration;
                    break;
                case "MG.POWER": // 动力
                    before = dataRow.Power;
                    after = before + afterTotalPower;
                    break;
                case "MG.BRAKE": // 制动
                    before = dataRow.Brake;
                    after = before + afterTotalBrake;
                    break;
                case "MG.ACCELERATION": // 加速度
                    before = dataRow.Acceleration;
                    after = before + afterTotalAcceleration;
                    break;
            }
            SetStrAndColor(before, after, varAfterNumArr[i]);
        }
    }

    private void SetStrAndColor(float before, float after, TextMeshProUGUI textMesh)
    {
        textMesh.text = after.ToString();
        if (after > before)
        {
            textMesh.color = Color.green;
        }
        else if (after < before)
        {
            textMesh.color = Color.red;
        }
    }

}
