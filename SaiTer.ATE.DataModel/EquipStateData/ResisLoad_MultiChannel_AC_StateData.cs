using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.EquipStateData
{
    public class ResisLoad_MultiChannel_AC_StateData : StateDataBase
    {
        public ResisLoad_MultiChannel_AC_StateData()
        {
            EquipName = "电阻负载";
        }
        /// <summary>
        /// 通道号
        /// </summary>
        public int Chanel { get; set; }

        /// <summary>
        /// 通道电压设定值
        /// </summary>
        public double Voltage { get; set; }
        /// <summary>
        /// 通道电流设定值
        /// </summary>
        public double Current { get; set; }


    }
}
