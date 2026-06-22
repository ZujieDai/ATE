using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.EnumModel
{
    /// <summary>
    /// 检测结果枚举
    /// </summary>
    public enum EmTrialResult
    {
        /// <summary>
        /// 待测
        /// </summary>
        Wait,
        /// <summary>
        /// 合格-通过
        /// </summary>
        Pass,
        /// <summary>
        /// 失败-不通过
        /// </summary>
        Fail,
        /// <summary>
        /// 测试中
        /// </summary>
        Testing,
        /// <summary>
        /// 不判断
        /// </summary>
        NA
    }
}
