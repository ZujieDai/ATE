using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.EquipStateData
{
    /// <summary>
    /// 交流源实时状态
    /// </summary>
    public class ACSource_StateData : StateDataBase
    {
        public ACSource_StateData()
        { 
            EquipName = "交流源";
        }
        /// <summary>
        /// 频率
        /// </summary>
        public float Freq { get; set; }
        public float Volt { get; set; }

        public float Volt_B { get; set; }

        public float Volt_C { get; set; }

        public float Current { get; set; }
        public float Current_B { get; set; }
        public float Current_C { get; set; }
        /// <summary>
        /// 交流源型号
        /// </summary>
        public string ACSourceName { get; set; }
    }
}
