using SaiTer.ATE.DataModel.EquipStateData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel
{
    /// <summary>
    /// 所有设备的实时状态数据
    /// </summary>
    public class AllEquipStateData
    {
        private static AllEquipStateData instance = null;

        /// <summary>
        /// 所有设备的实时状态数据
        /// </summary>
        public static AllEquipStateData GetInstance()
        {
            if (instance == null)
            {
                instance = new AllEquipStateData();
            }
            return instance;
        }

        /// <summary>
        /// 直流BMS实时数据集合（桩ID-BMS数据）
        /// </summary>
        public static Dictionary<int, BMS_DC_StateData> DicBMS_DC_StateData { get; set; } = new Dictionary<int, BMS_DC_StateData>();

        /// <summary>
        /// 直流BMS实时数据集合（桩ID-BMS数据）
        /// </summary>
        public static Dictionary<int, BMS_EU_DC_StateData> DicBMS_EU_DC_StateData { get; set; } = new Dictionary<int, BMS_EU_DC_StateData>();

        /// <summary>
        /// 美标直流BMS实时数据集合（桩ID-BMS数据）
        /// </summary>
        public static Dictionary<int, BMS_USA_DC_StateData> DicBMS_USA_DC_StateData { get; set; } = new Dictionary<int, BMS_USA_DC_StateData>();

        /// <summary>
        /// 日标直流BMS实时数据集合（桩ID-BMS数据）
        /// </summary>
        public static Dictionary<int, BMS_JP_DC_StateData> DicBMS_JP_DC_StateData { get; set; } = new Dictionary<int, BMS_JP_DC_StateData>();

        /// <summary>
        /// 交流BMS实时数据集合（桩ID-BMS数据）
        /// </summary>
        public static Dictionary<int, BMS_AC_StateData> DicBMS_AC_StateData { get; set; } = new Dictionary<int, BMS_AC_StateData>();


        /// <summary>
        /// 鼎阳示波器实时数据集合(桩ID-示波器数据)
        /// /// </summary>
        public static Dictionary<int, SDSOscilloscope_StateData> DicSDSOscilloscope_StateData { get; set; } = new Dictionary<int, SDSOscilloscope_StateData>();


        /// <summary>
        /// 交流源实时数据集合(桩ID-交流源数据)
        /// </summary>
        public static Dictionary<int, ACSource_StateData> DicACSource_StateData { get; set; } = new Dictionary<int, ACSource_StateData>();


        /// <summary>
        /// 电阻负载实时数据集合(桩ID-电阻负载数据)
        /// </summary>
        public static Dictionary<int, ResisLoad_StateData> DicResisLoad_StateData { get; set; } = new Dictionary<int, ResisLoad_StateData>();

        /// <summary>
        /// 多通道交流电阻负载实时数据集合(桩ID-电阻负载数据)
        /// </summary>
        public static Dictionary<int, ResisLoad_MultiChannel_AC_StateData> DicResisLoad_MultiChannel_AC_StateData { get; set; } = new Dictionary<int, ResisLoad_MultiChannel_AC_StateData>();

        /// <summary>
        /// 多通道直流电阻负载实时数据集合(桩ID-电阻负载数据)
        /// </summary>
        public static Dictionary<int, ResisLoad_MultiChannel_DC_StateData> DicResisLoad_MultiChannel_DC_StateData { get; set; } = new Dictionary<int, ResisLoad_MultiChannel_DC_StateData>();

        /// <summary>
        /// 中佳剩余电流保护测试仪(桩ID-测试仪数据)
        /// </summary>
        public static Dictionary<int, ZJLeakageCurrent_StateData> DicZJLeakageCurrent_StateData { get; set; } = new Dictionary<int, ZJLeakageCurrent_StateData>();



        /// <summary>
        /// QC齐充新能源-剩余电流保护测试仪(桩ID-测试仪数据)
        /// </summary>
        public static Dictionary<int, QCLeakageCurrent_StateData> DicQCLeakageCurrent_StateData { get; set; } = new Dictionary<int, QCLeakageCurrent_StateData>();

        /// <summary>
        /// 功率分析仪实时数据集合(桩ID-功率分析仪数据)
        /// </summary>
        public static Dictionary<int, PowerAnalyzer_StateData> DicPowerAnalyzer_StateData { get; set; } = new Dictionary<int, PowerAnalyzer_StateData>();


        /// <summary>
        /// 万用表实时数据集合(桩ID-功率分析仪数据)
        /// </summary>
        public static Dictionary<int, MultiMeter_StateData> MultiMeter_StateData { get; set; } = new Dictionary<int, MultiMeter_StateData>();

        /// <summary>
        /// 电子负载实时数据集合(桩ID-电子负载数据)
        /// </summary>
        public static Dictionary<int ,ElectronicLoad_StateData> DicElectronicLoad_StateData { get; set; } = new Dictionary<int, ElectronicLoad_StateData>();

        /// <summary>
        /// 回馈负载实时数据集合(桩ID-回馈负载数据)
        /// </summary>
        public static Dictionary<int, FeedbackLoad_StateData> DicFeedbackLoad_StateData { get; set; } = new Dictionary<int, FeedbackLoad_StateData>();

        /// <summary>
        /// 交流回馈负载实时数据集合(桩ID-回馈负载数据)
        /// </summary>
        public static Dictionary<int, FeedbackLoadAC_StateData> DicFeedbackLoadAC_StateData { get; set; } = new Dictionary<int, FeedbackLoadAC_StateData>();

        /// <summary>
        /// 手拉手环式回馈负载实时数据集合(桩ID-回馈负载数据)
        /// </summary>
        public static Dictionary<int, LoopFeedbackLoad_StateData> DicLoopFeedbackLoad_StateData { get; set; } = new Dictionary<int, LoopFeedbackLoad_StateData>();

        /// <summary>
        /// 辅源负载（程控板）实时数据集合(桩ID-辅源负载数据)
        /// </summary>
        public static Dictionary<int, AuxiliaryLoadCtrl_StateData> DicAuxiliaryLoadCtrl_StateData { get; set; } = new Dictionary<int, AuxiliaryLoadCtrl_StateData>();

        /// <summary>
        /// 桩模拟器实时数据集合(桩ID-桩模拟器数据)
        /// </summary>
        public static Dictionary<int, Charger_NTGX_StateData> DicChargerNTGXCtrl_StateData { get; set; } = new Dictionary<int, Charger_NTGX_StateData>();
    }
}
