using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Controls
{
    /// <summary>
    /// 手拉手环式回馈负载控制
    /// </summary>
    public abstract class LoopFeedbackLoadBase : ControlsBase
    {
        /// <summary>
        /// 启动负载输出
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        /// <param name="channel">通道号</param>
        public virtual void LoopFeedbackLoad_ON(List<int> lstIDs,int channel) { }
        /// <summary>
        /// 关闭负载
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        /// <param name="channel">通道号</param>
        public virtual void LoopFeedbackLoad_OFF(List<int> lstIDs, int channel) { }
        /// <summary>
        /// 设置负载需求电压电流
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        /// <param name="voltage">需求电压</param>
        /// <param name="channel">通道号</param>
        public virtual void SetLoopFeedbackLoadParams(List<int> lstIDs, int channel, double voltage, double current) { }

        /// <summary>
        /// 取消并机
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="channel">通道号</param>
        public virtual void LoopFeedbackLoad_NoParallel(List<int> lstIDs, int channel) { }

        /// <summary>
        /// 并机
        /// </summary>
        /// <param name="lstIDs"></param>
        /// <param name="channel">通道号</param>
        public virtual void LoopFeedbackLoad_Parallel(List<int> lstIDs, int channel) { }
    }
}
