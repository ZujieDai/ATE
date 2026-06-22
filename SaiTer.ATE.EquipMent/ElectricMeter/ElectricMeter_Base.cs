using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.EquipMent
{
    /// <summary>
    /// 设备基类——电表虚拟函数
    /// </summary>
    public partial class EquipMentBase
    {
        #region 电表的虚拟函数
        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="JcqAdd">寄存器起始地址</param>
        /// <param name="JcqCount">寄存器数量</param>
        /// <param name="XZoom">放大倍数</param>
        /// <returns></returns>
        public virtual double EM_GetKeyValue(int JcqAdd, int JcqCount, double XZoom) { return -1; }
        /// <summary>
        /// 获取ABC三相电压
        /// </summary>
        /// <returns></returns>
        public virtual  double[] EM_GetVolt() { return null; }
        /// <summary>
        /// 获取ABC三相电流
        /// </summary>
        /// <returns></returns>
        public virtual double[] EM_GetCurrent() { return null; }
        /// <summary>
        /// 获取ABC三相有功功率
        /// </summary>
        /// <returns></returns>
        public virtual double[] EM_GetPower() { return null; }
        /// <summary>
        /// 获取ABC三相功率因数
        /// </summary>
        /// <returns></returns>
        public virtual double[] EM_GetPowerFactor() { return null; }
        /// <summary>
        /// 获取ABC三相相位角
        /// </summary>
        /// <returns></returns>
        public virtual double[] EM_GetPhaseAngle() { return null; }
        /// <summary>
        /// 获取三相总有功功率
        /// </summary>
        /// <returns></returns>
        public virtual double EM_GetTotalPower() { return -1; }
        /// <summary>
        /// 获取三相总有功功率
        /// </summary>
        /// <returns></returns>
        public virtual double EM_GetTotalPower_ZH() { return -1; }

        /// <summary>
        /// 读取状态
        /// </summary>
        public virtual void Read_EMState() { }

        #endregion
    }
}
