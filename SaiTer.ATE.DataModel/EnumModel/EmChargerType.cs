using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.EnumModel
{
    /// <summary>
    /// 充电枪类型
    /// </summary>
    public enum EmChargerType
    {
        NULL=0,
        /// <summary>
        /// 国标直流充电枪
        /// </summary>
        [Description("国标直流充电枪")]
        Charger_GB_DC = 1,
        /// <summary>
        /// 国标交流充电枪
        /// </summary>
        [Description("国标交流充电枪")]
        Charger_GB_AC = 2,
        /// <summary>
        /// 欧标直流充电枪
        /// </summary>
        [Description("欧标直流充电枪")]
        Charger_EUR_DC = 3,
        /// <summary>
        /// 欧标交流充电枪
        /// </summary>
        [Description("欧标交流充电枪")]
        Charger_EUR_AC = 4,
        /// <summary>
        /// 美标直流充电枪
        /// </summary>
        [Description("美标直流充电枪")]
        Charger_USA_DC = 5,
        /// <summary>
        /// 美标交流充电枪
        /// </summary>
        [Description("美标交流充电枪")]
        Charger_USA_AC = 6,
        /// <summary>
        /// 日标直流充电枪
        /// </summary>
        [Description("日标直流充电枪")]
        Charger_JP_DC = 7,
        /// <summary>
        /// 北美充电标准直流（特斯拉接口）
        /// </summary>
        [Description("特斯拉直流充电枪")]
        Charger_NACS_DC = 8,
        /// <summary>
        /// 北美充电标准直流（特斯拉接口）
        /// </summary>
        [Description("特斯拉交流充电枪")]
        Charger_NACS_AC = 9,
        /// <summary>
        /// 北美充电标准直流（特斯拉接口）
        /// </summary>
        [Description("储能测试")]
        Charger_NTGX_CN = 10,

    }
}
