using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel
{
    /// <summary>
    /// 直流BMS实时状态数据
    /// </summary>
    public class BMS_DC_StateData : StateDataBase
    {
       
        ///// <summary>
        ///// 对应的充电枪ID
        ///// </summary>
        //public int ChargerID { get; set; }
        /// <summary>
        /// 连接状态
        /// </summary>
        public bool ConnectState { get; set; }
        /// <summary>
        /// 充电电压
        /// </summary>
        public double ChargingVoltage { get; set; }
        /// <summary>
        /// 充电电流
        /// </summary>
        public double ChargingCurrent { get; set; }
        /// <summary>
        /// CC1电压
        /// </summary>
        public double CC1Voltage { get; set; }
        /// <summary>
        /// CC2电压
        /// </summary>
        public double CC2Voltage { get; set; }
        /// <summary>
        /// 辅助电源电压
        /// </summary>
        public double APSVoltage { get; set; }

        private string _ChargingState = "";

        /// <summary>
        /// 充电状态
        /// </summary>
        public string ChargingState
        {
            get { return _ChargingState; }
            set { _ChargingState = value; }
        }
        /// <summary>
        /// DC+温度
        /// </summary>
        public double DCPulsTemp { get; set; }
        /// <summary>
        /// DC-温度
        /// </summary>
        public double DCMinusTemp { get; set; }
        /// <summary>
        /// 环境温度
        /// </summary>
        public double EnvironmentTemp { get; set; }
        /// <summary>
        /// 枪座温度
        /// </summary>
        public double ChargerTemp { get; set; }

        public BMS_DC_StateData()
        {
            EquipName = "直流导引BMS";
        }
    }
}
