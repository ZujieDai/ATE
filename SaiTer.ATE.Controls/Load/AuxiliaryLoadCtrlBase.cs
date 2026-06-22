using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Controls
{
    /// <summary>
    /// 辅源负载（程控板）控制类
    /// </summary>
    public class AuxiliaryLoadCtrlBase : ControlsBase
    {
        /// <summary>
        /// 取消所有状态
        /// </summary>
        /// <param name="lstIDs"></param>
        public virtual void CancelAllState(List<int> lstIDs) { }
        /// <summary>
        /// 设置12V辅源过压
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public virtual void Set12VoltOver(List<int> lstIDs) { }
        /// <summary>
        /// 设置24V辅源过压
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public virtual void Set24VoltOver(List<int> lstIDs) { }

        /// <summary>
        /// 设置辅源短路
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public virtual void SetShortCircuite(List<int> lstIDs) { }

        /// <summary>
        /// 设置12V辅源电流参数(1-16A范围，步进1A)
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        /// <param name="current">电流值(1-16A范围，步进1A)</param>
        public virtual void Set12VCurrent(List<int> lstIDs, int current) { }

        /// <summary>
        /// 设置24V辅源电流参数（2-14A范围，步进2A）
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        /// <param name="current">电流值（2-14A范围，步进2A）</param>
        public virtual void Set24VCurrent(List<int> lstIDs, int current) { }
    }
}
