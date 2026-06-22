using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Controls
{
    public abstract class ResistanceLoadBase : ControlsBase
    {
        /// <summary>
        /// 启动负载输出
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public virtual void ResistanceLoad_ON(List<int> lstIDs) { }
        /// <summary>
        /// 关闭负载
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public virtual void ResistanceLoad_OFF(List<int> lstIDs) { }
        /// <summary>
        /// 取消并机
        /// </summary>
        /// <param name="lstIDs"></param>
        public virtual void ResistanceLoad_NoParallel(List<int> lstIDs) { }

        /// <summary>
        /// 并机
        /// </summary>
        /// <param name="lstIDs"></param>
        public virtual void ResistanceLoad_Parallel(List<int> lstIDs) { }

        /// <summary>
        /// 设置负载需求电压电流
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        /// <param name="voltage">需求电压</param>
        public virtual void SetResisLoadVolCurr(List<int> lstIDs, double voltage, double current) { }

        ///// <summary>
        ///// 设置负载需求电流
        ///// </summary>
        ///// <param name="lstIDs">枪编号集合</param>
        ///// <param name="voltage">需求电流</param>
        //public virtual void SetResisLoadCurrent(List<int> lstIDs, double current) { }

        /// <summary>
        /// 设置负载需求功率
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        /// <param name="voltage">需求电压</param>
        /// <param name="power">需求功率</param>
        public virtual void SetResisLoadPower(List<int> lstIDs, double volt, double power) { }
    }
}
