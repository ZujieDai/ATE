using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel
{
    /// <summary>
    /// 统计信息模型
    /// </summary>
    public class StatisticsModel
    {
        /// <summary>
        /// 测试总数
        /// </summary>
        public int TestCount { get; set; }
        /// <summary>
        /// 通过数
        /// </summary>
        public int PassCount { get; set; }
        /// <summary>
        /// 失败数
        /// </summary>
        public int FailCount { get; set; }
        /// <summary>
        /// 合格率
        /// </summary>
        public double PassRate { get; set; }
        /// <summary>
        /// 不合格率
        /// </summary>
        public double FailRate { get; set; }
        /// <summary>
        /// 统计描述信息
        /// </summary>
        public string StatisticsDescriptionInfo { get; set; }
    }
}
