using SaiTer.ATE.DataModel.BMS;
using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 设备基类    BMS相关虚函数
    /// </summary>
    public partial class EquipMentBase
    {
        /// <summary>
        /// 读实时数据
        /// </summary>
        public virtual void ReadCharger_StateData() { }

        /// <summary>
        /// 设置CML参数
        /// </summary>
        /// <param name="MaxU">最大电压</param>
        /// <param name="MinU">最小电压</param>
        /// <param name="MaxI">最大电流</param>
        /// <param name="MinI">最小电流</param>
        public virtual void SetCMLParam(double MaxU, double MinU, double MaxI, double MinI) { }

        /// <summary>
        /// 充电启动
        /// </summary>
        public virtual void ChargerStart() { }

        /// <summary>
        /// 充电停止
        /// </summary>
        public virtual void ChargerStop() { }

        /// <summary>
        /// 负载启动（可单独当负载使用）
        /// </summary>
        public virtual void LoadStart_Charger() { }

        /// <summary>
        /// 负载停止（可单独当负载使用）
        /// </summary>
        public virtual void LoadStop_Charger() { }

        /// <summary>
        /// 设置负载参数
        /// </summary>
        /// <param name="dVoltage">电压</param>
        /// <param name="dCurrent">电流</param>
        public virtual void SetLoadParam_Charger(double dVoltage,double dCurrent) { }

        /// <summary>
        /// 报文是否自动上传
        /// </summary>
        /// <param name="isUpLoad"></param>
        public virtual void CANMsgAutoUpLoad_Charger(bool isUpLoad) { }

    }
}
