using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaiTer.ATE.DataModel
{
    /// <summary>
    /// 设备程序类名对应中文名称描述
    /// </summary>
    public class EquipDescripe
    {
        /// <summary>
        /// 设备程序类名对应中文名称描述
        /// </summary>
        public Dictionary<string, string> DicEquipDescripe = new Dictionary<string, string>();

        private static EquipDescripe Instance = null;

        public static EquipDescripe GetInstance()
        {
            if (Instance == null)
                Instance = new EquipDescripe();
            return Instance;
        }
        private EquipDescripe()
        {
            DicEquipDescripe.Add("emtBMS_AC", LanguageManager.GetByKey("交流") + "BMS");
            DicEquipDescripe.Add("emtBMS_GB_DC", LanguageManager.GetByKey("国标") + LanguageManager.GetByKey("直流") + "BMS");
            DicEquipDescripe.Add("emtBMS_EU_DC", LanguageManager.GetByKey("欧标") + LanguageManager.GetByKey("直流") + "BMS");
            DicEquipDescripe.Add("emtBMS_JP_DC", LanguageManager.GetByKey("日标") + LanguageManager.GetByKey("直流") + "BMS");
            DicEquipDescripe.Add("emtBMS_USA_DC", LanguageManager.GetByKey("美标") + LanguageManager.GetByKey("直流") + "BMS");
            DicEquipDescripe.Add("emtSafety", LanguageManager.GetByKey("安规"));
            DicEquipDescripe.Add("emtSafety_SE7441", LanguageManager.GetByKey("安规") + "SE");
            DicEquipDescripe.Add("SDSOscilloscope", "SDS" + LanguageManager.GetByKey("示波器"));
            DicEquipDescripe.Add("emtTekOscilloscope", "TEK" + LanguageManager.GetByKey("示波器"));
            DicEquipDescripe.Add("DLMOscilloscope", "DLM" + LanguageManager.GetByKey("示波器"));
            DicEquipDescripe.Add("emtTekOscilloscope_MDO34", "TEK MDO34" + LanguageManager.GetByKey("示波器"));
            DicEquipDescripe.Add("emtKSOscilloscope_3000X", "KeySight" + LanguageManager.GetByKey("示波器"));
            DicEquipDescripe.Add("emtRIGOLOscilloscope_MSO5000", "RIGOL " + LanguageManager.GetByKey("示波器"));
            DicEquipDescripe.Add("emtACSource_SPWY", "SPWY" + LanguageManager.GetByKey("交流源"));
            DicEquipDescripe.Add("emtACSource_STAS", "STAS" + LanguageManager.GetByKey("交流源"));
            DicEquipDescripe.Add("emtACSource_CtrlBoard", "Ctrl" + LanguageManager.GetByKey("交流源"));
            DicEquipDescripe.Add("emtACSource_HY", "HY" + LanguageManager.GetByKey("交流源"));
            DicEquipDescripe.Add("emtACSource_XH", "XH" + LanguageManager.GetByKey("交流源"));
            DicEquipDescripe.Add("emtACSource_AN", "AN" + LanguageManager.GetByKey("交流源"));
            DicEquipDescripe.Add("emtACSource_GT", "GT" + LanguageManager.GetByKey("交流源"));
            DicEquipDescripe.Add("emtACSource_TMP", "TMP" + LanguageManager.GetByKey("交流源"));
            DicEquipDescripe.Add("emtACSource_AKSB", "AKSB" + LanguageManager.GetByKey("交流源"));
            DicEquipDescripe.Add("emtResistanceLoad_DC", LanguageManager.GetByKey("直流")  + LanguageManager.GetByKey("电阻负载"));
            DicEquipDescripe.Add("emtResistanceLoad_MultiChannel_AC", LanguageManager.GetByKey("多通道") + LanguageManager.GetByKey("电阻负载"));
            DicEquipDescripe.Add("emtResistanceLoad_MultiChannel_DC", LanguageManager.GetByKey("多通道直流") + LanguageManager.GetByKey("电阻负载"));
            DicEquipDescripe.Add("emtResistanceLoad_AC", LanguageManager.GetByKey("交流")  + LanguageManager.GetByKey("电阻负载"));
            DicEquipDescripe.Add("emtFeedbackLoad", LanguageManager.GetByKey("回馈负载"));
            DicEquipDescripe.Add("emtFeedbackLoad_DC_ST", LanguageManager.GetByKey("回馈负载"));
            DicEquipDescripe.Add("emtFeedbackLoad_YKR", LanguageManager.GetByKey("回馈负载"));
            DicEquipDescripe.Add("emtElectronicLoad", LanguageManager.GetByKey("电子负载"));
            DicEquipDescripe.Add("emtFeedbackLoad_SZHY",  LanguageManager.GetByKey("回馈负载"));
            DicEquipDescripe.Add("emtFeedbackLoad_AC", LanguageManager.GetByKey("交流回馈负载"));
            DicEquipDescripe.Add("emtControlBoard", LanguageManager.GetByKey("程控板"));
            DicEquipDescripe.Add("emtDIORelay", LanguageManager.GetByKey("继电器"));
            DicEquipDescripe.Add("emtQCLeakageCurrent", "QCLeak 剩余电流保护测试仪");
            DicEquipDescripe.Add("emtZJLeakageCurrent", "ZJ" + LanguageManager.GetByKey("剩余电流保护测试仪"));
            DicEquipDescripe.Add("emtTMLeakageCurrent", "TM" + LanguageManager.GetByKey("剩余电流保护测试仪"));
            DicEquipDescripe.Add("emtPA6500", LanguageManager.GetByKey("功率分析仪"));
            DicEquipDescripe.Add("emtWT5000", LanguageManager.GetByKey("功率分析仪"));
            DicEquipDescripe.Add("emtWT333E", LanguageManager.GetByKey("功率分析仪"));
            DicEquipDescripe.Add("emtPA6000", LanguageManager.GetByKey("功率分析仪"));
            DicEquipDescripe.Add("emtGDM9061", LanguageManager.GetByKey("GDM万用表"));
            DicEquipDescripe.Add("emtDMM6500", "DMM" + LanguageManager.GetByKey("万用表"));
            DicEquipDescripe.Add("emtWaveRecoderBoard", LanguageManager.GetByKey("录波板"));
            DicEquipDescripe.Add("emtWaveRecoderBoard30", LanguageManager.GetByKey("录波板"));
            DicEquipDescripe.Add("emtElectricMeter_DTSD336D", "YD" + LanguageManager.GetByKey("电表"));
            DicEquipDescripe.Add("emtElectricMeter_ZH4041", "ZH" + LanguageManager.GetByKey("电表"));
            DicEquipDescripe.Add("emtOscillographInstrument", ("DL录波仪"));
            DicEquipDescripe.Add("emtAuxiliaryLoadCtrl", "辅源负载");
            DicEquipDescripe.Add("emtLoopFeedbackLoad", "Loop" + LanguageManager.GetByKey("回馈负载"));
            DicEquipDescripe.Add("emtStarLoopFeedbackLoad", "Star" + LanguageManager.GetByKey("回馈负载"));
            DicEquipDescripe.Add("emtCharger_NTGX",  LanguageManager.GetByKey("充电桩模拟器"));
        }
    }
}
