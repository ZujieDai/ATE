using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.CAN
{
    public class MatchClass
    {
        public enum StateName
        {
            HighLow,
            OverI,
            TooHigh,
            Abnormal,
            Permit,
            Achieved,
            Pause,
            Trouble,
            ExceedingDemand,
            Unusual,
            AchievedConditionPaused,
            ManPaused,
            TroublePaused,
            BMSPaused,
            HighTemp,
            EnergyTransfer,
            EmergencyStop,
            Mismatched
        }
        public class StateType
        {
            public const string Normal = "正常";
            public const string TooHigh = "过高";
            public const string TooLow = "过低";
            public const string OverCurrent = "过流";
            public const string OverTemp = "过温";
            public const string TimeOut = "超时";
            public const string Untrusted = "不可信状态";
            public const string Forbid = "禁止";
            public const string Permit = "允许";

            public const string Abnormal = "不正常";
            public const string Undefined = "Undefined SPN";

            public const string NotAchievedSoc = "未达到所需SOC目标值";
            public const string AchievedSoc = "达到所需SOC目标值";

            public const string NotAchievedTotalVolt = "未达到总电压设定值";
            public const string AchievedTotalVolt = "达到总电压设定值";

            public const string NotAchievedSingleVolt = "未达到单体电压的设定值";
            public const string AchievedSingleVolt = "达到单体电压的设定值";

            public const string NotAchieved = "未达到";
            public const string Achieved = "达到";

            public const string AchievedConditionPaused = "达到充电机设定条件中止";

            public const string ChargePaused = "充电机中止(收到CST帧)";

            public const string Trouble = "故障";
            public const string OverReq = "超过需求值";
            public const string Unusual = "异常";

            public const string Paused = "中止";
            public const string ManPaused = "人工中止";
            public const string TroublePaused = "故障中止";
            public const string BSTPaused = "故障中止(收到BST帧)";
            public const string BmsPaused = "BMS主动中止";
            public const string CannotTransfer = "不能传送";
            public const string EmergencyStop = "急停";

            public const string Matched = "匹配";
            public const string Mismatched = "不匹配";
        }
    }
}
