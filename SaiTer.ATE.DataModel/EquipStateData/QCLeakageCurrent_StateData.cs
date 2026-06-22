using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.EquipStateData
{
    /// <summary>
    /// 测试类型：
    /// 1=脱扣电流；2=S2突现时间；3=闭合时间；7=S1突现时间
    /// </summary>
    public enum emTestMode
    {
        TrippingSurrent = 1,
        EmergentTime_S2 = 2,
        CloseTime = 3,
        EmergentTime_S1 = 7
    }

    /// <summary>
    /// 齐充剩余电流保护测试仪实时状态数据
    /// </summary>
    public class QCLeakageCurrent_StateData : StateDataBase
    {
        /// <summary>
        /// 测试类型
        /// </summary>
        public emTestMode TestMode { get; set; }
        /// <summary>
        /// 脱扣电流
        /// </summary>
        public double TripCurrent { get; set; }
        /// <summary>
        /// 脱扣时间
        /// </summary>
        public double TripTime { get; set; }
        /// <summary>
        /// 测试结果
        /// 0=未检测到分断(NO TRIP),1=检测到分断,2=过压保护分断,3=超时未闭合,4=安全检测未通过,5=自检失败,6=设备过流保护测试失败,7=设备电压异常测试失败
        /// </summary>
        public int TestResult { get; set; }
        /// <summary>
        /// 合闸状态
        /// </summary>
        public int TestSW { get; set; }
        /// <summary>
        /// 电压使能 0="已断开" 1="已使能"
        /// </summary>
        public int EnableVoltage {  get; set; }
        /// <summary>
        /// 电流使能 0="已断开" 1="已使能"
        /// </summary>
        public int EnableCurrent { get; set; }
        /// <summary>
        /// 设备运行状态
        /// 0=空闲,1=测试中,2=报警,3=自检1失败,4=自检2失败,5=自检3失败,6=自检4失败,7=自检5失败,8=过流保护,9=电压异常
        /// </summary>
        public int RunTime {  get; set; }
        /// <summary>
        /// 设备受控状态 0=手动模式 1=上位机控制模式
        /// </summary>
        public int RemoteStatus { get; set; }

        public QCLeakageCurrent_StateData()
        {
            EquipName = "QC剩余电流保护测试仪";
        }
    }
}
