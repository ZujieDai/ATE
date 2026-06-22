using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.EnumModel
{
    /// <summary>
    /// 设备类型
    /// </summary>
    public enum EmEquipMentType
    {
        /// <summary>
        /// 无
        /// </summary>
        Null = 0,
        /// <summary>
        /// 充电桩
        /// </summary>
        Charger = 1,
    }

    /// <summary>
    /// 命令类型
    /// </summary>
    public enum EmCMDType
    {
        /// <summary>
        /// 启动
        /// </summary>
        Start = 0,
        /// <summary>
        /// 停止
        /// </summary>
        Stop = 1,
    }


}
