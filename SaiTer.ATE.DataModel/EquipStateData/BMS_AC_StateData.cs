using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.EquipStateData
{
    /// <summary>
    /// 交流BMS实时状态数据
    /// </summary>
    public class BMS_AC_StateData : StateDataBase
    {
        /// <summary>
        /// 枪连接状态
        /// </summary>
        public  string  ConnectState { get; set; }
        /// <summary>
        /// A相电压
        /// </summary>
        public double PhaseA_Voltage { get; set; }
        /// <summary>
        /// B相电压
        /// </summary>
        public double PhaseB_Voltage { get; set; }
        /// <summary>
        /// C相电压
        /// </summary>
        public double PhaseC_Voltage { get; set; }
        /// <summary>
        /// A相电流
        /// </summary>
        public double PhaseA_Current { get; set; }
        /// <summary>
        /// B相电流
        /// </summary>
        public double PhaseB_Current { get; set; }
        /// <summary>
        /// C相电流
        /// </summary>
        public double PhaseC_Current { get; set; }
        /// <summary>
        /// 充电功率
        /// </summary>
        public double ChargePower { get; set; }
        /// <summary>
        /// 充电电量
        /// </summary>
        public double ChargeKwh { get; set; }

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
        /// CC电阻
        /// </summary>
        public double CCResistance { get; set; }
        /// <summary>
        /// 允许充电电流
        /// </summary>
        public double AllowChargingCurrent { get; set; }


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
        /// 枪座温度
        /// </summary>
        public double ChargerTemp { get; set; }

        public BMS_AC_StateData()
        {
            EquipName = "交流导引BMS";
        }
    }
}
