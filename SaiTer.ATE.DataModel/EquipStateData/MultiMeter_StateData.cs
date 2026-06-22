using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.EquipStateData
{
    public class MultiMeter_StateData : StateDataBase
    {
        public MultiMeter_StateData()
        {
            EquipName = "万用表";
        }
        /// <summary>
        /// 输出电压
        /// </summary>
        public double OutPutVoltage { get; set; } 
    }
}
