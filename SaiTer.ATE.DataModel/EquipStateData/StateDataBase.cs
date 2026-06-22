using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel
{
    /// <summary>
    /// 设备状态数据基类
    /// </summary>
    public class StateDataBase
    {
        /// <summary>
        /// 设备名称
        /// </summary>
        public string EquipName = string.Empty;

        /// <summary>
        /// 对应的充电枪ID
        /// </summary>
        public int ChargerID { get; set; }
    }
}
