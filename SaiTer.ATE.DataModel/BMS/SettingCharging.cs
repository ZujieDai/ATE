using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.BMS
{
    public class SettingCharging
    {
        //BCL
        /// <summary>
        /// 电压需求V
        /// </summary>
        public string ReqV { get; set; }
        /// <summary>
        /// 电流需求A
        /// </summary>
        public string ReqI { get; set; }
        /// <summary>
        /// 充电模式
        /// </summary>
        public string ChargeMode { get; set; }
        /// <summary>
        /// BCL报文周期
        /// </summary>
        public string BCLPeriod { get; set; }

        //BCS
        /// <summary>
        /// 充电电压测量值
        /// </summary>
        public string MeasureV { get; set; }
        /// <summary>
        /// 充电电流测量值
        /// </summary>
        public string MeasureI { get; set; }
        /// <summary>
        /// 最高单体动力蓄电池电压(0~24V)
        /// </summary>
        public string MaxSingleBatV { get; set; }
        /// <summary>
        /// 最高单体动力蓄电池组号(0~15)
        /// </summary>
        public string MaxSingleBatGrpNum { get; set; }
        /// <summary>
        /// 当前荷电状态SOC(0~100%)
        /// </summary>
        public string CurSOC { get; set; }
        /// <summary>
        /// 估算剩余充电时间(0~600min)
        /// </summary>
        public string RemainTime { get; set; }
        /// <summary>
        /// BCS报文周期(ms)
        /// </summary>
        public string BCSPeriod { get; set; }

        //BSM
        /// <summary>
        /// 最高单体动力蓄电池电压所在编号(1~25)
        /// </summary>
        public string MaxSingleBatVNum { get; set; }
        /// <summary>
        /// 最高动力蓄电池温度(-50℃~+200℃)
        /// </summary>
        public string MaxBatTemp { get; set; }
        /// <summary>
        /// 最高温度检测点编号(1~128)
        /// </summary>
        public string MaxTempDetectionNum { get; set; }
        /// <summary>
        /// 最低动力蓄电池温度(-50℃~+200℃)
        /// </summary>
        public string MinBatTemp { get; set; }
        /// <summary>
        /// 最低温度检测点编号(1~128)
        /// </summary>
        public string MinTempDetectionNum { get; set; }

        /// <summary>
        /// 单体动力蓄电池电压过高/过低
        /// </summary>
        public string BitStateSingleV { get; set; }

        /// <summary>
        /// 整车动力蓄电池荷电状态SOC过高/过低
        /// </summary>
        public string BitStateSOC { get; set; }

        /// <summary>
        /// 动力蓄电池充电过电流
        /// </summary>
        public string BitStateOverI { get; set; }

        /// <summary>
        /// 动力蓄电池温度过高
        /// </summary>
        public string BitStateOverTemp { get; set; }

        /// <summary>
        /// 动力蓄电池绝缘状态
        /// </summary>
        public string BitStateInsulate { get; set; }

        /// <summary>
        /// 动力蓄电池组输出连接器连接状态
        /// </summary>
        public string BitStateConnState { get; set; }

        /// <summary>
        /// 充电允许
        /// </summary>
        public string BitStateChargePermit { get; set; }

        /// <summary>
        /// BSM周期
        /// </summary>
        public string BSMPeriod { get; set; }

        //BST
        //--BMS终止充电原因：

        /// <summary>
        /// 达到所需求的SOC目标值
        /// </summary>
        public string AchievedSOC { get; set; }

        /// <summary>
        /// 达到总电压的设定值
        /// </summary>
        public string AchievedTotalV { get; set; }
        public string AchievedSingleV { get; set; }
        public string BmsPause { get; set; }

        //--BMS终止充电故障原因
        /// <summary>
        /// 绝缘故障
        /// </summary>
        public string InsulateTrouble { get; set; }

        /// <summary>
        /// 输出连接器过温故障
        /// </summary>
        public string OutputConnTrouble { get; set; }

        /// <summary>
        /// BMS元件、输出连接器过温
        /// </summary>
        public string BmsConnTempTrouble { get; set; }

        /// <summary>
        /// 充电连接器故障
        /// </summary>
        public string ChargeConnTrouble { get; set; }

        /// <summary>
        /// 电池组温度过高故障
        /// </summary>
        public string BatOverTempTrouble { get; set; }

        /// <summary>
        /// 高压继电器故障
        /// </summary>
        public string RelayTrouble { get; set; }

        /// <summary>
        /// 检测点2电压检测故障
        /// </summary>
        public string Detection2Trouble { get; set; }

        /// <summary>
        /// 其他故障
        /// </summary>
        public string OtherTrouble { get; set; }

        //--BMS终止充电错误原因
        /// <summary>
        /// 电流过大
        /// </summary>
        public string OverIError { get; set; }

        /// <summary>
        /// 电压异常
        /// </summary>
        public string UnusualVError { get; set; }
        /// <summary>
        /// BST报文周期(ms)
        /// </summary>
        public string BSTPeriod { get; set; }
    }
}

