using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.EnumModel
{
    /// <summary>
    /// 检测状态枚举
    /// </summary>
    public enum EmTrialState
    {
        /// <summary>
        /// 待测
        /// </summary>
        Wait,
        /// <summary>
        /// 测试开始
        /// </summary>
        Start,
        /// <summary>
        /// 测试中
        /// </summary>
        Testing,
        /// <summary>
        /// 测试结束
        /// </summary>
        End
    }
}
