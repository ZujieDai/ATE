using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Controls
{
    public abstract class ACSourceBase : ControlsBase
    {
        /// <summary>
        /// 启动交流源
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public virtual void ACSource_ON(List<int> lstIDs) { }
        /// <summary>
        /// 关闭交流源
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public virtual void ACSource_OFF(List<int> lstIDs) { }
        /// <summary>
        /// 设置交流源电压
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        /// <param name="voltage">电压值</param>
        public virtual void ACSource_SetVolt(List<int> lstIDs, double voltage) { }
        /// <summary>
        /// 设置交流源频率
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="freq"></param>
        public virtual void ACSource_SetFreq(List<int> lstIDs, double freq) { }

        /// <summary>
        /// 交流源断开远程控制
        /// </summary>
        /// <param name="lstIDs"></param>
        public virtual void ACSource_DisConnect(List<int> lstIDs) { }
        /// <summary>
        /// 设置三相电压
        /// </summary>
        /// <param name="VoltA">电压A</param>
        /// <param name="VoltB">电压B</param>
        /// <param name="VoltC">电压C</param>
        public virtual void ACSource_SetVolt3(List<int> lstIDs,double VoltA, double VoltB, double VoltC) { }

        /// <summary>
        /// 设置三相相位角
        /// </summary>
        /// <param name="AngleA">A相相位角</param>
        /// <param name="AngleB">B相相位角</param>
        /// <param name="AngleC">C相相位角</param>
        public virtual void ACSource_SetAngle3(List<int> lstIDs,double AngleA, double AngleB, double AngleC) { }

        /// <summary>
        /// 设置缺相电压
        /// </summary>
        public virtual void ACSource_SetOpenPhase(List<int> lstIDs) { }
    }
}
