using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.Struct
{
    /// <summary>
    /// 充电枪待检状态
    /// </summary>
    public struct StChargerCheckState
    {
        /// <summary>
        /// 充电枪编号
        /// </summary>
        public int ChargerID;

        /// <summary>
        /// 是否要检测
        /// </summary>
        public bool IsCheck;
    }
}
