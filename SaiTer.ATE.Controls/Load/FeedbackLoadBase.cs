using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.Controls
{
    /// <summary>
    /// 回馈负载控制
    /// </summary>
    public abstract class FeedbackLoadBase : ControlsBase
    {
        /// <summary>
        /// 模拟BMS启动充电
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public virtual void FeedbackLoad_BMSON(List<int> lstIDs) { }
        /// <summary>
        /// 模拟BMS结束充电
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public virtual void FeedbackLoad_BMSOFF(List<int> lstIDs) { }
        /// <summary>
        /// 启动负载输出
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public virtual void FeedbackLoad_ON(List<int> lstIDs) { }
        /// <summary>
        /// 关闭负载
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        public virtual void FeedbackLoad_OFF(List<int> lstIDs) { }
        /// <summary>
        /// 设置负载需求电压电流
        /// </summary>
        /// <param name="lstIDs">枪编号集合</param>
        /// <param name="voltage">需求电压</param>
        public virtual void SetFeedbackLoadParams(List<int> lstIDs, double voltage, double current) { }

        /// <summary>
        /// 取消并机
        /// </summary>
        /// <param name="lstIDs"></param>
        public virtual void FeedbackLoad_NoParallel(List<int> lstIDs) { }

        /// <summary>
        /// 并机
        /// </summary>
        /// <param name="lstIDs"></param>
        public virtual void FeedbackLoad_Parallel(List<int> lstIDs) { }

        /// <summary>
        /// BMS启动（只有深圳HY的项目有这个设置）
        /// </summary>
        /// <param name="lstIDs"></param>
        public virtual void FeedbackLoad_BMS_ON(List<int> lstIDs) { }
    }
}
