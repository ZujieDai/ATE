using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.EquipStateData
{
    /// <summary>
    /// 电阻负载实时状态数据
    /// </summary>
    public class ResisLoad_StateData : StateDataBase
    {
        /// <summary>
        /// 负载类型（交、直流）
        /// </summary>
        public EmChargerType emType { get; set; }
        /// <summary>
        /// 实际电压A相
        /// </summary>
        public float ActualVolt_A { get; set; }
        /// <summary>
        /// 实际电压B相
        /// </summary>
        public float ActualVolt_B { get; set; }
        /// <summary>
        /// 实际电压C相
        /// </summary>
        public float ActualVolt_C { get; set; }
        /// <summary>
        /// 实际电流A相
        /// </summary>
        public float ActualCurrent_A { get; set; }
        /// <summary>
        /// 实际电流B相
        /// </summary>
        public float ActualCurrent_B { get; set; }
        /// <summary>
        /// 实际电流C相
        /// </summary>
        public float ActualCurrent_C { get; set; }
        /// <summary>
        /// 实际功率
        /// </summary>
        public float ActualPower { get; set; }
        /// <summary>
        /// 实际电阻
        /// </summary>
        public float ActualResis { get; set; }
        /// <summary>
        /// 需求电压
        /// </summary>
        public float DemandVolt { get; set; }
        /// <summary>
        /// 需求电流
        /// </summary>
        public float DemandCurrent { get; set; }
        /// <summary>
        /// 需求功率
        /// </summary>
        public float DemandPower { get; set; }
        /// <summary>
        /// 需求电阻
        /// </summary>
        public float DemandResis { get; set; }
        /// <summary>
        /// 在线设备
        /// </summary>

        public int OnlineEquip { get; set; }

        public ResisLoad_StateData()
        {
            EquipName += "电阻负载";
        }
    }
}
