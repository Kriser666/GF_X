//------------------------------------------------------------
//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：__DATA_TABLE_CREATE_TIME__
//------------------------------------------------------------

using GameFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityGameFramework.Runtime;

[System.Reflection.Obfuscation(Feature = "renaming", ApplyToMembers = false)]
/// <summary>
/// VehicleInfoTable
/// </summary>
public class VehicleInfoTable : DataRowBase
{
	private int m_Id = 0;
	/// <summary>
    /// 
    /// </summary>
    public override int Id
    {
        get { return m_Id; }
    }

        /// <summary>
        /// 预制体资源名称
        /// </summary>
        public string PrefabName
        {
            get;
            private set;
        }

        /// <summary>
        /// 车辆名称KEY
        /// </summary>
        public string CarName
        {
            get;
            private set;
        }

        /// <summary>
        /// 车辆缩略图资产名称
        /// </summary>
        public string CarImageAssetName
        {
            get;
            private set;
        }

        /// <summary>
        /// 车辆描述
        /// </summary>
        public string CarDesc
        {
            get;
            private set;
        }

        /// <summary>
        /// 车辆LOGO资产名称
        /// </summary>
        public string CarLogo
        {
            get;
            private set;
        }

        /// <summary>
        /// 车辆初始动力
        /// </summary>
        public float Power
        {
            get;
            private set;
        }

        /// <summary>
        /// 车辆初始制动
        /// </summary>
        public float Brake
        {
            get;
            private set;
        }

        /// <summary>
        /// 车辆初始加速度
        /// </summary>
        public float Acceleration
        {
            get;
            private set;
        }

        public override bool ParseDataRow(string dataRowString, object userData)
        {
            string[] columnStrings = dataRowString.Split(DataTableExtension.DataSplitSeparators);
            for (int i = 0; i < columnStrings.Length; i++)
            {
                columnStrings[i] = columnStrings[i].Trim(DataTableExtension.DataTrimSeparators);
            }

            int index = 0;
            index++;
            m_Id = int.Parse(columnStrings[index++]);
            index++;
            PrefabName = columnStrings[index++];
            CarName = columnStrings[index++];
            CarImageAssetName = columnStrings[index++];
            CarDesc = columnStrings[index++];
            CarLogo = columnStrings[index++];
            Power = float.Parse(columnStrings[index++]);
            Brake = float.Parse(columnStrings[index++]);
            Acceleration = float.Parse(columnStrings[index++]);

            return true;
        }

        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    m_Id = binaryReader.Read7BitEncodedInt32();
                    PrefabName = binaryReader.ReadString();
                    CarName = binaryReader.ReadString();
                    CarImageAssetName = binaryReader.ReadString();
                    CarDesc = binaryReader.ReadString();
                    CarLogo = binaryReader.ReadString();
                    Power = binaryReader.ReadSingle();
                    Brake = binaryReader.ReadSingle();
                    Acceleration = binaryReader.ReadSingle();
                }
            }

            return true;
        }

//__DATA_TABLE_PROPERTY_ARRAY__
}
