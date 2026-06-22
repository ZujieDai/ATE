using SaiTer.ATE.DataModel.EnumModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel.Struct
{
    /// <summary>
    /// 单一检测项目结构
    /// </summary>
    [Serializable]
    public class StTrialItem
    {
        /// <summary>
        /// 所在方案ID
        /// </summary>
        public int SchemeID;
        /// <summary>
        /// 所在方案名称
        /// </summary>
        public string SchemeName;
        /// <summary>
        /// 试验项目名称
        /// </summary>
        public string ItemName;

        /// <summary>
        /// 试验类型
        /// </summary>
        public EmTrialType TrialType;
        /// <summary>
        /// 交流源参数
        /// </summary>
        public string SourceParams;
        /// <summary>
        /// 检测项目参数
        /// </summary>
        public string ItemParams;

        /// <summary>
        /// BMS需求电压
        /// </summary>
        public double BMSDemandVoltage;

        /// <summary>
        /// BMS需求电流
        /// </summary>
        public double BMSDemandCurrent;
        /// <summary>
        /// 电阻负载电流
        /// </summary>
        public double ResistanceLoadCurrent;

        /// <summary>
        /// 检测项顺序
        /// </summary>
        public int TrialOrder;

        /// <summary>
        /// 检测方法
        /// </summary>
        public string TrialMethod;
        /// <summary>
        /// 判定标准
        /// </summary>
        public string DecideStandard;

        /// <summary>
        /// 判定结果条件参数
        /// </summary>
        public string ResultParams { get; set; }
        /// <summary>
        /// 其他参数
        /// </summary>
        public string OtherParams { get; set; }
    }
}
