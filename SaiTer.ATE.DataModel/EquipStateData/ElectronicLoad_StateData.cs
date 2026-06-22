using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.EquipStateData
{
    public class ElectronicLoad_StateData : StateDataBase
    {
        public ElectronicLoad_StateData()
        {
            EquipName = "电子负载";
        }
        /// <summary>
        /// 输入电压值
        /// </summary>
        public float InputVoltage { get; set; }
        /// <summary>
        /// 输入电流值
        /// </summary>
        public float InputCurrent { get; set; }
        /// <summary>
        /// 输入功率值
        /// </summary>
        public float InputPower { get; set; }
        /// <summary>
        /// 操作状态1
        /// </summary>
        public string OperateState1 { get; set; }
        /// <summary>
        /// 操作状态2
        /// </summary>
        public string OperateState2 { get; set; }
        /// <summary>
        /// 系统状态1
        /// </summary>
        public string SystemState1 { get; set; }
        /// <summary>
        /// 系统状态2
        /// </summary>
        public string SystemState2 { get; set; }
    }
}
