using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.EquipStateData
{
    /// <summary>
    /// 桩模拟器（NTGX）实时状态数据
    /// </summary>
    public class Charger_NTGX_StateData : StateDataBase
    {

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

        public Charger_NTGX_StateData()
        {
            EquipName = "充电桩模拟器";
        }
    }
}
