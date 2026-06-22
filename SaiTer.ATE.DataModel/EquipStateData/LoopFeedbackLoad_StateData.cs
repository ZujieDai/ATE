using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.EquipStateData
{
    /// <summary>
    /// 手拉手环式回馈负载实时状态数据
    /// </summary>
    public class LoopFeedbackLoad_StateData : StateDataBase
    {
        public LoopFeedbackLoad_StateData()
        {
            EquipName = "回馈负载";
        }


        /// <summary>
        /// 通道1电压设定值
        /// </summary>
        public double Voltage_1 { get; set; }
        /// <summary>
        /// 通道1电流设定值
        /// </summary>
        public double Current_1 { get; set; }
        /// <summary>
        /// 通道一运行状态
        /// </summary>
        public string RunState_1 { get; set; }

        /// <summary>
        /// 通道2电压设定值
        /// </summary>
        public double Voltage_2 { get; set; }
        /// <summary>
        /// 通道2电流设定值
        /// </summary>
        public double Current_2 { get; set; }
        /// <summary>
        /// 通道2运行状态
        /// </summary>
        public string RunState_2 { get; set; }
        /// <summary>
        /// 通道3电压设定值
        /// </summary>
        public double Voltage_3 { get; set; }
        /// <summary>
        /// 通道3电流设定值
        /// </summary>
        public double Current_3 { get; set; }
        /// <summary>
        /// 通道3运行状态
        /// </summary>
        public string RunState_3 { get; set; }



        /// <summary>
        /// 通道4电压设定值
        /// </summary>
        public double Voltage_4 { get; set; }
        /// <summary>
        /// 通道4电流设定值
        /// </summary>
        public double Current_4 { get; set; }
        /// <summary>
        /// 通道4运行状态
        /// </summary>
        public string RunState_4 { get; set; }


        /// <summary>
        /// 通道5电压设定值
        /// </summary>
        public double Voltage_5 { get; set; }
        /// <summary>
        /// 通道5电流设定值
        /// </summary>
        public double Current_5 { get; set; }
        /// <summary>
        /// 通道5运行状态
        /// </summary>
        public string RunState_5 { get; set; }


        /// <summary>
        /// 通道6电压设定值
        /// </summary>
        public double Voltage_6 { get; set; }
        /// <summary>
        /// 通道6电流设定值
        /// </summary>
        public double Current_6 { get; set; }
        /// <summary>
        /// 通道6运行状态
        /// </summary>
        public string RunState_6 { get; set; }


        /// <summary>
        /// 通道7电压设定值
        /// </summary>
        public double Voltage_7 { get; set; }
        /// <summary>
        /// 通道7电流设定值
        /// </summary>
        public double Current_7 { get; set; }
        /// <summary>
        /// 通道7运行状态
        /// </summary>
        public string RunState_7 { get; set; }


        /// <summary>
        /// 通道8电压设定值
        /// </summary>
        public double Voltage_8 { get; set; }
        /// <summary>
        /// 通道8电流设定值
        /// </summary>
        public double Current_8 { get; set; }
        /// <summary>
        /// 通道8运行状态
        /// </summary>
        public string RunState_8 { get; set; }


        /// <summary>
        /// 通道9电压设定值
        /// </summary>
        public double Voltage_9 { get; set; }
        /// <summary>
        /// 通道9电流设定值
        /// </summary>
        public double Current_9 { get; set; }
        /// <summary>
        /// 通道9运行状态
        /// </summary>
        public string RunState_9 { get; set; }


        /// <summary>
        /// 通道10电压设定值
        /// </summary>
        public double Voltage_10 { get; set; }
        /// <summary>
        /// 通道10电流设定值
        /// </summary>
        public double Current_10 { get; set; }
        /// <summary>
        /// 通道10运行状态
        /// </summary>
        public string RunState_10 { get; set; }


        /// <summary>
        /// 通道11电压设定值
        /// </summary>
        public double Voltage_11 { get; set; }
        /// <summary>
        /// 通道11电流设定值
        /// </summary>
        public double Current_11 { get; set; }
        /// <summary>
        /// 通道11运行状态
        /// </summary>
        public string RunState_11 { get; set; }


        /// <summary>
        /// 通道12电压设定值
        /// </summary>
        public double Voltage_12 { get; set; }
        /// <summary>
        /// 通道12电流设定值
        /// </summary>
        public double Current_12 { get; set; }
        /// <summary>
        /// 通道12运行状态
        /// </summary>
        public string RunState_12 { get; set; }


        /// <summary>
        /// 通道13电压设定值
        /// </summary>
        public double Voltage_13 { get; set; }
        /// <summary>
        /// 通道13电流设定值
        /// </summary>
        public double Current_13 { get; set; }
        /// <summary>
        /// 通道13运行状态
        /// </summary>
        public string RunState_13 { get; set; }


        /// <summary>
        /// 通道14电压设定值
        /// </summary>
        public double Voltage_14 { get; set; }
        /// <summary>
        /// 通道14电流设定值
        /// </summary>
        public double Current_14 { get; set; }
        /// <summary>
        /// 通道14运行状态
        /// </summary>
        public string RunState_14 { get; set; }


        /// <summary>
        /// 通道15电压设定值
        /// </summary>
        public double Voltage_15 { get; set; }
        /// <summary>
        /// 通道15电流设定值
        /// </summary>
        public double Current_15 { get; set; }
        /// <summary>
        /// 通道15运行状态
        /// </summary>
        public string RunState_15 { get; set; }


        /// <summary>
        /// 通道16电压设定值
        /// </summary>
        public double Voltage_16 { get; set; }
        /// <summary>
        /// 通道16电流设定值
        /// </summary>
        public double Current_16 { get; set; }
        /// <summary>
        /// 通道16运行状态
        /// </summary>
        public string RunState_16 { get; set; }

        /// <summary>
        /// 并机开关1状态 (0-未并机， 1-并机中)
        /// </summary>
        public int Parallet_1 { get; set; }
        public int Parallet_2 { get; set; }
        public int Parallet_3 { get; set; }
        public int Parallet_4 { get; set; }
        public int Parallet_5 { get; set; }
        public int Parallet_6 { get; set; }
        public int Parallet_7 { get; set; }
        public int Parallet_8 { get; set; }
        public int Parallet_9 { get; set; }
        public int Parallet_10 { get; set; }
        public int Parallet_11 { get; set; }
        public int Parallet_12 { get; set; }
        public int Parallet_13 { get; set; }
        public int Parallet_14 { get; set; }
        public int Parallet_15 { get; set; }
        public int Parallet_16 { get; set; }




    }
}
