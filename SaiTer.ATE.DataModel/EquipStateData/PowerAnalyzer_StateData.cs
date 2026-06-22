using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.EquipStateData
{
    /// <summary>
    /// 功率分析仪监控数据
    /// </summary>
    public class PowerAnalyzer_StateData : StateDataBase
    {
        public PowerAnalyzer_StateData() { EquipName = "功率分析仪"; }

        /// <summary>
        /// 效率值η1
        /// </summary>
        public double Efficiency { get; set; }

        /// <summary>
        /// 通道1 均方根电压
        /// </summary>
        public double Channel1RMSVolt { get; set; }

        /// <summary>
        /// 通道1 均方根电流
        /// </summary>
        public double Channel1RMSCurrent { get; set; }


        /// <summary>
        /// 通道1 功率
        /// </summary>
        public double Channel1Power { get; set; }

        /// <summary>
        /// 通道1 功率因素
        /// </summary>
        public double Channel1PowerFactor { get; set; }

        /// <summary>
        /// 通道1 无功功率
        /// </summary>
        public double Channel1ReactivePower { get; set; }

        /// <summary>
        /// 通道1 电能量
        /// </summary>
        public double Channel1ElectricEnergy { get; set; }

        /// <summary>
        /// 通道2 均方根电压
        /// </summary>
        public double Channel2RMSVolt { get; set; }

        /// <summary>
        /// 通道2 均方根电流
        /// </summary>
        public double Channel2RMSCurrent { get; set; }


        /// <summary>
        /// 通道2 功率
        /// </summary>
        public double Channel2Power { get; set; }

        /// <summary>
        /// 通道2 功率因素
        /// </summary>
        public double Channel2PowerFactor { get; set; }

        /// <summary>
        /// 通道2 无功功率
        /// </summary>
        public double Channel2ReactivePower { get; set; }

        /// <summary>
        /// 通道2 电能量
        /// </summary>
        public double Channel2ElectricEnergy { get; set; }

        /// <summary>
        /// 通道3 均方根电压
        /// </summary>
        public double Channel3RMSVolt { get; set; }

        /// <summary>
        /// 通道 3均方根电流
        /// </summary>
        public double Channel3RMSCurrent { get; set; }


        /// <summary>
        /// 通道3 功率
        /// </summary>
        public double Channel3Power { get; set; }

        /// <summary>
        /// 通道 3功率因素
        /// </summary>
        public double Channel3PowerFactor { get; set; }

        /// <summary>
        /// 通道3 无功功率
        /// </summary>
        public double Channel3ReactivePower { get; set; }

        /// <summary>
        /// 通道3 电能量
        /// </summary>
        public double Channel3ElectricEnergy { get; set; }

        /// <summary>
        /// 通道4 均方根电压
        /// </summary>
        public double Channel4RMSVolt { get; set; }

        /// <summary>
        /// 通道4 均方根电流
        /// </summary>
        public double Channel4RMSCurrent { get; set; }


        /// <summary>
        /// 通道4 功率
        /// </summary>
        public double Channel4Power { get; set; }

        /// <summary>
        /// 通道4 功率因素
        /// </summary>
        public double Channel4PowerFactor { get; set; }

        /// <summary>
        /// 通道4 无功功率
        /// </summary>
        public double Channel4ReactivePower { get; set; }

        /// <summary>
        /// 通道5 均方根电压
        /// </summary>
        public double Channel5RMSVolt { get; set; }

        /// <summary>
        /// 通道5 均方根电流
        /// </summary>
        public double Channel5RMSCurrent { get; set; }

        /// <summary>
        /// 通道5 功率
        /// </summary>
        public double Channel5Power { get; set; }

        /// <summary>
        /// 通道5 功率因素
        /// </summary>
        public double Channel5PowerFactor { get; set; }

        /// <summary>
        /// 通道5 无功功率
        /// </summary>
        public double Channel5ReactivePower { get; set; }

        /// <summary>
        /// 通道6 均方根电压
        /// </summary>
        public double Channel6RMSVolt { get; set; }

        /// <summary>
        /// 通道6 均方根电流
        /// </summary>
        public double Channel6RMSCurrent { get; set; }

        /// <summary>
        /// 通道6 功率
        /// </summary>
        public double Channel6Power { get; set; }

        /// <summary>
        /// 通道6 功率因素
        /// </summary>
        public double Channel6PowerFactor { get; set; }

        /// <summary>
        /// 通道6 无功功率
        /// </summary>
        public double Channel6ReactivePower { get; set; }


        /// <summary>
        /// ΣA组三相总电压值
        /// </summary>
        public double TotalVoltage { get; set; }

        /// <summary>
        /// ΣA组三相总电流值
        /// </summary>
        public double TotalCurrent { get; set; }

        /// <summary>
        /// ΣA组三相总功率
        /// </summary>
        public double TotalPower { get; set; }

        /// <summary>
        /// ΣA组三相功率因素
        /// </summary>
        public double TotalPowerFactor { get; set; }
    }
}
