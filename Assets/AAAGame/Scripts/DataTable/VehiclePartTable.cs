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
/// VehiclePartTable
/// </summary>
public class VehiclePartTable : DataRowBase
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
        /// 部件名称
        /// </summary>
        public string PartName
        {
            get;
            private set;
        }

        /// <summary>
        /// 部件类型
        /// </summary>
        public VehiclePartTypeEnum PartType
        {
            get;
            private set;
        }

        /// <summary>
        /// 对应的汽车ID
        /// </summary>
        public int VehicleId
        {
            get;
            private set;
        }

        /// <summary>
        /// 价格
        /// </summary>
        public float Cost
        {
            get;
            private set;
        }

        /// <summary>
        /// 预制体名称
        /// </summary>
        public string PrefebName
        {
            get;
            private set;
        }

        /// <summary>
        /// 缩略图名称
        /// </summary>
        public string PartImage
        {
            get;
            private set;
        }

        /// <summary>
        /// 动力
        /// </summary>
        public float Power
        {
            get;
            private set;
        }

        /// <summary>
        /// 制动
        /// </summary>
        public float Brake
        {
            get;
            private set;
        }

        /// <summary>
        /// 加速度
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
            PartName = columnStrings[index++];
            PartType = DataTableExtension.ParseEnum<VehiclePartTypeEnum>(columnStrings[index++]);
            VehicleId = int.Parse(columnStrings[index++]);
            Cost = float.Parse(columnStrings[index++]);
            PrefebName = columnStrings[index++];
            PartImage = columnStrings[index++];
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
                    PartName = binaryReader.ReadString();
                    PartType = binaryReader.ReadEnum<VehiclePartTypeEnum>();
                    VehicleId = binaryReader.Read7BitEncodedInt32();
                    Cost = binaryReader.ReadSingle();
                    PrefebName = binaryReader.ReadString();
                    PartImage = binaryReader.ReadString();
                    Power = binaryReader.ReadSingle();
                    Brake = binaryReader.ReadSingle();
                    Acceleration = binaryReader.ReadSingle();
                }
            }

            return true;
        }

//__DATA_TABLE_PROPERTY_ARRAY__
}
