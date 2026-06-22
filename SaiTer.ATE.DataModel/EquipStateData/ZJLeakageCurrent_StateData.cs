using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.EquipStateData
{
    /// <summary>
    /// 中佳剩余电流保护测试仪实时状态数据
    /// </summary>
    public class ZJLeakageCurrent_StateData : StateDataBase
    {
        /// <summary>
        /// 实时电压
        /// </summary>
        public float NowVoltage { get; set; }
        /// <summary>
        /// 实时电流
        /// </summary>
        public float NowCurrent { get; set; }
        /// <summary>
        /// 测试时间
        /// </summary>
        public float TestTime { get; set; }
        /// <summary>
        /// 测试电流
        /// </summary>
        public float TestCurrent { get; set; }
        /// <summary>
        /// 报警显示
        /// </summary>
        public string AlarmInfo { get; set; }
        /// <summary>
        /// 预先调整动作电流
        /// </summary>
        public string PresetCurrent { get; set; }
        /// <summary>
        /// ABC三相切换
        /// </summary>
        public string Phase { get; set; }
        /// <summary>
        /// S3开关状态
        /// </summary>
        public string S3_State { get; set; }

        /// <summary>
        /// S2开关状态
        /// </summary>
        public string S2_State { get; set; }

        /// <summary>
        /// S1开关状态
        /// </summary>
        public string S1_State { get; set; }
        public ZJLeakageCurrent_StateData()
        {
            EquipName = "ZJ剩余电流保护测试仪";
        }
    }
}
