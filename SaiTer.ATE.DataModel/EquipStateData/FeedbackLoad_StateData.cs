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
    public class FeedbackLoad_StateData : StateDataBase
    {
        public FeedbackLoad_StateData()
        {
            EquipName = "回馈负载";
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

        ///// <summary>
        ///// 通道2电压设定值
        ///// </summary>
        //public double Voltage_2 { get; set; }
        ///// <summary>
        ///// 通道2电流设定值
        ///// </summary>
        //public double Current_2 { get; set; }

        ///// <summary>
        ///// 通道3电压设定值
        ///// </summary>
        //public double Voltage_3 { get; set; }
        ///// <summary>
        ///// 通道3电流设定值
        ///// </summary>
        //public double Current_3 { get; set; }

    }
}
