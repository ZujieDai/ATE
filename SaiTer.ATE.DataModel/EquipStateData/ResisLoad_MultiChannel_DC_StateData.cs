using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.EquipStateData
{
    /// <summary>
    /// 回馈负载实时状态数据
    /// </summary>
    public class ResisLoad_MultiChannel_DC_StateData : StateDataBase
    {
        public ResisLoad_MultiChannel_DC_StateData()
        {
            EquipName = "直流电阻负载";
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
