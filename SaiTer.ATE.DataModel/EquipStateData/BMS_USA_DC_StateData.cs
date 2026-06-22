using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel
{
    /// <summary>
    /// 直流BMS实时状态数据
    /// </summary>
    public class BMS_USA_DC_StateData : StateDataBase
    {
        ///// <summary>
        ///// 对应的充电枪ID
        ///// </summary>
        //public int ChargerID { get; set; }
        private string _SystemState = "";
        /// <summary>
        /// 系统状态
        /// </summary>
        public string SystemState
        {
            get { return _SystemState; }
            set { _SystemState = value; }
        }
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
        /// 充电功率
        /// </summary>
        public double ChargingPower { get; set; }
        /// <summary>
        /// 充电电量
        /// </summary>
        public double ChargingQuantity { get; set; }
        /// <summary>
        /// CP占空比
        /// </summary>
        public double CPDutyCycle { get; set; }
        /// <summary>
        /// CP电压
        /// </summary>
        public double CPVoltage { get; set; }
        /// <summary>
        /// CP频率
        /// </summary>
        public double CPFrequency { get; set; }
        /// <summary>
        /// 枪座温度
        /// </summary>
        public double ChargerTemp { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public double ErrorMessage { get; set; }

        public BMS_USA_DC_StateData()
        {
            EquipName = "美标直流导引BMS";
        }
    }
}
