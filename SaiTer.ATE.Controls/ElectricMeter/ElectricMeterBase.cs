using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Controls
{
    public abstract class ElectricMeterBase : ControlsBase
    {
        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="JcqAdd">寄存器起始地址</param>
        /// <param name="JcqCount">寄存器数量</param>
        /// <param name="XZoom">放大倍数</param>
        /// <returns></returns>
        public virtual Dictionary<int, double> EM_GetKeyValue(List<int> lstIDs,int JcqAdd,int JcqCount,double XZoom ) { return null; }
        /// <summary>
        /// 获取ABC三相电压
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <returns></returns>
        public virtual Dictionary<int, double[]> EM_GetVolt(List<int> lstIDs) { return null; }
        /// <summary>
        /// 获取ABC三相电流
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <returns></returns>
        public virtual Dictionary<int, double[]> EM_GetCurrent(List<int> lstIDs) { return null; }
        /// <summary>
        /// 获取ABC三相有功功率
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <returns></returns>
        public virtual Dictionary<int, double[]> EM_GetPower(List<int> lstIDs) { return null; }
        /// <summary>
        /// 获取ABC三相功率因数
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <returns></returns>
        public virtual Dictionary<int, double[]> EM_GetPowerFactor(List<int> lstIDs) { return null; }
        /// <summary>
        /// 获取ABC三相相位角
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <returns></returns>
        public virtual Dictionary<int, double[]> EM_GetPhaseAngle(List<int> lstIDs) { return null; }
        /// <summary>
        /// 获取三相总有功功率
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <returns></returns>
        public virtual Dictionary<int, double> EM_GetTotalPower(List<int> lstIDs) { return null; }
        /// <summary>
        /// 获取三相总有功功率
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <returns></returns>
        public virtual Dictionary<int, double> EM_GetTotalPower_ZH(List<int> lstIDs) { return null; }



    }
}
