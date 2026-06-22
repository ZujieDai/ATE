using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.CAN
{
    public class SPN
    {
        public enum StateName
        {
            SPN3090,
            SPN3091,
            SPN3092,
            SPN3093,
            SPN3094,
            SPN3095,
            SPN3096,

            SPN3511_12,
            SPN3511_34,
            SPN3511_56,
            SPN3511_78,

            SPN3512_12,
            SPN3512_34,
            SPN3512_56,
            SPN3512_78,
            SPN3512_9,
            SPN3512_11,
            SPN3512_13,
            SPN3512_15,

            SPN3513_12,
            SPN3513_34,
            SPN3513_56,

            SPN3521_12,
            SPN3521_34,
            SPN3521_56,
            SPN3521_78,

            SPN3522_12,
            SPN3522_34,
            SPN3522_56,
            SPN3522_78,
            SPN3522_9,
            SPN3522_11,
            SPN3522_13,
            SPN3522_15,

            SPN3523_12,
            SPN3523_34,
            SPN3523_56,

            SPN3901,
            SPN3902,
            SPN3903,
            SPN3904,
            SPN3905,
            SPN3906,
            SPN3907,

            SPN3921,
            SPN3922,
            SPN3923,
            SPN3924,
            SPN3925,
            SPN3926,
            SPN3927,
            SPN3928,
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
        public enum SPN2560
        {
            Recognized,
            Unrecognized
        }
        public enum SPN2829
        {
            BeReady,
            NotReady,
            Invalid
        }
        public enum SPN3929
        {
            Permit,
            Pause
        }
    }
}
