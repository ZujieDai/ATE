using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.BMS
{
    public class SettingCharging_JP_DC
    {
        /// <summary>
        /// 最小电池电压
        /// </summary>
        public string MinBatteryVolt { get; set; }
        /// <summary>
        /// 最大电池电压
        /// </summary>
        public string MaxBatteryVolt { get; set; }
        /// <summary>
        /// 充电率参考常数（%）
        /// </summary>
        public string ChargingRateConst { get; set; }
        /// <summary>
        /// 最大充电时间(s)
        /// </summary>
        public string MaxChargingTime_s { get; set; }
        /// <summary>
        /// 最大充电时间(m)
        /// </summary>
        public string MaxChargingTime_m{ get; set; }
        /// <summary>
        /// 预计充电时间（m）
        /// </summary>
        public string EstimatedChargingTime_m { get; set; }
        /// <summary>
        /// 充电协议版本号
        /// </summary>
        public string CHAdeMOProtocolNumber { get; set; }
        /// <summary>
        /// 目标电池电压(充电需求电压)
        /// </summary>
        public string TargetBatteryVolt { get; set; }
        /// <summary>
        /// 充电需求电流
        /// </summary>
        public string ChargingCurrent { get; set; }
        /// <summary>
        /// 充电需求功率（%）
        /// </summary>
        public string ChargingRate { get; set; }
        /// <summary>
        /// 充电故障标志
        /// </summary>
        public int[] FaultFlags { get; set; }
        /// <summary>
        /// 充电状态标志
        /// </summary>
        public int[] StateFlags { get; set; }

        public SettingCharging_JP_DC()
        {
            MinBatteryVolt = "200";
            MaxBatteryVolt = "600";
            ChargingRateConst = "100";
            MaxChargingTime_s = "90";
            MaxChargingTime_m = "90";
            EstimatedChargingTime_m = "90";
            CHAdeMOProtocolNumber = "2";
            TargetBatteryVolt = "500";
            ChargingCurrent = "20";
            ChargingRate = "69";
            FaultFlags = new int[8];
            StateFlags = new int[8];
            StateFlags[0] = 1;
        }
    }
}
