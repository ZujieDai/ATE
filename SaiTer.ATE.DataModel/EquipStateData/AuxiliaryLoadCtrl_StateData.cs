using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.EquipStateData
{
    /// <summary>
    /// 辅源负载（程控板控制）实时状态数据
    /// </summary>
    public class AuxiliaryLoadCtrl_StateData : StateDataBase
    {
        /// <summary>
        /// 12V过压状态
        /// </summary>
        public string VoltOver_12V { get; set; }

        /// <summary>
        /// 24V过压状态
        /// </summary>
        public string VoltOver_24V { get; set; }

        /// <summary>
        /// 辅源短路状态
        /// </summary>
        public string ShortCircuite { get; set; }

        

        /// <summary>
        /// 辅源12V电流值
        /// </summary>
        public int AuxiCurrent_12V { get; set; }

        /// <summary>
        /// 辅源24V电流值
        /// </summary>
        public int AuxiCurrent_24V { get; set; }


        public AuxiliaryLoadCtrl_StateData()
        {
            EquipName = "辅源负载";
        }
    }
}
