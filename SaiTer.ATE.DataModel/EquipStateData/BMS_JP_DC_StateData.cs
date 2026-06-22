using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel
{
    /// <summary>
    /// 日标直流BMS实时状态数据
    /// </summary>
    public class BMS_JP_DC_StateData : StateDataBase
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
        /// 环境温度
        /// </summary>
        public double ChargerTemp { get; set; }
        /// <summary>
        /// 版本号
        /// </summary>
        public double Version { get; set; }
        /// <summary>
        /// Signal d1
        /// </summary>
        public double Signal_d1 { get; set; }
        /// <summary>
        /// Signal d2
        /// </summary>
        public double Signal_d2 { get; set; }
        /// <summary>
        /// Signal k
        /// </summary>
        public double Signal_k { get; set; }
        /// <summary>
        /// ProxDetect
        /// </summary>
        public double ProxDetect { get; set; }

        public BMS_JP_DC_StateData()
        {
            EquipName = "日标直流导引BMS";
        }
    }
}
